using DataLayer.Models.RMS;
using static Dapper.SqlMapper;

namespace DataLayer.Repos.RMS;

public interface ISupplierRepos : IBaseRepos<Supplier>
{
	Task<Supplier?> GetFullAsync(int id);

	Task<int> InsertFullAsync(Supplier obj);
	Task<bool> UpdateFullAsync(Supplier obj);

	Task<List<Supplier>> SearchAsync(
		int pgSize = 0,
		int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		DateTime? fromDate = null,
		DateTime? toDate = null,
		string? status = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		DateTime? fromDate = null,
		DateTime? toDate = null,
		string? status = null);

	Task<List<DropDownListItem>> GetForDropdownSelect1Async(string? searchText = null, int? includingObjId = null);
}

public class SupplierRepos(IDbContext dbContext) : BaseRepos<Supplier>(dbContext, Supplier.DatabaseObject), ISupplierRepos
{
	public async Task<Supplier?> GetFullAsync(int id)
    {
        var sql = $"SELECT * FROM {Supplier.MsSqlTable} WHERE IsDeleted=0 AND Id=@Id; " +
                  $"SELECT * FROM {SupplierBranch.MsSqlTable} WHERE IsDeleted=0 AND SupplierId=@Id; " +
                  $"SELECT * FROM {Contact.MsSqlTable} WHERE IsDeleted=0 AND LinkedObjectType=@LinkedObjectType AND LinkedObjectId=@Id; " +
                  $"SELECT * FROM {CambodiaAddress.MsSqlTable} WHERE IsDeleted=0 AND LinkedObjectType=@LinkedObjectType AND LinkedObjectId=@Id; ";

        DynamicParameters param = new();
        param.Add("@LinkedObjectType", typeof(Supplier).Name, DbType.AnsiString);
        param.Add("@Id", id);

        using var cn = DbContext.DbCxn;

        using var multi = await cn.QueryMultipleAsync(sql, param);
        Supplier? obj = multi.ReadFirstOrDefault<Supplier>();

        if (obj != null)
        {
            obj.Branches = (await multi.ReadAsync<SupplierBranch>()).AsList();
            obj.Contacts = (await multi.ReadAsync<Contact>()).AsList();
            obj.MainCambodiaAddress = await multi.ReadSingleOrDefaultAsync<CambodiaAddress>();
        }

        return obj;
    }

    public async Task<int> InsertFullAsync(Supplier obj)
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

            //DynamicParameters addressUpdateParam = new();
            //SqlBuilder sbAddressUpdateSql = new();

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

