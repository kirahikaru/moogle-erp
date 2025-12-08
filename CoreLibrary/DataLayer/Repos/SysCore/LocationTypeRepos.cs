namespace DataLayer.Repos.SysCore;

public interface ILocationTypeRepos : IBaseRepos<LocationType>
{
	Task<List<DropDownListItem>> GetValidParentsAsync(
		int objectId,
		string objectCode,
		string hierarchyPath,
		string? searchText = null);

	Task<List<LocationType>> GetChildsAsync(int objId, string hierarchyPath);

	Task<List<LocationType>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		string? localName = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		string? localName = null);
}

public class LocationTypeRepos(IDbContext dbContext) : BaseRepos<LocationType>(dbContext, LocationType.DatabaseObject), ILocationTypeRepos
{
	public async Task<List<LocationType>> GetChildsAsync(int objId, string hierarchyPath)
    {
        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.Id<>@Id");
        sbSql.Where("t.HierarchyPath IS NOT NULL");
        sbSql.Where("t.HierarchyPath LIKE @HierarchyPath+'>%'");
        sbSql.LeftJoin($"{LocationType.MsSqlTable} p ON p.Id=t.ParentId");

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        param.Add("@Id", objId);
        param.Add("@HierarchyPath", hierarchyPath, DbType.AnsiString);

        using var cn = DbContext.DbCxn;

        List<LocationType> dataList = (await cn.QueryAsync<LocationType, LocationType, LocationType>(sql,
                                       (obj, parent) =>
                                       {
                                           obj.Parent = parent;
                                           return obj;
                                       }, param, splitOn: "Id")).AsList();
        return dataList;
    }

	public async Task<List<DropDownListItem>> GetValidParentsAsync(
		int objectId,
		string objectCode,
		string hierarchyPath,
		string? searchText = null)
	{
		DynamicParameters param = new();
		SqlBuilder sbSql = new();

		sbSql.Select("'ObjectId'=t.Id")
			.Select("t.ObjectCode")
			.Select("t.ObjectName")
			.Select("t.HierarchyPath");

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.Id<>@ObjectId");
		param.Add("@ObjectId", objectId);

		if (objectId > 0)
		{
			sbSql.Where("t.Id<>@Id");
			param.Add("@Id", objectId);
		}

		if (!string.IsNullOrEmpty(objectCode))
		{
			sbSql.Where("t.ObjectCode<>@ObjectCode");
			param.Add("@ObjectCode", objectCode, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(hierarchyPath))
		{
			sbSql.Where("t.HierarchyPath NOT LIKE @HierarchyPath+'>%'");
			param.Add("@HierarchyPath", hierarchyPath, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(searchText))
		{
			sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@SearchText)+'%'");
			param.Add("@SearchText", searchText, DbType.AnsiString);
		}

		sbSql.LeftJoin($"{OrgStructType.MsSqlTable} ost ON ost.Id=t.OrgStructTypeId");

		sbSql.OrderBy("t.HierarchyPath");
		sbSql.OrderBy("t.ObjectName");

		using var cn = DbContext.DbCxn;

		string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

		var dataList = (await cn.QueryAsync<DropDownListItem>(sql, param)).AsList();

		return dataList;
	}

	public override async Task<KeyValuePair<int, IEnumerable<LocationType>>> SearchNewAsync(
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

		sbSql.LeftJoin($"{LocationType.MsSqlTable} pr ON pr.Id=t.ParentId");

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

			sql = sbSql.AddTemplate(
				$";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
				$"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
		}

		using var cn = DbContext.DbCxn;

		var dataList = await cn.QueryAsync<LocationType, LocationType, LocationType>(sql, (obj, pr) =>
		{
			obj.Parent = pr;
			return obj;
		}, param, splitOn: "Id");

		string countSql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int count = await cn.ExecuteScalarAsync<int>(countSql, param);

		return new KeyValuePair<int, IEnumerable<LocationType>>(count, dataList);
	}

	public async Task<List<LocationType>> SearchAsync(
        int pgSize = 0, int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
        string? localName = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

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

        if (!string.IsNullOrEmpty(localName))
        {
            sbSql.Where("t.LocalName LIKE '%'+@LocalName+'%'");
            param.Add("@LocalName", localName);
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

            sql = sbSql.AddTemplate($";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                                    $"SELECT t.* FROM {LocationType.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        List<LocationType> dataList = (await cn.QueryAsync<LocationType>(sql, param)).AsList();

        return dataList;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(int pgSize = 0, string? objectCode = null, string? objectName = null, string? localName = null)
    {
        if (pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

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

        if (!string.IsNullOrEmpty(localName))
        {
            sbSql.Where("t.LocalName LIKE '%'+@LocalName+'%'");
            param.Add("@LocalName", localName);
        }
        #endregion

        var sql = $"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/";

        using var cn = DbContext.DbCxn;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = pgSize == 0 ? 1 : (int)Math.Ceiling(recordCount / pgSize);

        DataPagination pagination = new()
        {
            ObjectType = typeof(CambodiaProvince).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }
}