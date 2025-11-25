using DataLayer.GlobalConstant;
using DataLayer.Models.SystemCore.NonPersistent;
using System.Text.RegularExpressions;

namespace DataLayer.Repos.SystemCore;

public interface IEmployeeRepos : IBaseRepos<Employee>
{
	Task<Employee?> GetFullAsync(int objId);

	Task<Employee?> GetFullAsync(string objectCode);

	Task<bool> UpdateFullAsync(Employee obj, UserSessionInfo user, string endStatus = "", string? remark = null);

	Task<int> InsertFullAsync(Employee obj, UserSessionInfo user, string endStatus = "", string? remark = null);

	Task<List<Employee>> FindAsync(
		int? objId,
		string? surname,
		string? givenName,
		string? surnameKh,
		string? givenNameKh,
		string? gender,
		DateTime? birthDate,
		string? nationalIdNum,
		string? passportIdNum);

	Task<bool> IsDuplicateEmployeeIDAsync(int objId, string employeeID);

	Task<List<Employee>> QuickSearchAsync(int pgSize = 0,
		int pgNo = 0,
		string? searchText = null,
		Dictionary<string, bool>? fieldRequiredToHaveValues = null,
		List<int>? excludeIdList = null,
		List<string>? employeeStatusList = null);

	Task<DataPagination> GetQuickSearchPaginationAsync(
		int pgSize = 0,
		string? searchText = null,
		Dictionary<string, bool>? fieldRequiredToHaveValues = null,
		List<int>? excludeIdList = null,
		List<string>? employeeStatusList = null);

	Task<List<Employee>> SearchAsync(
		int pgSize = 0,
		int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		string? fullNameKh = null,
		List<string>? genderList = null,
		List<string>? maritalStatusList = null,
		int? ageFrom = null,
		int? ageTo = null,
		string? nationalIdNum = null,
		string? passportNo = null,
		string? jobTitle = null,
		string? positionTitle = null,
		string? orgStructHierarchyPath = null,
		List<string>? NatlCtyCodeList = null,
		List<int>? residentialAddressProvinceIdList = null,
		List<string>? contractTypeList = null,
		List<string>? timeTypeList = null,
		List<string>? employeeStatusList = null,
		Dictionary<string, bool>? fieldRequiredToHaveValues = null,
		List<int>? excludeIdList = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		string? fullNameKh = null,
		List<string>? genderList = null,
		List<string>? maritalStatusList = null,
		int? ageFrom = null,
		int? ageTo = null,
		string? nationalIdNum = null,
		string? passportNo = null,
		string? jobTitle = null,
		string? positionTitle = null,
		string? orgStructHierarchyPath = null,
		List<string>? NatlCtyCodeList = null,
		List<int>? residentialAddressProvinceIdList = null,
		List<string>? contractTypeList = null,
		List<string>? timeTypeList = null,
		List<string>? employeeStatusList = null,
		Dictionary<string, bool>? fieldRequiredToHaveValues = null,
		List<int>? excludeIdList = null);
}

public class EmployeeRepos(IConnectionFactory connectionFactory) : BaseRepos<Employee>(connectionFactory, Employee.DatabaseObject), IEmployeeRepos
{
	public async Task<int> InsertFullAsync(Employee obj, UserSessionInfo user, string endStatus = "", string? remark = null)
    {
        if (obj.Id > 0)
            throw new Exception("Invalid object.");

        using var cn = ConnectionFactory.GetDbConnection()!;

        //<**!IMPORATNT**> Need to ensure Connection is open before opening transaction
        if (cn.State != ConnectionState.Open) cn.Open();

        using var tran = cn.BeginTransaction();

		try
		{
			DateTime timestamp = DateTime.UtcNow.AddHours(7);

			obj.CreatedDateTime = timestamp;
			obj.ModifiedDateTime = timestamp;

			int objId = await cn.InsertAsync(obj, tran);

			if (objId > 0)
			{
				if (obj.CorrespondentAddress != null)
				{
					obj.CorrespondentAddress.LinkedObjectType = obj.GetType().Name;
					obj.CorrespondentAddress.LinkedObjectId = obj.Id;
					obj.CorrespondentAddress.CreatedUser = obj.CreatedUser;
					obj.CorrespondentAddress.ModifiedUser = obj.ModifiedUser;
					obj.CorrespondentAddress.CreatedDateTime = obj.CreatedDateTime;
					obj.CorrespondentAddress.ModifiedDateTime = obj.ModifiedDateTime;

					int corAddrId = await cn.InsertAsync(obj.CorrespondentAddress);

					if (corAddrId > 0)
                        obj.CorrespAddrId = corAddrId;
					else
						throw new Exception("Failed to insert Employee > Corresponding Address to database.");
				}

				if (obj.ResidentialAddress != null)
				{
					obj.ResidentialAddress.LinkedObjectType = obj.GetType().Name;
					obj.ResidentialAddress.LinkedObjectId = obj.Id;
					obj.ResidentialAddress.CreatedUser = obj.CreatedUser;
					obj.ResidentialAddress.ModifiedUser = obj.ModifiedUser;
					obj.ResidentialAddress.CreatedDateTime = obj.CreatedDateTime;
					obj.ResidentialAddress.ModifiedDateTime = obj.ModifiedDateTime;

					int resAddrId = await cn.InsertAsync(obj.ResidentialAddress);

					if (resAddrId > 0)
						obj.ResAddrId = resAddrId;
					else
						throw new Exception("Failed to insert Employee > Residential Address to database.");
                }

				if (obj.CorrespAddrId != null || obj.ResAddrId != null)
					await cn.UpdateAsync(obj);

                ObjectStatusAuditTrail statusChgAudit = new()
                {
                    ObjectCode = obj.ObjectCode,
                    ObjectName = obj.GetType().Name,
                    ObjectId = objId,
                    ActionCode = "SAFE-DRAFT",
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

                if (endStatus.Length > 0 && endStatus == EmployeeStatuses.ACTIVE)
                {
                    #region EMPLOYEE ACTIVATION
                    string updEmployeeCmd = $"UPDATE {Employee.MsSqlTable} SET [Status]=@Status WHERE Id=@Id";

                    DynamicParameters updCustomerParam = new();
                    updCustomerParam.Add("@Status", endStatus, DbType.AnsiString);
                    updCustomerParam.Add("@Id", objId);

                    int updEmployeeCount = await cn.ExecuteAsync(updEmployeeCmd, updCustomerParam, tran);

                    if (updEmployeeCount > 0)
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
                    }
                    #endregion
                }
            }
			else
			{
				throw new Exception("Unable to save object to databse.");
			}

			tran.Commit();
			return objId;
		}
		catch 
		{
			tran.Rollback();
			throw;
		}
    }