                //obj.MainAddressId = mainAddressId;
                //addressUpdateParam.Add("@MainAddressId", mainAddressId);
            }

            if (obj.MainCambodiaAddress != null && (
                obj.MainCambodiaAddress.CambodiaProvinceId is not null ||
                !string.IsNullOrEmpty(obj.MainCambodiaAddress.UnitFloor) ||
                !string.IsNullOrEmpty(obj.MainCambodiaAddress.StreetNo)))
            {
                obj.MainCambodiaAddress.CreatedUser = obj.CreatedUser;
                obj.MainCambodiaAddress.CreatedDateTime = timestamp;
                obj.MainCambodiaAddress.ModifiedUser = obj.ModifiedUser;
                obj.MainCambodiaAddress.ModifiedDateTime = timestamp;
                obj.MainCambodiaAddress.LinkedObjectId = objId;
                obj.MainCambodiaAddress.LinkedObjectType = obj.GetType().Name;

                int mainCambodiaAddressId = await cn.InsertAsync(obj.MainCambodiaAddress, tran);

                //obj.MainCambodiaAddressId = mainCambodiaAddressId;
                //addressUpdateParam.Add("@MainCambodiaAddressId", mainCambodiaAddressId);
            }

            //if (addressUpdateParam.ParameterNames.Any())
            //{
            //    addressUpdateParam.Add("@Id", objId);
            //    string addressUpdateSql = sbAddressUpdateSql.AddTemplate($"UPDATE {DbObject.MsSqlTable} SET MainAddressId=@MainAddressId, MainCambodiaAddressId=@MainCambodiaAddressId WHERE Id=@Id").RawSql;
            //    int addressUpdCount = await cn.ExecuteAsync(addressUpdateSql, addressUpdateParam);
            //}

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

            if (obj.Branches != null && obj.Branches.Any())
            {
                foreach (SupplierBranch branch in obj.Branches)
                {
                    if (branch.IsDeleted) continue;

                    branch.CreatedUser = obj.CreatedUser;
                    branch.CreatedDateTime = obj.CreatedDateTime;
                    branch.ModifiedUser = obj.ModifiedUser;
                    branch.ModifiedDateTime = obj.ModifiedDateTime;
                    branch.SupplierId = objId;
                    int branchId = await cn.InsertAsync(branch, tran);
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

    public async Task<bool> UpdateFullAsync(Supplier obj)
    {
        if (obj.Id <= 0) throw new Exception("Not exsiting object.");

        DateTime timestamp = DateTime.Now;

        using var cn = DbContext.DbCxn;

        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();

        using var tran = cn.BeginTransaction();

        try
        {
            obj.ModifiedDateTime = timestamp;

            bool isUpdated = await cn.UpdateAsync(obj, tran);

            if (!isUpdated)
                throw new Exception("Failed to insert object into database.");

            //DynamicParameters addressUpdateParam = new();
            //SqlBuilder sbAddressUpdateSql = new();

            if (obj.MainAddress != null && (
                !string.IsNullOrEmpty(obj.MainAddress.CountryCode) ||
                !string.IsNullOrEmpty(obj.MainAddress.Line1) ||
                !string.IsNullOrEmpty(obj.MainAddress.Line2) ||
                !string.IsNullOrEmpty(obj.MainAddress.Line3)))
            {
                if (obj.MainAddress.Id<=0)
                {
                    obj.MainAddress.CreatedUser = obj.CreatedUser;
                    obj.MainAddress.CreatedDateTime = timestamp;
                    obj.MainAddress.ModifiedUser = obj.ModifiedUser;
                    obj.MainAddress.ModifiedDateTime = timestamp;
                    obj.MainAddress.LinkedObjectId = obj.Id;
                    obj.MainAddress.LinkedObjectType = obj.GetType().Name;


                    int mainAddressId = await cn.InsertAsync(obj.MainAddress, tran);
                }
                else
                {
                    bool isMainAddressUpdated = await cn.UpdateAsync(obj.MainAddress, tran);
                }
            }

            if (obj.MainCambodiaAddress != null && (
                obj.MainCambodiaAddress.CambodiaProvinceId is not null ||
                !string.IsNullOrEmpty(obj.MainCambodiaAddress.UnitFloor) ||
                !string.IsNullOrEmpty(obj.MainCambodiaAddress.StreetNo)))
            {
                if (obj.MainCambodiaAddress.Id<=0)
                {
                    obj.MainCambodiaAddress.CreatedUser = obj.CreatedUser;
                    obj.MainCambodiaAddress.CreatedDateTime = timestamp;
                    obj.MainCambodiaAddress.ModifiedUser = obj.ModifiedUser;
                    obj.MainCambodiaAddress.ModifiedDateTime = timestamp;
                    obj.MainCambodiaAddress.LinkedObjectId = obj.Id;
                    obj.MainCambodiaAddress.LinkedObjectType = obj.GetType().Name;

                    int mainCambodiaAddressId = await cn.InsertAsync(obj.MainCambodiaAddress, tran);
                }
                else
                {
                    bool isMainCambodiaAddressUpdated = await cn.UpdateAsync(obj.MainCambodiaAddress, tran);
                }
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
                        contact.LinkedObjectId = obj.Id;
                        contact.LinkedObjectType = obj.GetType().Name;
                        contact.ModifiedUser = obj.ModifiedUser;
                        contact.CreatedUser = obj.ModifiedUser;
                        contact.CreatedDateTime = obj.ModifiedDateTime;
                        contact.ModifiedDateTime = obj.ModifiedDateTime;

                        int contactId = await cn.InsertAsync(contact);
                    }
                }
            }

            if (obj.Branches != null && obj.Branches.Count != 0)
            {
                foreach (SupplierBranch branch in obj.Branches)
                {
                    if (branch.Id <= 0 && !branch.IsDeleted)
                    {
                        branch.CreatedUser = obj.ModifiedUser;
                        branch.CreatedDateTime = obj.ModifiedDateTime;
                        branch.ModifiedUser = obj.ModifiedUser;
                        branch.ModifiedDateTime = obj.ModifiedDateTime;
                        branch.SupplierId = obj.Id;
                        int branchId = await cn.InsertAsync(branch, tran);
                    }
                    else if (branch.Id > 0)
                    {
                        branch.ModifiedUser = obj.ModifiedUser;
                        branch.ModifiedDateTime = obj.ModifiedDateTime;

                    }
                }
            }

            tran.Commit();
            return isUpdated;
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

    public override async Task<List<Supplier>> QuickSearchAsync(int pgSize = 0, int pgNo = 0, string? searchText = null, List<int>? excludeIdList = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        SqlBuilder sbSql = new();
        DynamicParameters param = new();

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

        sbSql.LeftJoin($"{CambodiaAddress.MsSqlTable} khAddr ON khAddr.IsDeleted=0 AND khAddr.LinkedObjectType='Supplier' AND khAddr.LinkedObjectId=t.Id");

        sbSql.OrderBy("t.ObjectName ASC, t.ObjectNameKh ASC");

        //foreach (string orderByClause in GetSearchOrderbBy())
        //    sbSql.OrderBy(orderByClause);

        string sql;

        if (pgNo == 0 && pgSize == 0)
        {
            sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
        }
        else
        {
            param.Add("@PageSize", pgSize);
            param.Add("@PageNo", pgNo);

            sql = sbSql.AddTemplate($";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                                    $"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<Supplier, CambodiaAddress, Supplier>(sql,
                            (obj, khAddr) => {
                                obj.MainCambodiaAddress = khAddr;

                                return obj;
                            }, param, splitOn: "Id")).AsList();

        return dataList;
    }

    public async Task<List<Supplier>> SearchAsync(
        int pgSize = 0,
        int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? status = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

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

        if (fromDate.HasValue)
        {
            if (toDate.HasValue)
            {
                sbSql.Where("t.StartDate IS NOT NULL");
                sbSql.Where("t.StartDate <= @ToDate");
                sbSql.Where("(t.EndDate IS NULL OR t.EndDate>=@FromDate)");
                param.Add("@FromDate", fromDate);
                param.Add("@ToDate", toDate);
            }
            else
            {
                sbSql.Where("t.StartDate IS NOT NULL");
                sbSql.Where("t.StartDate <= @FromDate");
                param.Add("@FromDate", fromDate);
            }
        }
        else if (toDate.HasValue)
        {
            sbSql.Where("t.StartDate IS NOT NULL");
            sbSql.Where("t.StartDate <= @ToDate");
            sbSql.Where("(t.EndDate IS NOT NULL OR t.EndDate>=@ToDate)");
            param.Add("@ToDate", toDate);
        }

        if (!string.IsNullOrEmpty(status))
        {
            sbSql.Where("t.[Status]=@Status");
            param.Add("@Status", status, DbType.AnsiString);
        }
        #endregion

        sbSql.LeftJoin($"{CambodiaAddress.MsSqlTable} khAddr ON khAddr.IsDeleted=0 AND khAddr.LinkedObjectType='Supplier' AND khAddr.LinkedObjectId=t.Id");

        sbSql.OrderBy("t.ObjectName ASC, t.ObjectNameKh ASC");

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
                $"SELECT * FROM {DbObject.MsSqlTable} t /***leftjoin***/ WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<Supplier, CambodiaAddress, Supplier>(sql, 
                            (obj, khAddr) => {
                                obj.MainCambodiaAddress = khAddr;

                                return obj;
                            }, param, splitOn: "Id")).AsList();

        return dataList;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        string? objectCode = null,
        string? objectName = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? status = null)
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

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

        if (fromDate.HasValue)
        {
            if (toDate.HasValue)
            {
                sbSql.Where("t.StartDate IS NOT NULL");
                sbSql.Where("t.StartDate <= @ToDate");
                sbSql.Where("(t.EndDate IS NULL OR t.EndDate>=@FromDate)");
                param.Add("@FromDate", fromDate);
                param.Add("@ToDate", toDate);
            }
            else
            {
                sbSql.Where("t.StartDate IS NOT NULL");
                sbSql.Where("t.StartDate <= @FromDate");
                param.Add("@FromDate", fromDate);
            }
        }
        else if (toDate.HasValue)
        {
            sbSql.Where("t.StartDate IS NOT NULL");
            sbSql.Where("t.StartDate <= @ToDate");
            sbSql.Where("(t.EndDate IS NOT NULL OR t.EndDate>=@ToDate)");
        }

		if (!string.IsNullOrEmpty(status))
		{
			sbSql.Where("t.[Status]=@Status");
			param.Add("@Status", status);
		}
		#endregion

		string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

		decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize));

        DataPagination pagination = new()
        {
            ObjectType = typeof(Supplier).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }

    public async Task<List<DropDownListItem>> GetForDropdownSelect1Async(string? searchText = null, int? includingObjId = null)
    {
        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Select("t.Id")
            .Select("'ObjectId'=t.Id")
            .Select("t.ObjectCode")
            .Select("t.ObjectName")
            .Select("'ObjectNameEn'=t.ObjectName")
            .Select("t.ObjectNameKh");

        sbSql.Where("t.IsDeleted=0");

        string sql;

        if (includingObjId != null)
        {
            if (!string.IsNullOrEmpty(searchText))
            {
                sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@SearchText)+'%'");
                param.Add("@SearchText", searchText);
            }

            sql = sbSql.AddTemplate($"SELECT TOP 100 /**select**/ FROM {DbObject.MsSqlTable} t /**where**/ UNION SELECT /**select**/ FROM {DbObject.MsSqlTable} t WHERE t.Id=@IncludingObjId").RawSql;
            param.Add("@IncludingObjId", includingObjId!.Value);
        }
        else
        {
            if (!string.IsNullOrEmpty(searchText))
            {
                sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@SearchText)");
                param.Add("@SearchText", searchText);
            }

            sql = sbSql.AddTemplate($"SELECT TOP 100 /**select**/ FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;
        var dataList = (await cn.QueryAsync<DropDownListItem>(sql, param)).OrderBy(x => x.ObjectName).AsList();

        return dataList;
    }
}