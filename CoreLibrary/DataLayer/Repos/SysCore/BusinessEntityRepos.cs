using MongoDB.Driver;

namespace DataLayer.Repos.SysCore;

public interface IBusinessEntityRepos : IBaseRepos<BusinessEntity>
{
	Task<BusinessEntity?> GetFullAsync(int id);
	Task<int> InsertFullAsync(BusinessEntity obj);
	Task<bool> UpdateFullAsync(BusinessEntity obj);

	Task<List<BusinessEntity>> QuickSearchAsync(
		int pgSize = 0,
		int pgNo = 0,
		string? searchText = null,
		bool? showCustomerOrNonCustomer = null,
		Dictionary<string, bool>? fieldRequiredToHaveValues = null,
		List<int>? excludeIdList = null);

	Task<DataPagination> GetQuickSearchPaginationAsync(
		int pgSize = 0,
		string? searchText = null,
		bool? showCustomerOrNonCustomer = null,
		Dictionary<string, bool>? fieldRequiredToHaveValues = null,
		List<int>? excludeIdList = null);

	Task<List<BusinessEntity>> FindAsync(int? objId,
		string? objectName,
		string? objectNameKh,
		string? licenseNo,
		string? registrationNo,
		string? baseCountryCode);

	Task<List<BusinessEntity>> SearchAsync(
		int pgSize = 0,
		int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		string? licenceNo = null,
		string? registrationNo = null,
		List<string>? entityTypeList = null,
		List<int>? businessSectorIdList = null,
		List<int>? industryIdList = null,
		List<string>? baseCountryCodeList = null,
		DateTime? registrationDateFrom = null,
		DateTime? registrationDateTo = null,
		bool? showCustomerOrNonCustomer = null,
		string? customerId = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		string? licenceNo = null,
		string? registrationNo = null,
		List<string>? entityTypeList = null,
		List<int>? businessSectorIdList = null,
		List<int>? industryIdList = null,
		List<string>? baseCountryCodeList = null,
		DateTime? registrationDateFrom = null,
		DateTime? registrationDateTo = null,
		bool? showCustomerOrNonCustomer = null,
		string? customerId = null);
}

public class BusinessEntityRepos(IDbContext dbContext) : BaseRepos<BusinessEntity>(dbContext, BusinessEntity.DatabaseObject), IBusinessEntityRepos
{
	public async Task<BusinessEntity?> GetFullAsync(int id)
    {
        /*
         SELECT * FROM {DbObject.MsSqlTable} t 
         LEFT JOIN {Country.MsSqlTable} cty ON cty.IsDeleted=0 AND cty.ObjectCode=t.BaseCountryCode
         LEFT JOIN {BusinessSector.MsSqlTable} bs ON bs.Id=t.BusinessSectorId
         LEFT JOIN {Industry.MsSqlTable} i ON i.Id=t.IndustryId
         WHERE t.IsDeleted=0 AND t.Id=@Id
         */
        SqlBuilder sbSql = new();
        sbSql.LeftJoin($"{Country.MsSqlTable} cty ON cty.IsDeleted=0 AND cty.ObjectCode=t.BaseCountryCode");
        sbSql.LeftJoin($"{BusinessSector.MsSqlTable} bs ON bs.Id=t.BusinessSectorId");
        sbSql.LeftJoin($"{Industry.MsSqlTable} i ON i.Id=t.IndustryId");

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.Id=@Id");

        sbSql.OrderBy("t.ObjectName ASC");

        var sbSqlTempl = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/");


        using var cn = DbContext.DbCxn;

        List<BusinessEntity> dataList = (await cn.QueryAsync<BusinessEntity, Country, BusinessSector, Industry, BusinessEntity>(sbSqlTempl.RawSql,
                                                (obj, country, sector, industry) =>
                                                {
                                                    obj.BaseCountry = country;
                                                    obj.BusinessSector = sector;
                                                    obj.Industry = industry;

                                                    return obj;
                                                }, new { Id=id }, splitOn: "Id")).AsList();

        if (dataList != null && dataList.Any())
            return dataList[0];
        else
            return null;
    }

    public async Task<List<BusinessEntity>> FindAsync(int? objId,
        string? objectName,
        string? objectNameKh,
        string? licenseNo,
        string? registrationNo,
        string? baseCountryCode)
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

