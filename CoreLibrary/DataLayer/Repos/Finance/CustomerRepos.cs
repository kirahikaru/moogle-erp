using DataLayer.GlobalConstant;
using DataLayer.Models.Finance;
using DataLayer.Models.SystemCore.NonPersistent;
using System.Text.RegularExpressions;
using static Dapper.SqlMapper;

namespace DataLayer.Repos.Finance;

public interface ICustomerRepos : IBaseRepos<Customer>
{
	Task<int> InsertFullAsync(Customer obj, UserSessionInfo user, string endStatus = "", string? remark = null);

	Task<bool> UpdateFullAsync(Customer obj, UserSessionInfo user, string endStatus = "", string? remark = null);

	Task<Customer?> GetFullAsync(int id);

	Task<List<Customer>> QuickSearchAsync(bool isSearchIndividual, int pgSize = 0, int pgNo = 0, string? searchText = null,
		Dictionary<string, bool>? fieldRequiredToHaveValues = null,
		List<int>? excludeIdList = null);

	Task<DataPagination> GetQuickSearchPaginationAsync(bool isSearchIndividual, int pgSize = 0, string? searchText = null,
		Dictionary<string, bool>? fieldRequiredToHaveValues = null,
		List<int>? excludeIdList = null);

	Task<List<Customer>> SearchAsync(
		bool isSearchIndividual,
		int pgSize = 0,
		int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		string? objectNameKh = null,
		DateTime? startDateFrom = null,
		DateTime? startDateTo = null,
		string? gender = null,
		string? idNum = null,
		List<string>? countryList = null,
		string? customerStatus = null,
		Dictionary<string, bool>? fieldRequiredToHaveValues = null,
		List<int>? excludeIdList = null);

	Task<DataPagination> GetSearchPaginationAsync(
		bool isSearchIndividual,
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		string? objectNameKh = null,
		DateTime? startDateFrom = null,
		DateTime? startDateTo = null,
		string? gender = null,
		string? idNum = null,
		List<string>? countryList = null,
		string? customerStatus = null,
		Dictionary<string, bool>? fieldRequiredToHaveValues = null,
		List<int>? excludeIdList = null);
}

