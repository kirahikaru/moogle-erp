using DataLayer.Models.HomeInventory;

namespace DataLayer.Repos.HomeInventory;

public interface IMerchantRepos : IBaseRepos<Merchant>
{
	Task<Merchant?> GetFullAsync(int id);
	Task<List<Merchant>> SearchAsync(int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		string? childOfHierarchyPath = null,
		List<int>? parentIdList = null,
		List<string>? merchantTypeList = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		string? childOfHierarchyPath = null,
		List<int>? parentIdList = null,
		List<string>? merchantTypeList = null);

	Task<List<DropDownListItem>> GetValidParentAsync(int objectId, string objectCode, int? includingId);
}

public class MerchantRepos(IDbContext dbContext) : BaseRepos<Merchant>(dbContext, Merchant.DatabaseObject), IMerchantRepos
{
	public async Task<Merchant?> GetFullAsync(int id)
    {
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.Id=@Id");
        sbSql.LeftJoin($"{Merchant.MsSqlTable} pr ON pr.Id=t.ParentId");
        sbSql.LeftJoin($"{Address.MsSqlTable} addr ON addr.Id=t.AddressId");
        sbSql.LeftJoin($"{CambodiaAddress.MsSqlTable} khAddr ON khAddr.Id=t.CambodiaAddressId");

        using var cn = DbContext.DbCxn;
        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        var data = (await cn.QueryAsync<Merchant, Merchant, Address, CambodiaAddress, Merchant>(sql,
                            (obj, m, addr, khAddr) =>
                            {
                                obj.Parent = m;
                                obj.Address = addr;
                                obj.CambodiaAddress = khAddr;

                                return obj;
                            }, new { Id = id }, splitOn: "Id")).FirstOrDefault();

        return data;
    }