    public async Task<Employee?> GetFullAsync(int objId)
	{
		SqlBuilder sbSql = new();
		DynamicParameters param = new();
		param.Add("@Id", objId);

		sbSql.LeftJoin($"{CambodiaAddress.MsSqlTable} resAdd ON resAdd.IsDeleted=0 AND resAdd.Id=t.ResidentialAddressId");
		sbSql.LeftJoin($"{CambodiaAddress.MsSqlTable} corAdd ON corAdd.IsDeleted=0 AND corAdd.Id=t.CorrespondentAddressId");
		sbSql.LeftJoin($"{OrgStruct.MsSqlTable} org ON org.IsDeleted=0 AND org.Id=t.OrgStructId");

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.Id=@Id");

		string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

		using var cn = ConnectionFactory.GetDbConnection()!;

		Employee? data = (await cn.QueryAsync<Employee, CambodiaAddress, CambodiaAddress, OrgStruct, Employee>(
				sql, (obj, residentialAddress, correspondentAddress, orgStruct) =>
				{
					obj.ResidentialAddress = residentialAddress;
					obj.CorrespondentAddress = correspondentAddress;

					if (orgStruct != null && !string.IsNullOrEmpty(orgStruct.ObjectCode) && !string.IsNullOrEmpty(orgStruct.OrgStructTypeCode))
					{
						switch (orgStruct.OrgStructTypeCode)
						{
							case OrganizationStructureTypes.DEPARTMENT:
								obj.Department = orgStruct;
								break;
							case OrganizationStructureTypes.FUNCTION:
								obj.Function = orgStruct;
								break;
							case OrganizationStructureTypes.TEAM:
								obj.Team = orgStruct;
								break;
						}
					}

					return obj;
				}, param, splitOn: "Id")).FirstOrDefault();

		return data;
	}

    public async Task<Employee?> GetFullAsync(string objectCode)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();
        param.Add("@ObjectCode", objectCode, DbType.AnsiString);

        sbSql.LeftJoin($"{CambodiaAddress.MsSqlTable} resAdd ON resAdd.IsDeleted=0 AND resAdd.Id=t.ResidentialAddressId");
        sbSql.LeftJoin($"{CambodiaAddress.MsSqlTable} corAdd ON corAdd.IsDeleted=0 AND corAdd.Id=t.CorrespondentAddressId");
        sbSql.LeftJoin($"{OrgStruct.MsSqlTable} org ON org.IsDeleted=0 AND org.Id=t.OrgStructId");

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.ObjectCode=@ObjectCode");

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        Employee? data = (await cn.QueryAsync<Employee, CambodiaAddress, CambodiaAddress, OrgStruct, Employee>(
                sql, (obj, residentialAddress, correspondentAddress, orgStruct) =>
                {
                    obj.ResidentialAddress = residentialAddress;
                    obj.CorrespondentAddress = correspondentAddress;

                    if (orgStruct != null && !string.IsNullOrEmpty(orgStruct.ObjectCode) && !string.IsNullOrEmpty(orgStruct.OrgStructTypeCode))
                    {
                        switch (orgStruct.OrgStructTypeCode)
                        {
                            case OrganizationStructureTypes.DEPARTMENT:
                                obj.Department = orgStruct;
                                break;
                            case OrganizationStructureTypes.FUNCTION:
                                obj.Function = orgStruct;
                                break;
                            case OrganizationStructureTypes.TEAM:
                                obj.Team = orgStruct;
                                break;
                        }
                    }

                    return obj;
                }, param, splitOn: "Id")).FirstOrDefault();