        if (!string.IsNullOrEmpty(objectName))
        {
            sbNameEn.Append("t.ObjectName=@ObjectName");
            param.Add("@ObjectName", objectName, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectNameKh))
        {
            sbNameKh.Append("t.ObjectNameKh=@ObjectNameKh");
            param.Add("@ObjectNameKh", objectNameKh);
        }

        StringBuilder sbSearchCondition = new();

        if (sbNameEn.Length > 0)
            sbSearchCondition.Append($"(({sbNameEn})");

        if (sbNameKh.Length > 0)
            sbSearchCondition.Append(sbSearchCondition.Length > 0 ? $" OR ({sbNameKh})" : $"(({sbNameKh})");

        if (!string.IsNullOrEmpty(licenseNo))
        {
            if (sbNameEn.Length > 0)
                sbNameEn.Append(" AND t.LicenceNo=@LicenceNo");

            if (sbNameKh.Length > 0)
                sbNameKh.Append(" AND t.LicenceNo=@LicenceNo");

            param.Add("@LicenceNo", licenseNo, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(registrationNo))
        {
            if (sbNameEn.Length > 0)
                sbNameEn.Append(" AND t.RegistrationNo=@RegistrationNo");

            if (sbNameKh.Length > 0)
                sbNameKh.Append(" AND t.RegistrationNo=@RegistrationNo");

            param.Add("@RegistrationNo", registrationNo, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(baseCountryCode))
        {
            if (sbNameEn.Length > 0)
                sbNameEn.Append(" AND t.BaseCountryCode=@BaseCountryCode");

            if (sbNameKh.Length > 0)
                sbNameKh.Append(" AND t.BaseCountryCode=@BaseCountryCode");

            param.Add("@BaseCountryCode", baseCountryCode, DbType.AnsiString);
        }

        if (sbSearchCondition.Length > 0)
        {
            sbSearchCondition.Append(')');
            sbSql.Where(sbSearchCondition.ToString());
        }

        sbSql.LeftJoin($"{Country.MsSqlTable} nty ON nty.IsDeleted=0 AND nty.ObjectCode=t.BaseCountryCode");

        using var cn = DbContext.DbCxn;

        string sql = sbSql.AddTemplate($"SELECT TOP 100 * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        var dataList = (await cn.QueryAsync<BusinessEntity, Country, BusinessEntity>(sql, (p, cty) =>
        {
            p.BaseCountry = cty;
            return p;
        }, param, splitOn: "Id")).AsList();

        return dataList;
    }

    public async Task<List<BusinessEntity>> QuickSearchAsync(
        int pgSize = 0,
        int pgNo = 0,
        string? searchText = null,
        bool? showCustomerOrNonCustomer = null,
        Dictionary<string, bool>? fieldRequiredToHaveValues = null,
        List<int>? excludeIdList = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");

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
            else
            {
                sbSql.Where("UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%'");
                param.Add("@SearchText", searchText, DbType.AnsiString);
            }
        }

        if (showCustomerOrNonCustomer is not null)
        {
            if (showCustomerOrNonCustomer!.Value)
            {
                sbSql.Where("t.CustomerId IS NOT NULL");
                sbSql.Where("t.CustomerObjectCode IS NOT NULL");
            }
            else
            {
                sbSql.Where("t.CustomerId IS NULL");
            }
        }