	public override async Task<KeyValuePair<int, IEnumerable<Merchant>>> SearchNewAsync(
		int pgSize = 0,
		int pgNo = 0,
		string? searchText = null,
		IEnumerable<SqlSortCond>? sortConds = null,
		IEnumerable<SqlFilterCond>? filterConds = null,
		List<int>? excludeIdList = null)
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
				sbSql.Where("UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%'");
				param.Add("@SearchText", searchText, DbType.AnsiString);
			}
		}

		sbSql.LeftJoin($"{Merchant.MsSqlTable} pr ON pr.Id=t.ParentId");

		if (excludeIdList != null && excludeIdList.Count > 0)
		{
			sbSql.Where("t.Id NOT IN @ExcludeIdList");
			param.Add("@ExcludeIdList", excludeIdList);
		}
		#endregion

		foreach (string orderByClause in GetSearchOrderbBy())
			sbSql.OrderBy(orderByClause);

		string sql;

		if (pgNo == 0 && pgSize == 0)
		{
			sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
		}
		else
		{
			param.Add("@PageSize", pgSize);
			param.Add("@PageNo", pgNo);

			sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) ROWS FETCH NEXT @PageSize ROW ONLY;").RawSql;
		}

		using var cn = DbContext.DbCxn;

		var dataList = await cn.QueryAsync<Merchant, Merchant, Merchant>(sql, (obj, pr) =>
		{
			obj.Parent = pr;
			return obj;
		}, param, splitOn: "Id");

		string countSql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int count = await cn.ExecuteScalarAsync<int>(countSql, param);

		return new KeyValuePair<int, IEnumerable<Merchant>>(count, dataList);
	}

	public async Task<List<Merchant>> SearchAsync(
        int pgSize = 0,
        int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
		string? childOfHierarchyPath = null,
		List<int>? parentIdList = null,
		List<string>? merchantTypeList = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();

        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("t.ObjectCode LIKE '%'+@ObjectCode+'%'");
			param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@ObjectName)+'%'");
			param.Add("@ObjectName", objectName, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(childOfHierarchyPath))
        {
            sbSql.Where("t.HierarchyPath LIKE @HierarchyPath+'>%'");
            param.Add("@HierarchyPath", childOfHierarchyPath, DbType.AnsiString);
        }

        if (parentIdList !=null && parentIdList.Any())
        {
            if (parentIdList.Count == 1)
            {
                sbSql.Where("t.ParentId LIKE @ParentId");
                param.Add("@ParentId", parentIdList[0]);
            }
            else
            {
				sbSql.Where("t.ParentId IN @ParentIdList");
				param.Add("@ParentIdList", parentIdList);
			}
        }

        if (merchantTypeList != null && merchantTypeList.Any())
        {
            if (merchantTypeList.Count == 1)
            {
                sbSql.Where("t.MerchantType=@MerchantType");
				param.Add("@MerchantType", merchantTypeList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.MerchantType IN @MerchantTypeList");
				param.Add("@MerchantTypeList", merchantTypeList);
            }
        }
        #endregion

        sbSql.OrderBy("t.ObjectName ASC");
        string sql;

        if (pgNo == 0 && pgSize == 0)
        {
			sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
        }
        else
        {
			param.Add("@PageSize", pgSize);
			param.Add("@PageNo", pgNo);
			sql = sbSql.AddTemplate($";WITH pg AS (SELECT Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                  $"SELECT t.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<Merchant>(sql, param)).AsList();

        return dataList;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        string? objectCode = null,
        string? objectName = null,
		string? childOfHierarchyPath = null,
		List<int>? parentIdList = null,
		List<string>? merchantTypeList = null)
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();

        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");

		#region Form Search Conditions
		if (!string.IsNullOrEmpty(objectCode))
		{
			sbSql.Where("t.ObjectCode LIKE '%'+@ObjectCode+'%'");
			param.Add("@ObjectCode", objectCode, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(objectName))
		{
			sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@ObjectName)+'%'");
			param.Add("@ObjectName", objectName, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(childOfHierarchyPath))
		{
			sbSql.Where("t.HierarchyPath LIKE @HierarchyPath+'>%'");
			param.Add("@HierarchyPath", childOfHierarchyPath, DbType.AnsiString);
		}

		if (parentIdList != null && parentIdList.Any())
		{
			if (parentIdList.Count == 1)
			{
				sbSql.Where("t.ParentId LIKE @ParentId");
				param.Add("@ParentId", parentIdList[0]);
			}
			else
			{
				sbSql.Where("t.ParentId IN @ParentIdList");
				param.Add("@ParentIdList", parentIdList);
			}
		}

		if (merchantTypeList != null && merchantTypeList.Any())
		{
			if (merchantTypeList.Count == 1)
			{
				sbSql.Where("t.MerchantType=@MerchantType");
				param.Add("@MerchantType", merchantTypeList[0], DbType.AnsiString);
			}
			else
			{
				sbSql.Where("t.MerchantType IN @MerchantTypeList");
				param.Add("@MerchantTypeList", merchantTypeList);
			}
		}
		#endregion

		string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;

        using var cn = DbContext.DbCxn;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize));

        DataPagination pagination = new()
        {
            ObjectType = typeof(OwnedItemCategory).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }

    public async Task<List<DropDownListItem>> GetValidParentAsync(int objectId, string objectCode, int? includingId)
    {
        SqlBuilder sbSql = new();

        sbSql.Select("'ObjectId'=t.Id")
            .Select("t.ObjectCode")
            .Select("t.ObjectName")
            .Select("'ObjectNameEn'=t.ObjectName")
            .Select("'ObjectNameEn'=t.ObjectNameKh")
            .Select("t.HierarchyPath");

        DynamicParameters param = new();
        param.Add("@Id", objectId);
        param.Add("@ObjectCode", objectCode, DbType.AnsiString);

        if (includingId.HasValue)
        {
            sbSql.Where("(t.IsDeleted=0 AND t.Id<>@Id AND t.HierarchyPath NOT LIKE '%'+@ObjectCode+'%') OR t.Id=@IncludingId");
            param.Add("@IncludingId", includingId.Value);
        }
        else
        {
            sbSql.Where("t.IsDeleted=0");
            sbSql.Where("t.Id<>@Id");
            sbSql.Where("t.HierarchyPath NOT LIKE '%'+@ObjectCode+'%'");
        }

        sbSql.OrderBy("t.ObjectName ASC");

        using var cn = DbContext.DbCxn;
        var sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        return (await cn.QueryAsync<DropDownListItem>(sql, param)).AsList();
    }
}