        return data;
    }

    public async Task<bool> UpdateFullAsync(Employee obj, UserSessionInfo user, string endStatus = "", string? remark = null)
    {
		if (obj.Id <= 0)
			throw new Exception("Invalid object.");

        using var cn = ConnectionFactory.GetDbConnection()!;

        //<**!IMPORATNT**> Need to ensure Connection is open before opening transaction
        if (cn.State != ConnectionState.Open) cn.Open();

        using var tran = cn.BeginTransaction();

        try
        {
            ObjectStatusAuditTrail? statusChgAudit = null;

            if (EmployeeStatuses.IsValid(endStatus))
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

			switch (endStatus)
			{
				case EmployeeStatuses.INVALID:
					statusChgAudit!.ActionCode = "CANCEL";
					break;
                case EmployeeStatuses.ACTIVE:
                    statusChgAudit!.ActionCode = "ACTIVATE";
                    break;
                case EmployeeStatuses.BLACKLIST:
                    statusChgAudit!.ActionCode = "BLACKLIST";
                    break;
                case EmployeeStatuses.TERMINATED:
                    statusChgAudit!.ActionCode = "TERMINATE";
                    break;
                case EmployeeStatuses.SUSPENDED:
                    statusChgAudit!.ActionCode = "SUSPEND";
                    break;
                case EmployeeStatuses.RESIGNED:
                    statusChgAudit!.ActionCode = "RESIGN";
                    break;
                default:
					break;
			}

            if (statusChgAudit != null && !statusChgAudit.FromStatus!.Equals(statusChgAudit.ToStatus))
            {
                int statusChgAuditId = await cn.InsertAsync(statusChgAudit, tran);
                if (statusChgAuditId <= 0)
                    throw new Exception("Failed to insert customer change history.");
                else obj.Status = endStatus;
            }

            if (obj.CorrespondentAddress != null)
			{
                obj.CorrespondentAddress.LinkedObjectType = obj.GetType().Name;
                obj.CorrespondentAddress.LinkedObjectId = obj.Id;

                if (obj.CorrespondentAddress.Id > 0)
				{
                    bool isCorAddrUpdated = await cn.UpdateAsync(obj.CorrespondentAddress, tran);

					if (isCorAddrUpdated)
						obj.CorrespAddrId = obj.CorrespondentAddress.Id;
				}
				else
				{
					int corAddrId = await cn.InsertAsync(obj, tran);
					if (corAddrId > 0)
						obj.CorrespAddrId = obj.CorrespondentAddress.Id;
                }
            }

            if (obj.ResidentialAddress != null)
            {
                obj.ResidentialAddress.LinkedObjectType = obj.GetType().Name;
                obj.ResidentialAddress.LinkedObjectId = obj.Id;

                if (obj.ResidentialAddress.Id > 0)
                {
                    bool isCorAddrUpdated = await cn.UpdateAsync(obj.ResidentialAddress, tran);

                    if (isCorAddrUpdated)
                        obj.ResAddrId = obj.ResidentialAddress.Id;
                }
                else
                {
                    int corAddrId = await cn.InsertAsync(obj, tran);
                    if (corAddrId > 0)
                        obj.ResAddrId = obj.ResidentialAddress.Id;
                }
            }

            bool isUpdated = await cn.UpdateAsync(obj, tran);

            tran.Commit();
            return isUpdated;
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

    public async Task<bool> IsDuplicateEmployeeIDAsync(int objId, string employeeID)
	{
		SqlBuilder sbSql = new();
		DynamicParameters param = new();
		param.Add("@Id", objId);
		param.Add("@EmployeeID", employeeID, DbType.AnsiString);

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.ObjectCode=@EmployeeID");
		sbSql.Where("t.Id<>@Id");

		string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;
        int count = await cn.ExecuteScalarAsync<int>(sql, param);

		return count > 0;
    }

	public override async Task<KeyValuePair<int, IEnumerable<Employee>>> SearchNewAsync(
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
		#endregion

		sbSql.LeftJoin($"{CambodiaAddress.MsSqlTable} resAdd ON resAdd.IsDeleted=0 AND resAdd.Id=t.ResAddrId");
		sbSql.LeftJoin($"{CambodiaAddress.MsSqlTable} corAdd ON corAdd.IsDeleted=0 AND corAdd.Id=t.CorrespAddrId");
		sbSql.LeftJoin($"{OrgStruct.MsSqlTable} org ON org.IsDeleted=0 AND org.Id=t.OrgStructId");

		foreach (string order in GetSearchOrderbBy())
		{
			sbSql.OrderBy(order);
		}

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

		var dataList = (await cn.QueryAsync<Employee, CambodiaAddress, CambodiaAddress, OrgStruct, Employee>(
				sql, (obj, resAddr, correspAddr, orgStruct) =>
				{
					obj.ResidentialAddress = resAddr;
					obj.CorrespondentAddress = correspAddr;

					if (orgStruct != null && !string.IsNullOrEmpty(orgStruct.ObjectCode) && !string.IsNullOrEmpty(orgStruct.OrgStructTypeCode))
					{
						switch (orgStruct.OrgStructTypeCode)
						{
							case OrganizationStructureTypes.DEPARTMENT:
								obj.Department = orgStruct;
								break;
							case OrganizationStructureTypes.FUNCTION:
								obj.Function = orgStruct;
								break;
							case OrganizationStructureTypes.TEAM:
								obj.Team = orgStruct;
								break;
						}
					}

					return obj;
				}, param, splitOn: "Id")).AsList();

		string sqlCount = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int dataCount = await cn.ExecuteScalarAsync<int>(sqlCount, param);
		return new(dataCount, dataList);
	}

	public async Task<List<Employee>> FindAsync(
        int? objId,
        string? surname,
        string? givenName,
        string? surnameKh,
        string? givenNameKh,
        string? gender,
        DateTime? birthDate,
        string? nationalIdNum,
        string? passportIdNum)
    {
        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");

        if (objId != null)
        {
            sbSql.Where("t.Id<>@ObjId");
            param.Add("@ObjId", objId!.Value);
        }

        StringBuilder sbNameEn = new();
        StringBuilder sbNameKh = new();

        if (!string.IsNullOrEmpty(surname))
        {
            sbNameEn.Append("t.Surname=@Surname");
            param.Add("@Surname", surname, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(givenName))
        {
            sbNameEn.Append(sbNameEn.Length > 0 ? " AND t.GivenName=@GivenName" : "t.GivenName=@GivenName");
            param.Add("@GivenName", givenName, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(surnameKh))
        {
            sbNameKh.Append("t.SurnameKh=@SurnameKh");
            param.Add("@SurnameKh", surnameKh);
        }

        if (!string.IsNullOrEmpty(givenNameKh))
        {
            sbNameKh.Append(sbNameEn.Length > 0 ? " AND t.GivenNameKh=@GivenNameKh" : "t.GivenNameKh=@GivenNameKh");
            param.Add("@GivenNameKh", givenNameKh, DbType.AnsiString);
        }

        StringBuilder sbSearchCondition = new();


        if (sbNameEn.Length > 0)
            sbSearchCondition.Append($"(({sbNameEn})");

        if (sbNameKh.Length > 0)
            sbSearchCondition.Append(sbSearchCondition.Length > 0 ? $" OR ({sbNameKh})" : $"(({sbNameKh})");

        if (birthDate != null)
        {
            if (sbNameEn.Length > 0)
                sbNameEn.Append(" AND t.BirthDate=@BirthDate");

            if (sbNameKh.Length > 0)
                sbNameKh.Append(" AND t.BirthDate=@BirthDate");

            param.Add("@BirthDate", birthDate);
        }

        if (!string.IsNullOrEmpty(gender))
        {
            if (sbNameEn.Length > 0)
                sbNameEn.Append(" AND t.Gender=@Gender");

            if (sbNameKh.Length > 0)
                sbNameKh.Append(" AND t.Gender=@Gender");

            param.Add("@Gender", gender, DbType.AnsiString);
        }

        if (sbNameEn.Length > 0)
            sbSearchCondition.Append(sbSearchCondition.Length > 0 ? $" OR ({sbNameEn})" : $"(({sbNameEn})");

        if (sbNameKh.Length > 0)
            sbSearchCondition.Append(sbSearchCondition.Length > 0 ? $" OR ({sbNameKh})" : $"(({sbNameKh})");

        if (!string.IsNullOrEmpty(nationalIdNum))
        {
            sbSearchCondition.Append(sbSearchCondition.Length > 0 ? $" OR t.NationalIdNum=@NationalIdNum" : $"(t.NationalIdNum=@NationalIdNum");
            param.Add("@NationalIdNum", nationalIdNum, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(passportIdNum))
        {
            sbSearchCondition.Append(sbSearchCondition.Length > 0 ? $" OR t.PassportNo=@PassportNo" : $"(t.PassportNo=@PassportNo");
            param.Add("@PassportNo", passportIdNum, DbType.AnsiString);
        }

        if (sbSearchCondition.Length > 0)
        {
            sbSearchCondition.Append(')');
            sbSql.Where(sbSearchCondition.ToString());
        }

        sbSql.LeftJoin($"{Country.MsSqlTable} nty ON nty.IsDeleted=0 AND nty.ObjectCode=t.NatlCtyCode");

        using var cn = ConnectionFactory.GetDbConnection()!;

        string sql = sbSql.AddTemplate($"SELECT TOP 100 * FROM {Employee.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        var dataList = (await cn.QueryAsync<Employee, Country, Employee>(sql, (p, cty) =>
        {
            p.Nationality = cty;
            return p;
        }, param, splitOn: "Id")).AsList();

        return dataList;
    }

    public async Task<List<Employee>> QuickSearchAsync(int pgSize = 0,
        int pgNo = 0,
        string? searchText = null,
        Dictionary<string, bool>? fieldRequiredToHaveValues = null,
        List<int>? excludeIdList = null,
        List<string>? employeeStatusList = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        Regex engNamePattern = new(@"^[a-zA-Z\s.-]{1,}$");

        if (!string.IsNullOrEmpty(searchText))
        {
            if (engNamePattern.IsMatch(searchText))
            {
                sbSql.Where("(UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%')");
                param.Add("@SearchText", searchText, DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.FullNameKh LIKE '%'+@SearchText+'%'");
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
                    case "Gender":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.Gender IS NOT NULL");
                        break;

                    case "BirthDate":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.BirthDate IS NOT NULL");
                        break;

                    case "NationalIdNum":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.NationalIdNum,''))>0");
                        break;

                    case "PassportNo":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.PassportNo,''))>0");
                        break;

                    case "PhoneLine1":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.PhoneLine1,''))>0");
                        break;

                    case "PhoneLine2":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.PhoneLine2,''))>0");
                        break;

                    case "NatlCtyCode":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.NatlCtyCode IS NOT NULL");
                        break;
                    default: break;
                }
            }
        }

        if (employeeStatusList != null && employeeStatusList.Count > 0)
        {
            if (employeeStatusList.Count == 1)
            {
                sbSql.Where("t.[Status]=@EmployeeStatus");
                param.Add("@EmployeeStatus", employeeStatusList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.[Status] IN @EmployeeStatusList");
                param.Add("@EmployeeStatusList", employeeStatusList);
            }
        }
        #endregion

        sbSql.LeftJoin($"{Country.MsSqlTable} nty ON nty.IsDeleted=0 AND nty.ObjectCode=t.NatlCtyCode");

        sbSql.OrderBy("t.Surname ASC")
            .OrderBy("t.GivenName ASC")
            .OrderBy("t.SurnameKh ASC")
            .OrderBy("t.GivenNameKh ASC");

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

        var dataList = (await cn.QueryAsync<Employee, Country, Employee>(sql, (obj, nationality) =>
        {
            obj.Nationality = nationality;
            return obj;
        }, param, splitOn: "Id")).AsList();

        return dataList;
    }

    public async Task<DataPagination> GetQuickSearchPaginationAsync(
        int pgSize = 0,
        string? searchText = null,
        Dictionary<string, bool>? fieldRequiredToHaveValues = null,
        List<int>? excludeIdList = null,
        List<string>? employeeStatusList = null)
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        Regex engNamePattern = new(@"^[a-zA-Z\s.-]{1,}$");

        if (!string.IsNullOrEmpty(searchText))
        {
            if (engNamePattern.IsMatch(searchText))
            {
                sbSql.Where("(UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%')");
                param.Add("@SearchText", searchText, DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.FullNameKh LIKE '%'+@SearchText+'%'");
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
                    case "Gender":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.Gender IS NOT NULL");
                        break;

                    case "BirthDate":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.BirthDate IS NOT NULL");
                        break;

                    case "NationalIdNum":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.NationalIdNum,''))>0");
                        break;

                    case "PassportNo":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.PassportNo,''))>0");
                        break;

                    case "PhoneLine1":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.PhoneLine1,''))>0");
                        break;

                    case "PhoneLine2":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.PhoneLine2,''))>0");
                        break;

                    case "NatlCtyCode":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.NatlCtyCode IS NOT NULL");
                        break;
                    default: break;
                }
            }
        }

        if (employeeStatusList != null && employeeStatusList.Count > 0)
        {
            if (employeeStatusList.Count == 1)
            {
                sbSql.Where("t.[Status]=@EmployeeStatus");
                param.Add("@EmployeeStatus", employeeStatusList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.[Status] IN @EmployeeStatusList");
                param.Add("@EmployeeStatusList", employeeStatusList);
            }
        }
        #endregion

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize));

        DataPagination pagination = new()
        {
            ObjectType = typeof(Employee).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }

    public async Task<List<Employee>> SearchAsync(
		int pgSize = 0,
		int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		string? fullNameKh = null,
		List<string>? genderList = null,
		List<string>? maritalStatusList = null,
		int? ageFrom = null,
		int? ageTo = null,
		string? nationalIdNum = null,
		string? passportNo = null,
		string? jobTitle = null,
		string? positionTitle = null,
		string? orgStructHierarchyPath = null,
		List<string>? NatlCtyCodeList = null,
		List<int>? residentialAddressProvinceIdList = null,
		List<string>? contractTypeList = null,
		List<string>? timeTypeList = null,
		List<string>? employeeStatusList = null,
        Dictionary<string, bool>? fieldRequiredToHaveValues = null,
        List<int>? excludeIdList = null)
	{
		if (pgNo < 0 && pgSize < 0)
			throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");

		#region Form Search Conditions
		if (!string.IsNullOrEmpty(objectCode))
		{
			sbSql.Where("UPPER(t.ObjectCode) LIKE '%'+UPPER(@ObjectCode)+'%'");
			param.Add("@ObjectCode", objectCode, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(objectName))
		{
			sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@ObjectName)+'%'");
			param.Add("@ObjectName", objectName, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(fullNameKh))
		{
			sbSql.Where("t.FullNameKh=@FullNameKh");
			param.Add("@FullNameKh", fullNameKh);
		}

		if (genderList != null && genderList.Any())
		{
			if (genderList.Count == 1)
			{
				sbSql.Where("t.Gender=@Gender");
				param.Add("@Gender", genderList[0], DbType.AnsiString);
			}
			else
			{
				sbSql.Where("t.Gender IN @GenderList");
				param.Add("@GenderList", genderList);
			}
		}

		if (maritalStatusList != null && maritalStatusList.Any())
		{
			if (maritalStatusList.Count == 1)
			{
				sbSql.Where("t.MaritalStatus=@MaritalStatus");
				param.Add("@MaritalStatus", maritalStatusList[0], DbType.AnsiString);
			}
			else
			{
				sbSql.Where("t.MaritalStatus IN @MaritalStatusList");
				param.Add("@MaritalStatusList", maritalStatusList);
			}
		}

		if (ageFrom != null)
		{
			sbSql.Where("t.BirthDate IS NOT NULL");
			sbSql.Where("FLOOR(DATEDIFF(DAY, t.BirthDate, GETDATE()) / 365.25)>=@AgetFrom");
			param.Add("@AgetFrom", ageFrom.Value);

			if (ageTo != null)
			{
				sbSql.Where("FLOOR(DATEDIFF(DAY, t.BirthDate, GETDATE()) / 365.25)<=@AgetTo");
				param.Add("@AgetTo", ageTo.Value);
			}
		}
		else if (ageTo != null)
		{
			sbSql.Where("t.BirthDate IS NOT NULL");
			sbSql.Where("FLOOR(DATEDIFF(DAY, t.BirthDate, GETDATE()) / 365.25)>=@AgetTo");
			param.Add("@AgetTo", ageTo.Value);
		}

		if (!string.IsNullOrEmpty(nationalIdNum))
		{
			sbSql.Where("t.NationalIdNum LIKE '%'+@NationalIdNum+'%'");
			param.Add("@NationalIdNum", nationalIdNum, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(passportNo))
		{
			sbSql.Where("t.PassportNo LIKE '%'+@PassportNo+'%'");
			param.Add("@PassportNo", passportNo, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(jobTitle))
		{
			sbSql.Where("LOWER(t.JobTitle) LIKE '%'+LOWER(@JobTitle)+'%'");
			param.Add("@JobTitle", jobTitle, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(positionTitle))
		{
			sbSql.Where("LOWER(t.JobPositionName) LIKE '%'+LOWER(@JobPositionName)+'%'");
			param.Add("@JobPositionName", positionTitle, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(orgStructHierarchyPath))
		{
			sbSql.Where("org.HierarchyPath LIKE LOWER(@OrgStructHierarhcyPath)+'%'");
			param.Add("@OrgStructHierarhcyPath", orgStructHierarchyPath, DbType.AnsiString);
		}

		if (NatlCtyCodeList != null && NatlCtyCodeList.Any())
		{
			if (NatlCtyCodeList.Count == 1)
			{
				sbSql.Where("t.NatlCtyCode=@NatlCtyCode");
				param.Add("@NatlCtyCode", NatlCtyCodeList[0], DbType.AnsiString);
			}
			else
			{
				sbSql.Where("t.NatlCtyCode IN @NatlCtyCodeList");
				param.Add("@NatlCtyCodeList", NatlCtyCodeList);
			}
		}

		if (residentialAddressProvinceIdList != null && residentialAddressProvinceIdList.Any())
		{
			if (residentialAddressProvinceIdList.Count == 1)
			{
				sbSql.Where("t.ResidentialAddressId IS NOT NULL");
				sbSql.Where("resAdd.CambodiaProvinceId=@ResAddrCambodiaProvinceId");
				param.Add("@ResAddrCambodiaProvinceId", residentialAddressProvinceIdList[0]);
			}
			else
			{
				sbSql.Where("t.ResidentialAddressId IS NOT NULL");
				sbSql.Where("resAdd.CambodiaProvinceId IN @ResAddrCambodiaProvinceIdList");
				param.Add("@ResAddrCambodiaProvinceIdList", residentialAddressProvinceIdList);
			}
		}

		if (contractTypeList != null && contractTypeList.Any())
		{
			if (contractTypeList.Count == 1)
			{
				sbSql.Where("t.ContractType=@ContractType");
				param.Add("@ContractType", contractTypeList[0], DbType.AnsiString);
			}
			else
			{
				sbSql.Where("t.ContractType IN @ContractTypeList");
				param.Add("@ContractTypeList", contractTypeList);
			}
		}

		if (timeTypeList != null && timeTypeList.Any())
		{
			if (timeTypeList.Count == 1)
			{
				sbSql.Where("t.TimeType=@TimeType");
				param.Add("@TimeType", timeTypeList[0], DbType.AnsiString);
			}
			else
			{
				sbSql.Where("t.TimeType IN @TimeTypeList");
				param.Add("@TimeTypeList", timeTypeList);
			}
		}

		if (employeeStatusList != null && employeeStatusList.Any())
		{
			if (employeeStatusList.Count == 1)
			{
				sbSql.Where("t.[Status]=@Status");
				param.Add("@Status", employeeStatusList[0], DbType.AnsiString);
			}
			else
			{
				sbSql.Where("t.[Status] IN @StatusList");
				param.Add("@StatusList", employeeStatusList);
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
                    case "Gender":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.Gender IS NOT NULL");
                        break;

                    case "BirthDate":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.BirthDate IS NOT NULL");
                        break;

                    case "NationalIdNum":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.NationalIdNum,''))>0");
                        break;

                    case "PassportNo":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.PassportNo,''))>0");
                        break;

                    case "PhoneLine1":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.PhoneLine1,''))>0");
                        break;

                    case "PhoneLine2":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.PhoneLine2,''))>0");
                        break;

                    case "NatlCtyCode":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.NatlCtyCode IS NOT NULL");
                        break;
                    default: break;
                }
            }
        }
        #endregion

        sbSql.LeftJoin($"{CambodiaAddress.MsSqlTable} resAdd ON resAdd.IsDeleted=0 AND resAdd.Id=t.ResidentialAddressId");
		sbSql.LeftJoin($"{CambodiaAddress.MsSqlTable} corAdd ON corAdd.IsDeleted=0 AND corAdd.Id=t.CorrespondentAddressId");
		sbSql.LeftJoin($"{OrgStruct.MsSqlTable} org ON org.IsDeleted=0 AND org.Id=t.OrgStructId");

		sbSql.OrderBy("t.ObjectName ASC");
		sbSql.OrderBy("t.FullNameKh ASC");

		string sql;

		if (pgNo == 0 && pgSize == 0)
		{
			sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
		}
		else
		{
			param.Add("@PageSize", pgSize);
			param.Add("@PageNo", pgNo);

			sql = sbSql.AddTemplate($";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
				  $"SELECT t.*, resAdd.*, corAdd.*, org.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
		}

		using var cn = ConnectionFactory.GetDbConnection()!;

		List<Employee> dataList = (await cn.QueryAsync<Employee, CambodiaAddress, CambodiaAddress, OrgStruct, Employee>(
				sql, (obj, residentialAddress, correspondentAddress, orgStruct) =>
				{
					obj.ResidentialAddress = residentialAddress;
					obj.CorrespondentAddress = correspondentAddress;

					if (orgStruct != null && !string.IsNullOrEmpty(orgStruct.ObjectCode) && !string.IsNullOrEmpty(orgStruct.OrgStructTypeCode))
					{
						switch (orgStruct.OrgStructTypeCode)
						{
							case OrganizationStructureTypes.DEPARTMENT:
								obj.Department = orgStruct;
								break;
							case OrganizationStructureTypes.FUNCTION:
								obj.Function = orgStruct;
								break;
							case OrganizationStructureTypes.TEAM:
								obj.Team = orgStruct;
								break;
						}
					}
					
					return obj;
				}, param, splitOn: "Id")).AsList();

		return dataList;
	}

	public async Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		string? fullNameKh = null,
		List<string>? genderList = null,
		List<string>? maritalStatusList = null,
		int? ageFrom = null,
		int? ageTo = null,
		string? nationalIdNum = null,
		string? passportNo = null,
		string? jobTitle = null,
		string? positionTitle = null,
		string? orgStructHierarchyPath = null,
		List<string>? NatlCtyCodeList = null,
		List<int>? residentialAddressProvinceIdList = null,
		List<string>? contractTypeList = null,
		List<string>? timeTypeList = null,
		List<string>? employeeStatusList = null,
        Dictionary<string, bool>? fieldRequiredToHaveValues = null,
        List<int>? excludeIdList = null)
	{
		if (pgSize < 0)
			throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");

		#region Form Search Conditions
		if (!string.IsNullOrEmpty(objectCode))
		{
			sbSql.Where("UPPER(t.ObjectCode) LIKE '%'+UPPER(@ObjectCode)+'%'");
			param.Add("@ObjectCode", objectCode, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(objectName))
		{
			sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@ObjectName)+'%'");
			param.Add("@ObjectName", objectName, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(fullNameKh))
		{
			sbSql.Where("t.FullNameKh=@FullNameKh");
			param.Add("@FullNameKh", fullNameKh);
		}

		if (genderList != null && genderList.Any())
		{
			if (genderList.Count == 1)
			{
				sbSql.Where("t.Gender=@Gender");
				param.Add("@Gender", genderList[0], DbType.AnsiString);
			}
			else
			{
				sbSql.Where("t.Gender IN @GenderList");
				param.Add("@GenderList", genderList);
			}
		}

        if (maritalStatusList != null && maritalStatusList.Count != 0)
		{
			if (maritalStatusList.Count == 1)
			{
				sbSql.Where("t.MaritalStatus=@MaritalStatus");
				param.Add("@MaritalStatus", maritalStatusList[0], DbType.AnsiString);
			}
			else
			{
				sbSql.Where("t.MaritalStatus IN @MaritalStatusList");
				param.Add("@MaritalStatusList", maritalStatusList);
			}
		}

		if (ageFrom != null)
		{
			sbSql.Where("t.BirthDate IS NOT NULL");
			sbSql.Where("FLOOR(DATEDIFF(DAY, t.BirthDate, GETDATE()) / 365.25)>=@AgetFrom");
			param.Add("@AgetFrom", ageFrom.Value);

			if (ageTo != null)
			{
				sbSql.Where("FLOOR(DATEDIFF(DAY, t.BirthDate, GETDATE()) / 365.25)<=@AgetTo");
				param.Add("@AgetTo", ageTo.Value);
			}
		}
		else if (ageTo != null)
		{
			sbSql.Where("t.BirthDate IS NOT NULL");
			sbSql.Where("FLOOR(DATEDIFF(DAY, t.BirthDate, GETDATE()) / 365.25)>=@AgetTo");
			param.Add("@AgetTo", ageTo.Value);
		}

		if (!string.IsNullOrEmpty(nationalIdNum))
		{
			sbSql.Where("t.NationalIdNum LIKE '%'+@NationalIdNum+'%'");
			param.Add("@NationalIdNum", nationalIdNum, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(passportNo))
		{
			sbSql.Where("t.PassportNo LIKE '%'+@PassportNo+'%'");
			param.Add("@PassportNo", passportNo, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(jobTitle))
		{
			sbSql.Where("LOWER(t.JobTitle) LIKE '%'+LOWER(@JobTitle)+'%'");
			param.Add("@JobTitle", jobTitle, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(positionTitle))
		{
			sbSql.Where("LOWER(t.JobPositionName) LIKE '%'+LOWER(@JobPositionName)+'%'");
			param.Add("@JobPositionName", positionTitle, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(orgStructHierarchyPath))
		{
			sbSql.Where("org.HierarchyPath LIKE LOWER(@OrgStructHierarhcyPath)+'%'");
			param.Add("@OrgStructHierarhcyPath", orgStructHierarchyPath, DbType.AnsiString);
		}

        if (NatlCtyCodeList != null && NatlCtyCodeList.Count != 0)
		{
			if (NatlCtyCodeList.Count == 1)
			{
				sbSql.Where("t.NatlCtyCode=@NatlCtyCode");
				param.Add("@NatlCtyCode", NatlCtyCodeList[0], DbType.AnsiString);
			}
			else
			{
				sbSql.Where("t.NatlCtyCode IN @NatlCtyCodeList");
				param.Add("@NatlCtyCodeList", NatlCtyCodeList);
			}
		}

        if (residentialAddressProvinceIdList != null && residentialAddressProvinceIdList.Count != 0)
		{
			if (residentialAddressProvinceIdList.Count == 1)
			{
				sbSql.Where("t.ResidentialAddressId IS NOT NULL");
				sbSql.Where("resAdd.CambodiaProvinceId=@ResAddrCambodiaProvinceId");
				param.Add("@ResAddrCambodiaProvinceId", residentialAddressProvinceIdList[0]);
			}
			else
			{
				sbSql.Where("t.ResidentialAddressId IS NOT NULL");
				sbSql.Where("resAdd.CambodiaProvinceId IN @ResAddrCambodiaProvinceIdList");
				param.Add("@ResAddrCambodiaProvinceIdList", residentialAddressProvinceIdList);
			}
		}

        if (contractTypeList != null && contractTypeList.Count != 0)
		{
			if (contractTypeList.Count == 1)
			{
				sbSql.Where("t.ContractType=@ContractType");
				param.Add("@ContractType", contractTypeList[0], DbType.AnsiString);
			}
			else
			{
				sbSql.Where("t.ContractType IN @ContractTypeList");
				param.Add("@ContractTypeList", contractTypeList);
			}
		}

        if (timeTypeList != null && timeTypeList.Count != 0)
		{
			if (timeTypeList.Count == 1)
			{
				sbSql.Where("t.TimeType=@TimeType");
				param.Add("@TimeType", timeTypeList[0], DbType.AnsiString);
			}
			else
			{
				sbSql.Where("t.TimeType IN @TimeTypeList");
				param.Add("@TimeTypeList", timeTypeList);
			}
		}

		if (employeeStatusList != null && employeeStatusList.Any())
		{
			if (employeeStatusList.Count == 1)
			{
				sbSql.Where("t.[Status]=@Status");
				param.Add("@Status", employeeStatusList[0], DbType.AnsiString);
			}
			else
			{
				sbSql.Where("t.[Status] IN @StatusList");
				param.Add("@StatusList", employeeStatusList);
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
                    case "Gender":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.Gender IS NOT NULL");
                        break;

                    case "BirthDate":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.BirthDate IS NOT NULL");
                        break;

                    case "NationalIdNum":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.NationalIdNum,''))>0");
                        break;

                    case "PassportNo":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.PassportNo,''))>0");
                        break;

                    case "PhoneLine1":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.PhoneLine1,''))>0");
                        break;

                    case "PhoneLine2":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.PhoneLine2,''))>0");
                        break;

                    case "NatlCtyCode":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.NatlCtyCode IS NOT NULL");
                        break;
                    default: break;
                }
            }
        }
        #endregion

        sbSql.LeftJoin($"{CambodiaAddress.MsSqlTable} resAdd ON resAdd.IsDeleted=0 AND resAdd.Id=t.ResidentialAddressId");
		sbSql.LeftJoin($"{CambodiaAddress.MsSqlTable} corAdd ON corAdd.IsDeleted=0 AND corAdd.Id=t.CorrespondentAddressId");
		sbSql.LeftJoin($"{OrgStruct.MsSqlTable} org ON org.IsDeleted=0 AND org.Id=t.OrgStructId");

		string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

		using var cn = ConnectionFactory.GetDbConnection()!;

		decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
		int pageCount = pgSize == 0 ? 1 : (int)Math.Ceiling(recordCount / pgSize);

		DataPagination pagination = new()
		{
			ObjectType = typeof(Employee).Name,
			PageSize = pgSize,
			PageCount = pageCount,
			RecordCount = (int)recordCount
		};

		return pagination;
	}

	public override List<string> GetSearchOrderbBy()
	{
		return ["ISNULL(t.ObjectName,'ZZZZ') ASC", "t.ObjectNameKh"];
	}
}