        if (fieldRequiredToHaveValues != null && fieldRequiredToHaveValues.Keys.Count > 0)
        {
            foreach (string key in fieldRequiredToHaveValues.Keys)
            {
                switch (key)
                {
                    case "LicenceNo":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.LicenceNo,''))>0");
                        break;
                    case "RegistrationDate":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.RegistrationDate IS NOT NULL");
                        break;
                    case "RegistrationNo":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.RegistrationNo,''))>0");
                        break;
                    case "PhoneLine1":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.PhoneLine1,''))>0");
                        break;
                    case "PhoneLine2":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.PhoneLine2,''))>0");
                        break;
                    case "BaseCountryCode":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.BaseCountryCode IS NOT NULL");
                        break;
                    default: break;
                }
            }
        }

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }
        #endregion

        sbSql.LeftJoin($"{BusinessSector.MsSqlTable} bs ON bs.Id=t.BusinessSectorId");
        sbSql.LeftJoin($"{Industry.MsSqlTable} i ON i.Id=t.IndustryId");
        sbSql.LeftJoin($"{Country.MsSqlTable} cty ON cty.IsDeleted=0 AND cty.ObjectCode=t.BaseCountryCode");

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
                $"SELECT t.*, bs.*, i.*, cty.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        List<BusinessEntity> dataList = (await cn.QueryAsync<BusinessEntity, BusinessSector, Industry, Country, BusinessEntity>(sql,
                                                (obj, sector, industry, country) =>
                                                {
                                                    obj.BusinessSector = sector;
                                                    obj.Industry = industry;
                                                    obj.BaseCountry = country;

                                                    return obj;
                                                }, param, splitOn: "Id")).AsList();

        return dataList;
    }

    public async Task<DataPagination> GetQuickSearchPaginationAsync(
        int pgSize = 0,
        string? searchText = null,
        bool? showCustomerOrNonCustomer = null,
        Dictionary<string, bool>? fieldRequiredToHaveValues = null,
        List<int>? excludeIdList = null)
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");

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
            else
            {
                sbSql.Where("UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%'");
                param.Add("@SearchText", searchText, DbType.AnsiString);
            }
        }

        if (showCustomerOrNonCustomer is not null)
        {
            if (showCustomerOrNonCustomer!.Value)
            {
                sbSql.Where("t.CustomerId IS NOT NULL");
                sbSql.Where("t.CustomerObjectCode IS NOT NULL");
            }
            else
            {
                sbSql.Where("t.CustomerId IS NULL");
            }
        }

        if (fieldRequiredToHaveValues != null && fieldRequiredToHaveValues.Keys.Count > 0)
        {
            foreach (string key in fieldRequiredToHaveValues.Keys)
            {
                switch (key)
                {
                    case "LicenceNo":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.LicenceNo,''))>0");
                        break;
                    case "RegistrationDate":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.RegistrationDate IS NOT NULL");
                        break;
                    case "RegistrationNo":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.RegistrationNo,''))>0");
                        break;
                    case "PhoneLine1":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.PhoneLine1,''))>0");
                        break;
                    case "PhoneLine2":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.PhoneLine2,''))>0");
                        break;
                    case "BaseCountryCode":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.BaseCountryCode IS NOT NULL");
                        break;
                    default: break;
                }
            }
        }

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }
        #endregion

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize));

        DataPagination pagination = new()
        {
            ObjectType = typeof(BusinessEntity).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }

    public override async Task<List<BusinessEntity>> QuickSearchAsync(int pgSize = 0, int pgNo = 0, string? searchText = null, List<int>? excludeIdList = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");

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
            else
            {
                sbSql.Where("UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%'");
                param.Add("@SearchText", searchText, DbType.AnsiString);
            }
        }

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }
        #endregion

        sbSql.LeftJoin($"{BusinessSector.MsSqlTable} bs ON bs.Id=t.BusinessSectorId");
        sbSql.LeftJoin($"{Industry.MsSqlTable} i ON i.Id=t.IndustryId");
        sbSql.LeftJoin($"{Country.MsSqlTable} cty ON cty.IsDeleted=0 AND cty.ObjectCode=t.BaseCountryCode");

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
                $"SELECT t.*, bs.*, i.*, cty.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        List<BusinessEntity> dataList = (await cn.QueryAsync<BusinessEntity, BusinessSector, Industry, Country, BusinessEntity>(sql, 
                                                (obj, sector, industry, country) =>
                                                {
                                                    obj.BusinessSector = sector;
                                                    obj.Industry = industry;
                                                    obj.BaseCountry = country;

                                                    return obj;
                                                }, param, splitOn: "Id")).AsList();

        return dataList;
    }

    public async Task<int> InsertFullAsync(BusinessEntity obj)
    {
        DateTime timestamp = DateTime.Now;

        using var cn = DbContext.DbCxn;

        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();

        using var tran = cn.BeginTransaction();

        try
        {
            obj.CreatedDateTime = timestamp;
            obj.ModifiedDateTime = timestamp;

            int objId = await cn.InsertAsync(obj, tran);

            if (objId <= 0)
                throw new Exception("Failed to insert object into database.");

            obj.Id = objId;

            DynamicParameters addressUpdateParam = new();
            SqlBuilder sbAddressUpdateSql = new();

            if (obj.MainAddress != null && (
                !string.IsNullOrEmpty(obj.MainAddress.CountryCode) ||
                !string.IsNullOrEmpty(obj.MainAddress.Line1) ||
                !string.IsNullOrEmpty(obj.MainAddress.Line2) ||
                !string.IsNullOrEmpty(obj.MainAddress.Line3)))
            {
                obj.MainAddress.CreatedUser = obj.CreatedUser;
                obj.MainAddress.CreatedDateTime = timestamp;
                obj.MainAddress.ModifiedUser = obj.ModifiedUser;
                obj.MainAddress.ModifiedDateTime = timestamp;
                obj.MainAddress.LinkedObjectId = objId;
                obj.MainAddress.LinkedObjectType = obj.GetType().Name;

                int mainAddressId = await cn.InsertAsync(obj.MainAddress, tran);

                obj.MainAddressId = mainAddressId;
                addressUpdateParam.Add("@MainAddressId", mainAddressId);
            }

            if (obj.MainLocalAddress != null && (
                obj.MainLocalAddress.CambodiaProvinceId is not null ||
                !string.IsNullOrEmpty(obj.MainLocalAddress.UnitFloor) ||
                !string.IsNullOrEmpty(obj.MainLocalAddress.StreetNo)))
            {
                obj.MainLocalAddress.CreatedUser = obj.CreatedUser;
                obj.MainLocalAddress.CreatedDateTime = timestamp;
                obj.MainLocalAddress.ModifiedUser = obj.ModifiedUser;
                obj.MainLocalAddress.ModifiedDateTime = timestamp;
                obj.MainLocalAddress.LinkedObjectId = objId;
                obj.MainLocalAddress.LinkedObjectType = obj.GetType().Name;

                int mainCambodiaAddressId = await cn.InsertAsync(obj.MainLocalAddress, tran);

                obj.MainCambodiaAddressId = mainCambodiaAddressId;
                addressUpdateParam.Add("@MainCambodiaAddressId", mainCambodiaAddressId);
            }

            if (addressUpdateParam.ParameterNames.Any())
            {
                addressUpdateParam.Add("@Id", objId);
                string addressUpdateSql = sbAddressUpdateSql.AddTemplate($"UPDATE {DbObject.MsSqlTable} SET MainAddressId=@MainAddressId, MainCambodiaAddressId=@MainCambodiaAddressId WHERE Id=@Id").RawSql;
                int addressUpdCount = await cn.ExecuteAsync(addressUpdateSql, addressUpdateParam);
            }

            if (obj.Contacts != null && obj.Contacts.Any())
            {
                foreach (Contact contact in obj.Contacts)
                {
                    if (contact.Id > 0)
                    {
                        contact.ModifiedUser = obj.ModifiedUser;
                        contact.ModifiedDateTime = obj.ModifiedDateTime;
                        bool isContactUpdated = await cn.UpdateAsync(contact);
                    }
                    else if (contact.Id > 0)
                    {
                        contact.LinkedObjectId = objId;
                        contact.LinkedObjectType = obj.GetType().Name;
                        contact.ModifiedUser = obj.ModifiedUser;
                        contact.CreatedUser = obj.ModifiedUser;
                        contact.CreatedDateTime = obj.ModifiedDateTime;
                        contact.ModifiedDateTime = obj.ModifiedDateTime;

                        int contactId = await cn.InsertAsync(contact);
                    }
                }
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

    public async Task<bool> UpdateFullAsync(BusinessEntity obj)
    {
        using var cn = DbContext.DbCxn;

        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();

        using var tran = cn.BeginTransaction();

        try
        {
            if (obj.Id <= 0)
                throw new Exception("Object is not existing.");

            DynamicParameters addressUpdateParam = new();
            SqlBuilder sbAddressUpdateSql = new();

            if (obj.MainAddress != null && (
                !string.IsNullOrEmpty(obj.MainAddress.CountryCode) ||
                !string.IsNullOrEmpty(obj.MainAddress.Line1) ||
                !string.IsNullOrEmpty(obj.MainAddress.Line2) ||
                !string.IsNullOrEmpty(obj.MainAddress.Line3)))
            {
                obj.MainAddress.ModifiedUser = obj.ModifiedUser;
                obj.MainAddress.ModifiedDateTime = obj.ModifiedDateTime;
                if (obj.MainAddressId.HasValue)
                {
                    bool isMainAddressUpdated = await cn.UpdateAsync(obj.MainAddress, tran);
                }
                else
                {
                    obj.MainAddress.CreatedUser = obj.CreatedUser;
                    obj.MainAddress.CreatedDateTime = obj.CreatedDateTime;
                    obj.MainAddress.LinkedObjectId = obj.Id;
                    obj.MainAddress.LinkedObjectType = obj.GetType().Name;
                    int mainAddressId = await cn.InsertAsync(obj.MainAddress, tran);

                    if (mainAddressId > 0)
                        obj.MainAddressId = mainAddressId;
                }
            }

            if (obj.MainLocalAddress != null && (
                obj.MainLocalAddress.CambodiaProvinceId is not null ||
                !string.IsNullOrEmpty(obj.MainLocalAddress.UnitFloor) ||
                !string.IsNullOrEmpty(obj.MainLocalAddress.StreetNo)))
            {
                obj.MainLocalAddress.ModifiedUser = obj.ModifiedUser;
                obj.MainLocalAddress.ModifiedDateTime = obj.ModifiedDateTime;

                if (obj.MainCambodiaAddressId.HasValue)
                {
                    bool isMainCambodiaAddressUpdated = await cn.UpdateAsync(obj.MainLocalAddress, tran);
                }
                else
                {
                    obj.MainLocalAddress.CreatedUser = obj.CreatedUser;
                    obj.MainLocalAddress.CreatedDateTime = obj.ModifiedDateTime;

                    obj.MainLocalAddress.LinkedObjectId = obj.Id;
                    obj.MainLocalAddress.LinkedObjectType = obj.GetType().Name;

                    int mainCambodiaAddressId = await cn.InsertAsync(obj.MainLocalAddress, tran);

                    if (mainCambodiaAddressId > 0)
                        obj.MainCambodiaAddressId = mainCambodiaAddressId;

                }
            }

            bool isUpdated = await cn.UpdateAsync(obj, tran);

            if (isUpdated)
            {
                foreach (Contact contact in obj.Contacts)
                {
                    if (contact.Id > 0)
                    {
                        contact.ModifiedUser = obj.ModifiedUser;
                        contact.ModifiedDateTime = obj.ModifiedDateTime;
                        bool isContactUpdated = await cn.UpdateAsync(contact, tran);
                    }
                    else if (contact.Id > 0)
                    {
                        contact.LinkedObjectId = obj.Id;
                        contact.LinkedObjectType = obj.GetType().Name;
                        contact.ModifiedUser = obj.ModifiedUser;
                        contact.CreatedUser = obj.ModifiedUser;
                        contact.CreatedDateTime = obj.ModifiedDateTime;
                        contact.ModifiedDateTime = obj.ModifiedDateTime;

                        int contactId = await cn.InsertAsync(contact, tran);
                    }
                }
            }
            else
                throw new Exception("Failed to update BusinessEntity.");

            tran.Commit();
            return isUpdated;
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

    public async Task<List<BusinessEntity>> SearchAsync(
        int pgSize = 0,
        int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
        string? licenceNo = null,
        string? registrationNo = null,
        List<string>? entityTypeList = null,
        List<int>? businessSectorIdList = null,
        List<int>? industryIdList = null,
        List<string>? baseCountryCodeList = null,
        DateTime? registrationDateFrom = null,
        DateTime? registrationDateTo = null,
		bool? showCustomerOrNonCustomer = null,
		string? customerId = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("t.ObjectCode LIKE @ObjectCode+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+@ObjectName+'%'");
            param.Add("@ObjectName", objectName.ToLower(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(licenceNo))
        {
            sbSql.Where("UPPER(t.LicenceNo) LIKE '%'+UPPER(@LicenceNo)+'%'");
            param.Add("@LicenceNo", licenceNo, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(registrationNo))
        {
            sbSql.Where("UPPER(t.RegistrationNo) LIKE '%'+UPPER(@RegistrationNo)+'%'");
            param.Add("@RegistrationNo", registrationNo, DbType.AnsiString);
        }

        if (entityTypeList != null && entityTypeList.Any())
        {
            if (entityTypeList.Count == 1)
            {
                sbSql.Where("t.EntityType=@EntityType");
                param.Add("@EntityType", entityTypeList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.EntityType IN @EntityTypeList");
                param.Add("@EntityTypeList", entityTypeList);
            }
        }

        if (businessSectorIdList != null && businessSectorIdList.Any())
        {
            if (businessSectorIdList.Count == 1)
            {
                sbSql.Where("t.BusinessSectorId=@BusinessSectorId");
                param.Add("@BusinessSectorId", businessSectorIdList[0]);
            }
            else
            {
                sbSql.Where("t.BusinessSectorId IN @BusinessSectorIdList");
                param.Add("@BusinessSectorIdList", businessSectorIdList);
            }
        }

        if (industryIdList != null && industryIdList.Any())
        {
            if (industryIdList.Count == 1)
            {
                sbSql.Where("t.IndustryId=@IndustryId");
                param.Add("@IndustryId", industryIdList[0]);
            }
            else
            {
                sbSql.Where("t.IndustryId IN @IndustryIdList");
                param.Add("@IndustryIdList", industryIdList);
            }
        }

        if (baseCountryCodeList != null && baseCountryCodeList.Any())
        {
            if (baseCountryCodeList.Count == 1)
            {
                sbSql.Where("t.BaseCountryCode=@BaseCountryCode");
                param.Add("@BaseCountryCode", baseCountryCodeList[0]);
            }
            else
            {
                sbSql.Where("t.BaseCountryCode IN @BaseCountryCodeList");
                param.Add("@BaseCountryCodeList", baseCountryCodeList);
            }
        }

        if (registrationDateFrom.HasValue)
        {
            sbSql.Where("t.RegistrationDate IS NOT NULL");
            sbSql.Where("t.RegistrationDate>=@RegistrationDateFrom");
            param.Add("@RegistrationDateFrom", registrationDateFrom.Value);

            if (registrationDateTo.HasValue)
            {
                sbSql.Where("t.RegistrationDate<=@RegistrationDateTo");
                param.Add("@RegistrationDateTo", registrationDateTo.Value);
            }
        }
        else if (registrationDateTo.HasValue)
        {
            sbSql.Where("t.RegistrationDate IS NOT NULL");
            sbSql.Where("t.RegistrationDate<=@RegistrationDateTo");
            param.Add("@RegistrationDateTo", registrationDateTo.Value);
        }

		if (showCustomerOrNonCustomer.HasValue)
		{
			if (showCustomerOrNonCustomer!.Value)
			{
				sbSql.Where("t.CustomerId IS NOT NULL");
				sbSql.Where("t.CustomerObjectCode IS NOT NULL");

				if (!string.IsNullOrEmpty(customerId))
				{
					sbSql.Where("t.CustomerObjectCode LIKE '%'+@CustomerObjectCode+'%'");
					param.Add("@CustomerObjectCode", customerId!, DbType.AnsiString);
				}
			}
			else
			{
				sbSql.Where("t.CustomerId IS NULL");
			}
		}
		#endregion

		sbSql.LeftJoin($"{BusinessSector.MsSqlTable} bs ON bs.Id=t.BusinessSectorId");
        sbSql.LeftJoin($"{Industry.MsSqlTable} i ON i.Id=t.IndustryId");
        sbSql.LeftJoin($"{Country.MsSqlTable} cty ON cty.IsDeleted=0 AND cty.ObjectCode=t.BaseCountryCode");

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
                $"SELECT t.*, bs.*, i.*, cty.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        List<BusinessEntity> result = (await cn.QueryAsync<BusinessEntity, BusinessSector, Industry, Country, BusinessEntity>(sql,
                                (obj, sector, industry, country) =>
                                {
                                    obj.BusinessSector = sector;
                                    obj.Industry = industry;
                                    obj.BaseCountry = country;

                                    return obj;
                                }, param, splitOn: "Id")).AsList();

        return result;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        string? objectCode = null,
        string? objectName = null,
        string? licenceNo = null,
        string? registrationNo = null,
        List<string>? entityTypeList = null,
        List<int>? businessSectorIdList = null,
        List<int>? industryIdList = null,
        List<string>? baseCountryCodeList = null,
        DateTime? registrationDateFrom = null,
        DateTime? registrationDateTo = null,
		bool? showCustomerOrNonCustomer = null,
		string? customerId = null)
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("t.ObjectCode LIKE @ObjectCode+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+@ObjectName+'%'");
            param.Add("@ObjectName", objectName.ToLower(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(licenceNo))
        {
            sbSql.Where("UPPER(t.LicenceNo) LIKE '%'+UPPER(@LicenceNo)+'%'");
            param.Add("@LicenceNo", licenceNo, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(registrationNo))
        {
            sbSql.Where("UPPER(t.RegistrationNo) LIKE '%'+UPPER(@RegistrationNo)+'%'");
            param.Add("@RegistrationNo", registrationNo, DbType.AnsiString);
        }

        if (entityTypeList != null && entityTypeList.Any())
        {
            if (entityTypeList.Count == 1)
            {
                sbSql.Where("t.EntityType=@EntityType");
                param.Add("@EntityType", entityTypeList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.EntityType IN @EntityTypeList");
                param.Add("@EntityTypeList", entityTypeList);
            }
        }

        if (businessSectorIdList != null && businessSectorIdList.Any())
        {
            if (businessSectorIdList.Count == 1)
            {
                sbSql.Where("t.BusinessSectorId=@BusinessSectorId");
                param.Add("@BusinessSectorId", businessSectorIdList[0]);
            }
            else
            {
                sbSql.Where("t.BusinessSectorId IN @BusinessSectorIdList");
                param.Add("@BusinessSectorIdList", businessSectorIdList);
            }
        }

        if (industryIdList != null && industryIdList.Any())
        {
            if (industryIdList.Count == 1)
            {
                sbSql.Where("t.IndustryId=@IndustryId");
                param.Add("@IndustryId", industryIdList[0]);
            }
            else
            {
                sbSql.Where("t.IndustryId IN @IndustryIdList");
                param.Add("@IndustryIdList", industryIdList);
            }
        }

        if (baseCountryCodeList != null && baseCountryCodeList.Any())
        {
            if (baseCountryCodeList.Count == 1)
            {
                sbSql.Where("t.BaseCountryCode=@BaseCountryCode");
                param.Add("@BaseCountryCode", baseCountryCodeList[0]);
            }
            else
            {
                sbSql.Where("t.BaseCountryCode IN @BaseCountryCodeList");
                param.Add("@BaseCountryCodeList", baseCountryCodeList);
            }
        }

        if (registrationDateFrom.HasValue)
        {
            sbSql.Where("t.RegistrationDate IS NOT NULL");
            sbSql.Where("t.RegistrationDate>=@RegistrationDateFrom");
            param.Add("@RegistrationDateFrom", registrationDateFrom.Value);

            if (registrationDateTo.HasValue)
            {
                sbSql.Where("t.RegistrationDate<=@RegistrationDateTo");
                param.Add("@RegistrationDateTo", registrationDateTo.Value);
            }
        }
        else if (registrationDateTo.HasValue)
        {
            sbSql.Where("t.RegistrationDate IS NOT NULL");
            sbSql.Where("t.RegistrationDate<=@RegistrationDateTo");
            param.Add("@RegistrationDateTo", registrationDateTo.Value);
        }

		if (showCustomerOrNonCustomer.HasValue)
		{
			if (showCustomerOrNonCustomer!.Value)
			{
				sbSql.Where("t.CustomerId IS NOT NULL");
				sbSql.Where("t.CustomerObjectCode IS NOT NULL");

				if (!string.IsNullOrEmpty(customerId))
				{
					sbSql.Where("t.CustomerObjectCode LIKE '%'+@CustomerObjectCode+'%'");
					param.Add("@CustomerObjectCode", customerId!, DbType.AnsiString);
				}
			}
			else
			{
				sbSql.Where("t.CustomerId IS NULL");
			}
		}
		#endregion

		string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize));

        DataPagination pagination = new()
        {
            ObjectType = typeof(BusinessEntity).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }
}