public class CustomerRepos(IConnectionFactory connectionFactory) : BaseRepos<Customer>(connectionFactory, Customer.DatabaseObject), ICustomerRepos
{
	public async Task<Customer?> GetFullAsync(int id)
    {
        string sql = $"SELECT * FROM {DbObject.MsSqlTable} t WHERE t.IsDeleted=0 AND t.Id=@Id";

        using var cn = ConnectionFactory.GetDbConnection()!;

        Customer? obj = await cn.QueryFirstOrDefaultAsync<Customer>(sql, new { Id = id });

        if (obj != null)
        {
			SqlBuilder sbSql = new();
			if (obj.CustomerType == CustomerTypes.INDIVIDUAL)
            {
                sbSql.Where("t.IsDeleted=0");
                sbSql.Where("t.Id=@PersonId");

                sbSql.LeftJoin($"{Country.MsSqlTable} natCty ON natCty.IsDeleted=0 AND natCty.ObjectCode=t.NationalityCountryCode");

                string personSql = sbSql.AddTemplate($"SELECT * FROM {Person.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

                obj.Person = (await cn.QueryAsync<Person, Country, Person>(personSql, (p, natCty) =>
                {
                    p.Nationality = natCty;
                    return p;
                }, new { PersonId = obj!.PersonId!.Value }, splitOn:"Id")).FirstOrDefault();

            }
            else
            {
				sbSql.Where("t.IsDeleted=0");
				sbSql.Where("t.Id=@BusinessEntityId");

				sbSql.LeftJoin($"{Industry.MsSqlTable} i ON i.IsDeleted=0 AND i.Id=t.IndustryId");
				sbSql.LeftJoin($"{BusinessSector.MsSqlTable} bs ON bs.IsDeleted=0 AND bs.Id=t.BusinessSectorId");
				sbSql.LeftJoin($"{Country.MsSqlTable} baseCty ON baseCty.IsDeleted=0 AND baseCty.ObjectCode=t.BaseCountryCode");

				string businessEntitySql = sbSql.AddTemplate($"SELECT * FROM {BusinessEntity.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

				obj.BusinessEntity = (await cn.QueryAsync<BusinessEntity, Industry, BusinessSector, Country, BusinessEntity>(businessEntitySql, (busnEnt, industry, sector, country) =>
				{
                    busnEnt.Industry = industry;
                    busnEnt.BusinessSector = sector;
                    busnEnt.BaseCountry = country;
					return busnEnt;

				}, new { BusinessEntityId = obj!.BusinessEntityId!.Value }, splitOn: "Id")).FirstOrDefault();
			}

            return obj;
        }
        else
            return null;
    }

    public async Task<int> InsertFullAsync(Customer obj, UserSessionInfo user, string endStatus= "", string? remark = null)
    {
        using var cn = ConnectionFactory.GetDbConnection()!;
        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();
        using var tran = cn.BeginTransaction();

        try
        {
            //if (endStatus.Length > 0 && endStatus != CustomerStatuses.PENDING_ACTIVATION && string.IsNullOrEmpty(obj.ObjectCode))
            //    throw new Exception("Customer.ObjectCode is required for EndStatus other Pending Activation.");

            if (endStatus == CustomerStatuses.ACTIVE && string.IsNullOrEmpty(obj.ObjectCode))
            {
                obj.ObjectCode = await GetRunningNumberAsync(cn, new RunNumGenParam()
                {
                    UserName = obj.ModifiedUser,
                    ObjectClassName = obj.GetType().Name
                }, tran);

                if (string.IsNullOrEmpty(obj.ObjectCode))
                    throw new Exception("Cannot get ObjectCode from Running Number Generator.");
            }

            if (obj.PersonId.HasValue)
            {
                if (obj.Person != null)
                {
                    obj.Person.ModifiedUser = obj.ModifiedUser;
                    obj.Person.ModifiedDateTime = obj.ModifiedDateTime;
                    bool isPersonUpd = await cn.UpdateAsync(obj.Person, tran);
                }
                else
                    throw new Exception("Person link object is null");
            }
            else if (obj.Person != null)
            {
                obj.Person.ObjectName = obj.ObjectName;
                obj.Person.ObjectNameKh = obj.ObjectNameKh;
                obj.Person.CreatedUser = obj.CreatedUser;
                obj.Person.CreatedDateTime = obj.CreatedDateTime;
                obj.Person.ModifiedUser = obj.ModifiedUser;
                obj.Person.ModifiedDateTime = obj.ModifiedDateTime;

                int personId = await cn.InsertAsync(obj.Person, tran);

                if (personId > 0)
                    obj.PersonId = personId;
                else
                    throw new Exception("Failed to insert person.");
            }

            if (obj.BusinessEntityId.HasValue)
            {
                if (obj.BusinessEntity != null)
                {
                    obj.BusinessEntity.ModifiedUser = obj.ModifiedUser;
                    obj.BusinessEntity.ModifiedDateTime = obj.ModifiedDateTime;
                    bool isBusnEntUpd = await cn.UpdateAsync(obj.BusinessEntity, tran);
                }
                else
                    throw new Exception("Business Entity link object is null.");
            }
            else if (obj.BusinessEntity != null)
            {
                obj.BusinessEntity.CreatedUser = obj.CreatedUser;
                obj.BusinessEntity.CreatedDateTime = obj.CreatedDateTime;
                obj.BusinessEntity.ModifiedUser = obj.ModifiedUser;
                obj.BusinessEntity.ModifiedDateTime = obj.ModifiedDateTime;

                int busnEntId = await cn.InsertAsync(obj.BusinessEntity, tran);

                if (busnEntId > 0)
                    obj.BusinessEntityId = busnEntId;
                else
                    throw new Exception("Failed to insert business entity.");
            }

            int objId;

            objId = await cn.InsertAsync(obj, tran);

            if (objId > 0)
            {
                ObjectStatusAuditTrail statusChgAudit = new()
                {
                    ObjectCode = obj.ObjectCode,
                    ObjectName = obj.GetType().Name,
                    ObjectId = objId,
                    ActionCode = "REGISTER",
                    FromStatus = "",
                    ToStatus = obj.Status,
                    Remark = remark,
                    TriggeredUserId = user.UserId,
                    CreatedDateTime = obj.CreatedDateTime,
                    CreatedUser = user.UserName,
                    ModifiedDateTime = obj.ModifiedDateTime,
                    ModifiedUser = obj.ModifiedUser
                };

                int statusChgAuditId = await cn.InsertAsync(statusChgAudit, tran);

                if (statusChgAuditId <= 0)
                    throw new Exception("Failed to inster Customer Change History ('Register').");

                if (endStatus.Length > 0 && endStatus == CustomerStatuses.ACTIVE)
                {
                    #region CUSOMTER ACTIVATION
                    string updCustomerCmd = $"UPDATE {Customer.MsSqlTable} SET [Status]=@Status WHERE Id=@Id";

                    DynamicParameters updCustomerParam = new();
                    updCustomerParam.Add("@Status", endStatus, DbType.AnsiString);
                    updCustomerParam.Add("@Id", objId);

                    int updCustomerCount = await cn.ExecuteAsync(updCustomerCmd, updCustomerParam, tran);

                    if (updCustomerCount > 0)
                    {
                        ObjectStatusAuditTrail activateStatusChgAudit = new()
                        {
                            ObjectCode = obj.ObjectCode,
                            ObjectName = obj.GetType().Name,
                            ObjectId = objId,
                            ActionCode = "ACTIVATE",
                            FromStatus = obj.Status,
                            ToStatus = endStatus,
                            Remark = remark,
                            TriggeredUserId = user.UserId,
                            CreatedDateTime = obj.CreatedDateTime,
                            CreatedUser = user.UserName,
                            ModifiedDateTime = obj.ModifiedDateTime,
                            ModifiedUser = obj.ModifiedUser
                        };

                        int activateStatusChgAuditId = await cn.InsertAsync(activateStatusChgAudit, tran);

                        if (activateStatusChgAuditId <= 0)
                            throw new Exception("Failed to inster Customer Change History ('Activate').");

                        // Stamp customer Id to Business Entity or Person once when Activate
                        string sqlUpdCustId;
                        DynamicParameters updCustIdParam = new();
                        updCustIdParam.Add("@CustomerId", objId);
                        updCustIdParam.Add("@CustomerObjectCode", obj.ObjectCode, DbType.AnsiString);

                        if (obj.BusinessEntityId.HasValue)
                        {
                            sqlUpdCustId = $"UPDATE {BusinessEntity.MsSqlTable} SET CustomerId=@CustomerId, CustomerObjectCode=@CustomerObjectCode WHERE Id=@Id";
                            updCustIdParam.Add("@Id", obj.BusinessEntityId!.Value);
                        }
                        else if (obj.PersonId.HasValue)
                        {
                            sqlUpdCustId = $"UPDATE {Person.MsSqlTable} SET CustomerId=@CustomerId, CustomerObjectCode=@CustomerObjectCode WHERE Id=@Id";
                            updCustIdParam.Add("@Id", obj.PersonId!.Value);
                        }
                        else
                        {
                            throw new Exception("Cannot update Customer Info because neither entity no person.");
                        }

                        int updCustIdCount = await cn.ExecuteAsync(sqlUpdCustId, updCustIdParam, tran);
                    }
                    #endregion
                }
            }

            if (objId <= 0)
                throw new Exception("Failed to insert customer");

            tran.Commit();
            return objId;
        }
        catch 
        {
            tran.Rollback();
            throw;
        }
    }

    public async Task<bool> UpdateFullAsync(Customer obj, UserSessionInfo user, string endStatus = "", string? remark = null)
    {
        using var cn = ConnectionFactory.GetDbConnection()!;
        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();
        using var tran = cn.BeginTransaction();

        try
        {
            ObjectStatusAuditTrail? statusChgAudit = null;

            if (CustomerStatuses.IsValid(endStatus))
            {
                statusChgAudit = new()
                {
                    ObjectCode = obj.ObjectCode,
                    ObjectName = obj.GetType().Name,
                    ObjectId = obj.Id,
                    TriggeredUserId = user.UserId,
                    FromStatus = obj.Status,
                    ToStatus = endStatus,
                    Remark = remark,
                    CreatedDateTime = obj.ModifiedDateTime,
                    CreatedUser = obj.ModifiedUser,
                    ModifiedDateTime = obj.ModifiedDateTime,
                    ModifiedUser = obj.ModifiedUser
                };
            }


            switch(endStatus)
            {
                case CustomerStatuses.ACTIVE:
                    {
                        RunNumGenParam rngParam = new(user.UserId, user.UserName!, obj.ModifiedDateTime!.Value, obj.ModifiedDateTime!.Value, obj.GetType().Name);
                        obj.ObjectCode = await GetRunningNumberAsync(cn, rngParam, tran);

                        if (string.IsNullOrEmpty(obj.ObjectCode))
                            throw new Exception("ObjectCode cannot be blank.");

                        statusChgAudit!.ActionCode = "ACTIVATE";

                        // Stamp customer Id to Business Entity or Person once when Activate
                        string sqlUpdCustId;
                        DynamicParameters updCustIdParam = new();
                        updCustIdParam.Add("@CustomerId", obj.Id);
                        updCustIdParam.Add("@CustomerObjectCode", obj.ObjectCode, DbType.AnsiString);

                        if (obj.BusinessEntityId.HasValue)
                        {
                            sqlUpdCustId = $"UPDATE {BusinessEntity.MsSqlTable} SET CustomerId=@CustomerId, CustomerObjectCode=@CustomerObjectCode WHERE Id=@Id";
                            updCustIdParam.Add("@Id", obj.BusinessEntityId!.Value);
                        }
                        else if (obj.PersonId.HasValue)
                        {
                            sqlUpdCustId = $"UPDATE {Person.MsSqlTable} SET CustomerId=@CustomerId, CustomerObjectCode=@CustomerObjectCode WHERE Id=@Id";
                            updCustIdParam.Add("@Id", obj.PersonId!.Value);
                        }
                        else
                        {
                            throw new Exception("Cannot update Customer Info because neither entity no person.");
                        }

                        int updCustIdCount = await cn.ExecuteAsync(sqlUpdCustId, updCustIdParam, tran);
                        obj.Status = endStatus;
                    }
                    break;
                case CustomerStatuses.BLACKLISTED:
                    {
                        statusChgAudit!.ActionCode = "BLACKLIST";
                        obj.Status = endStatus;
                    }
                    break;
                case CustomerStatuses.TERMINATED:
                    {
                        obj.TerminateDateTime = obj.ModifiedDateTime;
                        statusChgAudit!.ActionCode = "TERMINATE";
                        obj.Status = endStatus;
                    }
                    break;
                case CustomerStatuses.INACTIVE:
                    {
                        statusChgAudit!.ActionCode = "DE-ACTIVATE";
                        obj.Status = endStatus;
                    }
                    break;
                default:
                    {
                        statusChgAudit!.ActionCode = "INFO-UPDATE";
                    }
                    break;
            }

            if (statusChgAudit != null && !statusChgAudit.FromStatus!.Equals(statusChgAudit.ToStatus))
            {
                int statusChgAuditId = await cn.InsertAsync(statusChgAudit, tran);
                if (statusChgAuditId <= 0)
                    throw new Exception("Failed to insert customer change history.");
            }

            bool isMainObjUpdated = await cn.UpdateAsync(obj, tran);

            if (isMainObjUpdated)
            {
                if (obj.Person != null)
                {
                    obj.Person.ModifiedUser = obj.ModifiedUser;
                    obj.Person.ModifiedDateTime = obj.ModifiedDateTime;

                    bool isPersonUpdated = await cn.UpdateAsync(obj.Person, tran);

                    if (!isPersonUpdated)
                        throw new Exception("Failed to updated person link");
                }

                if (obj.BusinessEntity != null)
                {
                    obj.BusinessEntity.ModifiedUser = obj.ModifiedUser;
                    obj.BusinessEntity.ModifiedDateTime = obj.ModifiedDateTime;

                    bool isBusnEntUpdated = await cn.UpdateAsync(obj.BusinessEntity, tran);

                    if (!isBusnEntUpdated)
                        throw new Exception("Failed to updated business entity link");
                }
            }

            tran.Commit();
            return isMainObjUpdated;
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

	public override async Task<KeyValuePair<int, IEnumerable<Customer>>> SearchNewAsync(
		int pgSize = 0, int pgNo = 0, string? searchText = null,
		IEnumerable<SqlSortCond>? sortConds = null,
		IEnumerable<SqlFilterCond>? filterConds = null,
		List<int>? excludeIdList = null)
	{
		DynamicParameters param = new();
		SqlBuilder sbSql = new();

		sbSql.Where("t.IsDeleted=0");

		#region Form Search Conditions
		if (!string.IsNullOrEmpty(searchText))
		{
			if (searchText.StartsWith("id:"))
			{
				sbSql.Where("UPPER(t.ObjectCode) LIKE '%'+@SearchText+'%'");
				param.Add("@SearchText", searchText.Replace("id:", "", StringComparison.CurrentCultureIgnoreCase), DbType.AnsiString);
			}
			else
			{
				sbSql.Where("(UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%')");
				param.Add("@SearchText", searchText, DbType.AnsiString);
			}
		}

		if (excludeIdList != null && excludeIdList.Count != 0)
		{
			sbSql.Where("t.Id NOT IN @ExcludeIdList");
			param.Add("@ExcludeIdList", excludeIdList);
		}

        if (filterConds != null && filterConds.Any())
        {
            foreach (SqlFilterCond filter in filterConds)
            {
				sbSql.Where(filter.GetFilterSqlCommand("t"));

                if (filter.Parameters.ParameterNames.Any())
                    param.AddDynamicParams(filter.Parameters);

			}
        }
        #endregion

        sbSql.LeftJoin($"{Person.MsSqlTable} ps ON ps.Id=t.PersonId");
        sbSql.LeftJoin($"{BusinessEntity.MsSqlTable} be ON be.Id=t.BusinessEntityId");

		sbSql.OrderBy("t.ObjectName ASC");

		string sql;

		if (pgNo == 0 && pgSize == 0)
		{
			sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
		}
		else
		{
			param.Add("@PageSize", pgSize);
			param.Add("@PageNo", pgNo);
			sql = sbSql.AddTemplate(
				$";WITH pg AS (SELECT Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
				$"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
		}

		using var cn = ConnectionFactory.GetDbConnection()!;

		var dataList = await cn.QueryAsync<Customer, Person, BusinessEntity, Customer>(sql, (obj, person, busnEntity) =>
        {
            obj.Person = person;
            obj.BusinessEntity = busnEntity;

            return obj;

        }, param, splitOn:"Id");

		string sqlCount = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int dataCount = await cn.ExecuteScalarAsync<int>(sqlCount, param);
		return new(dataCount, dataList);
	}

	public async Task<List<Customer>> QuickSearchAsync(bool isSearchIndividual, int pgSize = 0, int pgNo = 0, string? searchText = null,
        Dictionary<string, bool>? fieldRequiredToHaveValues = null,
        List<int>? excludeIdList = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");
        param.Add("@CustomerType", CustomerTypes.INDIVIDUAL);
        sbSql.Where("t.CustomerType IS NOT NULL");

        if (isSearchIndividual)
            sbSql.Where("t.CustomerType=@CustomerType");
        else
            sbSql.Where("t.CustomerType<>@CustomerType");

		Regex alphabets = new(@"^[a-zA-Z0-9 ,.-]{1,}$");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(searchText))
        {
            if (searchText.StartsWith("id:", StringComparison.OrdinalIgnoreCase))
            {
                sbSql.Where("UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%'");
                param.Add("@SearchText", searchText.Replace("id:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
            }
            else if (searchText.StartsWith("code:", StringComparison.OrdinalIgnoreCase))
            {
                sbSql.Where("UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%'");
                param.Add("@SearchText", searchText.Replace("code:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
            }
            else if (alphabets.IsMatch(searchText))
            {
                sbSql.Where("UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%'");
                param.Add("@SearchText", searchText, DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.ObjectNameKh LIKE '%'+@SearchText+'%'");
                param.Add("@SearchText", searchText);
            }
        }

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }

        if (fieldRequiredToHaveValues != null && fieldRequiredToHaveValues.Keys.Count > 0)
        {
            foreach (string key in fieldRequiredToHaveValues.Keys)
            {
                switch (key)
                {
                    case "PersonId":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.PersonId IS NOT NULL");
                        break;

                    case "BusinessEntityId":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.BusinessEntityId IS NOT NULL");
                        break;

                    default: break;
                }
            }
        }
        #endregion

        if (isSearchIndividual)
        {
			sbSql.LeftJoin($"{Person.MsSqlTable} ps ON ps.Id=t.PersonId");
			sbSql.LeftJoin($"{Country.MsSqlTable} natCty ON natCty.IsDeleted=0 AND natCty.ObjectCode=ps.NationalityCountryCode");
		}
        else
        {
			sbSql.LeftJoin($"{BusinessEntity.MsSqlTable} be ON be.Id=t.BusinessEntityId");
			sbSql.LeftJoin($"{Industry.MsSqlTable} i ON i.Id=be.IndustryId");
			sbSql.LeftJoin($"{BusinessSector.MsSqlTable} bs ON bs.Id=be.BusinessSectorId");
			sbSql.LeftJoin($"{Country.MsSqlTable} cty ON cty.IsDeleted=0 AND cty.ObjectCode=be.BaseCountryCode");
		}

        sbSql.OrderBy("t.ObjectName ASC");

        string sql;

        if (pgNo == 0 && pgSize == 0)
        {
            sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
        }
        else
        {
            param.Add("@PageSize", pgSize);
            param.Add("@PageNo", pgNo);

            if (isSearchIndividual)
            {
				sql = sbSql.AddTemplate($";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
				                        $"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
			}
            else
            {
				sql = sbSql.AddTemplate($";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
										$"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
			}
        }

        using var cn = ConnectionFactory.GetDbConnection()!;

        if (isSearchIndividual)
        {
            var dataList = (await cn.QueryAsync<Customer, Person, Country, Customer>(sql,
                                (obj, person, natCty) =>
                                {
                                    person.Nationality = natCty;
                                    obj.Person = person;

                                    return obj;
                                }, param, splitOn: "Id")).AsList();

			return dataList;
		}
        else
        {
			var dataList = (await cn.QueryAsync<Customer, BusinessEntity, Industry, BusinessSector, Country, Customer>(sql,
								(obj, busnEnt, industry, sector, country) =>
								{
                                    busnEnt.Industry = industry;
                                    busnEnt.BusinessSector = sector;
                                    busnEnt.BaseCountry = country;
									obj.BusinessEntity = busnEnt;

									return obj;
								}, param, splitOn: "Id")).AsList();
			return dataList;
		}
    }

    public async Task<DataPagination> GetQuickSearchPaginationAsync(bool isSearchIndividual, int pgSize = 0, string? searchText = null,
        Dictionary<string, bool>? fieldRequiredToHaveValues = null,
        List<int>? excludeIdList = null)
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");
        param.Add("@CustomerType", CustomerTypes.INDIVIDUAL);
        sbSql.Where("t.CustomerType IS NOT NULL");

        if (isSearchIndividual)
            sbSql.Where("t.CustomerType=@CustomerType");
        else
            sbSql.Where("t.CustomerType<>@CustomerType");

        Regex alphabets = new(@"^[a-zA-Z0-9 ,.-]{1,}$");
        #region Form Search Conditions
        if (!string.IsNullOrEmpty(searchText))
        {
            if (searchText.StartsWith("id:", StringComparison.OrdinalIgnoreCase))
            {
                sbSql.Where("UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%'");
                param.Add("@SearchText", searchText.Replace("id:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
            }
            else if (searchText.StartsWith("code:", StringComparison.OrdinalIgnoreCase))
            {
                sbSql.Where("UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%'");
                param.Add("@SearchText", searchText.Replace("code:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
            }
            else if (alphabets.IsMatch(searchText))
            {
                sbSql.Where("UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%'");
                param.Add("@SearchText", searchText, DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.ObjectNameKh LIKE '%'+@SearchText+'%'");
                param.Add("@SearchText", searchText);
            }
        }

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }

        if (fieldRequiredToHaveValues != null && fieldRequiredToHaveValues.Keys.Count > 0)
        {
            foreach (string key in fieldRequiredToHaveValues.Keys)
            {
                switch (key)
                {
                    case "PersonId":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.PersonId IS NOT NULL");
                        break;

                    case "BusinessEntityId":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.BusinessEntityId IS NOT NULL");
                        break;

                    default: break;
                }
            }
        }
        #endregion

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)(Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize)));

        DataPagination pagination = new()
        {
            ObjectType = typeof(Customer).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }

    public async Task<List<Customer>> SearchAsync(
        bool isSearchIndividual,
        int pgSize = 0,
        int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
        string? objectNameKh = null,
        DateTime? startDateFrom = null,
        DateTime? startDateTo = null,
        string? gender = null,
        string? idNum = null,
		List<string>? countryList = null,
		string? customerStatus = null,
        Dictionary<string, bool>? fieldRequiredToHaveValues = null,
        List<int>? excludeIdList = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");
        param.Add("@CustomerType", CustomerTypes.INDIVIDUAL);
        sbSql.Where("t.CustomerType IS NOT NULL");

        if (isSearchIndividual)
            sbSql.Where("t.CustomerType=@CustomerType");
        else
            sbSql.Where("t.CustomerType<>@CustomerType");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("UPPER(t.ObjectCode) LIKE UPPER(@ObjectCode)+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@ObjectName)+'%'");
            param.Add("@ObjectName", objectName, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectNameKh))
        {
            sbSql.Where("t.ObjectNameKh LIKE '%'+@ObjectNameKh+'%'");
            param.Add("@ObjectNameKh", objectName, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(customerStatus))
        {
            sbSql.Where("t.[Status]=@CusotmerStatus");
            param.Add("@CusotmerStatus", customerStatus, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(gender))
        {
            if (isSearchIndividual)
            {
                sbSql.Where("ps.Gender=@Gender");
                param.Add("@Gender", gender, DbType.AnsiString);
            }
        }

        if (startDateFrom != null)
        {
            if (isSearchIndividual)
            {
                sbSql.Where("ps.BirthDate IS NOT NULL");
                sbSql.Where("ps.BirthDate >= @StartDateFrom");
            }
            else
            {
                sbSql.Where("ps.RegistrationDate IS NOT NULL");
                sbSql.Where("be.RegistrationDate >= @StartDateFrom");
            }

            param.Add("@StartDateFrom", startDateFrom!.Value);

            if (startDateTo != null)
            {
                if (isSearchIndividual)
                {
                    sbSql.Where("ps.BirthDate <= @StartDateTo");
                }
                else
                {
                    sbSql.Where("be.RegistrationDate <= @StartDateTo");
                }

                param.Add("@StartDateTo", startDateTo!.Value);
            }
        }
        else if (startDateTo != null)
        {
            if (isSearchIndividual)
            {
                sbSql.Where("ps.BirthDate IS NOT NULL");
                sbSql.Where("ps.BirthDate <= @StartDateTo");
            }
            else
            {
                sbSql.Where("ps.RegistrationDate IS NOT NULL");
                sbSql.Where("be.RegistrationDate <= @StartDateTo");
            }

            param.Add("@StartDateTo", startDateTo!.Value);
        }

        if (!string.IsNullOrEmpty(idNum))
        {
            if (isSearchIndividual)
            {
                sbSql.Where("ps.NationalIdNum LIKE '%'+@IdNum+'%' OR ps.PassportNo LIKE '%'+@IdNum+'%'");
            }
            else
            {
                sbSql.Where("be.RegistrationNo LIKE '%'+@IdNum+'%' OR ps.RegistrationNo LIKE '%'+@IdNum+'%'");
            }

            param.Add("@IdNum", idNum, DbType.AnsiString);
        }

        if (countryList != null && countryList.Count > 0)
        {
            if (isSearchIndividual)
            {

				sbSql.Where("ps.NationalityCountryCode IN @CountryList");
				param.Add("@CountryList", countryList);
			}
            else
            {
				sbSql.Where("be.BaseCountryCode IN @CountryList");
				param.Add("@CountryList", countryList);
			}
        }

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }

        if (fieldRequiredToHaveValues != null && fieldRequiredToHaveValues.Keys.Count > 0)
        {
            foreach (string key in fieldRequiredToHaveValues.Keys)
            {
                switch (key)
                {
                    case "PersonId":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.PersonId IS NOT NULL");
                        break;

                    case "BusinessEntityId":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.BusinessEntityId IS NOT NULL");
                        break;

                    default: break;
                }
            }
        }
        #endregion

        if (isSearchIndividual)
		{
			sbSql.LeftJoin($"{Person.MsSqlTable} ps ON ps.Id=t.PersonId");
			sbSql.LeftJoin($"{Country.MsSqlTable} natCty ON natCty.IsDeleted=0 AND natCty.ObjectCode=ps.NationalityCountryCode");
		}
		else
		{
			sbSql.LeftJoin($"{BusinessEntity.MsSqlTable} be ON be.Id=t.BusinessEntityId");
			sbSql.LeftJoin($"{Industry.MsSqlTable} i ON i.Id=be.IndustryId");
			sbSql.LeftJoin($"{BusinessSector.MsSqlTable} bs ON bs.Id=be.BusinessSectorId");
			sbSql.LeftJoin($"{Country.MsSqlTable} cty ON cty.IsDeleted=0 AND cty.ObjectCode=be.BaseCountryCode");
		}

		sbSql.OrderBy("t.ObjectName ASC");

        string sql;

        if (pgNo == 0 && pgSize == 0)
        {
            sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
        }
        else
        {
            param.Add("@PageSize", pgSize);
            param.Add("@PageNo", pgNo);

			if (isSearchIndividual)
			{
				sql = sbSql.AddTemplate($";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                                        $"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
            }
			else
			{
				sql = sbSql.AddTemplate($";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
										$"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
			}
		}

        using var cn = ConnectionFactory.GetDbConnection()!;

		if (isSearchIndividual)
		{
			var result = (await cn.QueryAsync<Customer, Person, Country, Customer>(sql,
								(obj, person, natCty) =>
								{
									person.Nationality = natCty;
									obj.Person = person;

									return obj;
								}, param, splitOn: "Id")).AsList();

			return result;
		}
		else
		{
			var result = (await cn.QueryAsync<Customer, BusinessEntity, Industry, BusinessSector, Country, Customer>(sql,
								(obj, busnEnt, industry, sector, country) =>
								{
									busnEnt.Industry = industry;
									busnEnt.BusinessSector = sector;
									busnEnt.BaseCountry = country;
									obj.BusinessEntity = busnEnt;

									return obj;
								}, param, splitOn: "Id")).AsList();
			return result;
		}
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        bool isSearchIndividual,
        int pgSize = 0,
        string? objectCode = null,
        string? objectName = null,
        string? objectNameKh = null,
        DateTime? startDateFrom = null,
        DateTime? startDateTo = null,
        string? gender = null,
        string? idNum = null,
		List<string>? countryList = null,
		string? customerStatus = null,
        Dictionary<string, bool>? fieldRequiredToHaveValues = null,
        List<int>? excludeIdList = null)
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");
        param.Add("@CustomerType", CustomerTypes.INDIVIDUAL);
        sbSql.Where("t.CustomerType IS NOT NULL");

        if (isSearchIndividual)
            sbSql.Where("t.CustomerType=@CustomerType");
        else
            sbSql.Where("t.CustomerType<>@CustomerType");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("UPPER(t.ObjectCode) LIKE UPPER(@ObjectCode)+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@ObjectName)+'%'");
            param.Add("@ObjectName", objectName, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectNameKh))
        {
            sbSql.Where("t.ObjectNameKh LIKE '%'+@ObjectNameKh+'%'");
            param.Add("@ObjectNameKh", objectName, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(customerStatus))
        {
            sbSql.Where("t.[Status]=@CusotmerStatus");
            param.Add("@CusotmerStatus", customerStatus, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(gender))
        {
            if (isSearchIndividual)
            {
                sbSql.Where("ps.Gender=@Gender");
                param.Add("@Gender", gender, DbType.AnsiString);
            }
        }

        if (startDateFrom != null)
        {
            if (isSearchIndividual)
            {
                sbSql.Where("ps.BirthDate IS NOT NULL");
                sbSql.Where("ps.BirthDate >= @StartDateFrom");
            }
            else
            {
                sbSql.Where("ps.RegistrationDate IS NOT NULL");
                sbSql.Where("be.RegistrationDate >= @StartDateFrom");
            }

            param.Add("@StartDateFrom", startDateFrom!.Value);

            if (startDateTo != null)
            {
                if (isSearchIndividual)
                {
                    sbSql.Where("ps.BirthDate <= @StartDateTo");
                }
                else
                {
                    sbSql.Where("be.RegistrationDate <= @StartDateTo");
                }

                param.Add("@StartDateTo", startDateTo!.Value);
            }
        }
        else if (startDateTo != null)
        {
            if (isSearchIndividual)
            {
                sbSql.Where("ps.BirthDate IS NOT NULL");
                sbSql.Where("ps.BirthDate <= @StartDateTo");
            }
            else
            {
                sbSql.Where("ps.RegistrationDate IS NOT NULL");
                sbSql.Where("be.RegistrationDate <= @StartDateTo");
            }

            param.Add("@StartDateTo", startDateTo!.Value);
        }

        if (!string.IsNullOrEmpty(idNum))
        {
            if (isSearchIndividual)
            {
                sbSql.Where("ps.NationalIdNum LIKE '%'+@IdNum+'%' OR ps.PassportNo LIKE '%'+@IdNum+'%'");
            }
            else
            {
                sbSql.Where("be.RegistrationNo LIKE '%'+@IdNum+'%' OR ps.RegistrationNo LIKE '%'+@IdNum+'%'");
            }

            param.Add("@IdNum", idNum, DbType.AnsiString);
        }

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }

        if (fieldRequiredToHaveValues != null && fieldRequiredToHaveValues.Keys.Count > 0)
        {
            foreach (string key in fieldRequiredToHaveValues.Keys)
            {
                switch (key)
                {
                    case "PersonId":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.PersonId IS NOT NULL");
                        break;

                    case "BusinessEntityId":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.BusinessEntityId IS NOT NULL");
                        break;

                    default: break;
                }
            }
        }
        #endregion

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)(Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize)));

        DataPagination pagination = new()
        {
            ObjectType = typeof(Customer).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }
}