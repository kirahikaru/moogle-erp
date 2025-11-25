namespace DataLayer.Repos.SysCore;

public interface ILocationRepos : IBaseRepos<Location>
{
	Task<Location?> GetFullAsync(int objId);
	Task<List<Location>> GetChildrenAsync(int objId, string hierarchyPath);
	Task<List<Location>> GetByLocationTypeAsync(int locationTypeId);

	Task<List<DropDownListItem>> GetValidParentsAsync(string? hierarchyPath, string? searchText = null);

	Task<List<DropdownSelectItem>> GetForDropdownByTypeAsync(string locationTypeCode, string? searchText = null, int pgSize = 0, int pgNo = 0);

	Task<List<Location>> SearchAsync(
			int pgSize = 0, int pgNo = 0,
			string? objectCode = null,
			string? objectName = null,
			List<int>? locationTypeIdList = null,
			string? localName = null,
			string? refNum = null);

	Task<DataPagination> GetSearchPaginationAsync(
			int pgSize = 0,
			string? objectCode = null,
			string? objectName = null,
			List<int>? locationTypeIdList = null,
			string? localName = null,
			string? refNum = null);
}

public class LocationRepos(IDbContext dbContext) : BaseRepos<Location>(dbContext, Location.DatabaseObject), ILocationRepos
{
	public async Task<Location?> GetFullAsync(int objId)
    {
        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.Id=@Id");
        sbSql.LeftJoin($"{DbObject.MsSqlTable} p ON p.Id=t.ParentId");
        sbSql.LeftJoin($"{LocationType.MsSqlTable} lt ON lt.Id=t.LocationTypeId");

        param.Add("@Id", objId);

        using var cn = DbContext.DbCxn;
        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;
        Location? data = (await cn.QueryAsync<Location?, Location, LocationType, Location?>(sql, 
                                (obj, l, type) =>
                                {
                                    if (obj != null) 
                                    {
                                        obj.Parent = l;
                                        obj.LocationType = type;
                                    }

                                    return obj;
                                }, param, splitOn: "Id")).FirstOrDefault();

        return data;
    }
    public async Task<List<Location>> GetChildrenAsync(int objId, string hierarchyPath)
    {
        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.Id<>@Id");
        sbSql.Where("t.HierarchyPath IS NOT NULL");
        sbSql.Where("t.HierarchyPath LIKE @HierarchyPath+'>%'");

        sbSql.LeftJoin($"{DbObject.MsSqlTable} p ON p.Id=t.ParentId");
        sbSql.LeftJoin($"{LocationType.MsSqlTable} lt ON lt.Id=t.LocationTypeId");

        param.Add("@Id", objId);
        param.Add("@HierarchyPath", hierarchyPath, DbType.AnsiString);

        using var cn = DbContext.DbCxn;

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        List<Location> dataList = (await cn.QueryAsync<Location, Location, LocationType, Location>(sql,
                                       (obj, parent, type) =>
                                       {
                                           obj.Parent = parent;
                                           obj.LocationType = type;
                                           return obj;
                                       }, param, splitOn: "Id")).ToList();
        return dataList;
    }

    public async Task<List<Location>> GetByLocationTypeAsync(int locationTypeId)
    {
        var sql = $"SELECT * FROM {Location.MsSqlTable} WHERE IsDeleted=0 AND LocationTypeId=@LocationTypeId";
        var param = new { LocationTypeId = locationTypeId };

        using var cn = DbContext.DbCxn;

        return (await cn.QueryAsync<Location>(sql, param).ConfigureAwait(false)).AsList();
    }

    public async Task<List<DropDownListItem>> GetValidParentsAsync(string? hierarchyPath, string? searchText = null)
    {
        if (string.IsNullOrEmpty(hierarchyPath))
			return [];

        SqlBuilder sbSql = new();

        sbSql.Select("t.Id")
            .Select("'ObjectType'='LocationType'")
            .Select("t.ObjectCode")
            .Select("t.ObjectName")
            .Select("t.HierarchyPath");

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.ObjectCode<>@ObjectCode");
        sbSql.Where("t.HierarchyPath NOT LIKE @HierarchyPath+'%'");

        DynamicParameters param = new();

        param.Add("@HierarchyPath", hierarchyPath, DbType.AnsiString);

        if (!string.IsNullOrEmpty(searchText))
        {
            sbSql.Where("UPPER(t.ObjectName) LIKE '%'+@SearchText+'%'");
            param.Add("@SearchText", searchText.ToUpper(), DbType.AnsiString);
        }

        sbSql.OrderBy("t.ObjectName ASC");

        string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;

        using var cn = DbContext.DbCxn;

        List<DropDownListItem> dataList = (await cn.QueryAsync<DropDownListItem>(sql, param)).ToList();
        return dataList;
    }

    public async Task<List<DropdownSelectItem>> GetForDropdownByTypeAsync(string locationTypeCode, string? searchText = null, int pgSize = 0, int pgNo = 0)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Select("t.Id");
        sbSql.Select("'Key'=t.ObjectCode");
        sbSql.Select("'Value'=t.ObjectName");

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("lt.ObjectCode=@LocationTypeCode");
        param.Add("@LocationTypeCode", locationTypeCode, DbType.AnsiString);

        if (!string.IsNullOrEmpty(searchText))
        {
            sbSql.Where("UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%'");
            param.Add("@SearchText", searchText, DbType.AnsiString);
        }

        sbSql.LeftJoin($"{LocationType.MsSqlTable} lt ON lt.Id=t.LocationTypeId");

        sbSql.OrderBy("t.ObjectName ASC");

        string sql;

        if (pgNo == 0 && pgSize == 0)
        {
            sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
        }
        else
        {
            param.Add("@PageSize", pgSize);
            param.Add("@PageNo", pgNo);
            sql = sbSql.AddTemplate($";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                    $"SELECT /**select**/ FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        List<DropdownSelectItem> data = (await cn.QueryAsync<DropdownSelectItem>(sql, param)).AsList();

        return data;
    }


    public override async Task<List<Location>> QuickSearchAsync(int pgSize = 0, int pgNo = 0, string? searchText = null, List<int>? excludeIdList = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(searchText))
        {
            sbSql.Where("(UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%')");
            param.Add("@SearchText", searchText, DbType.AnsiString);
        }

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }
        #endregion

        sbSql.OrderBy("t.ObjectName ASC");

        sbSql.LeftJoin($"{Location.MsSqlTable} pr ON pr.Id=t.ParentId");
        sbSql.LeftJoin($"{LocationType.MsSqlTable} lt ON lt.Id=t.LocationTypeId");

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
                $"SELECT t.*, pr.*, lt.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<Location, Location, LocationType, Location>(sql,
                                            (obj, p, type) =>
                                            {
                                                obj.Parent = p;
                                                obj.LocationType = type;

                                                return obj;
                                            }, param, splitOn: "Id")).AsList();

        return dataList;
    }

    public async Task<List<Location>> SearchAsync(
            int pgSize = 0, int pgNo = 0,
            string? objectCode = null,
            string? objectName = null,
            List<int>? locationTypeIdList = null,
            string? localName = null,
            string? refNum = null)
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
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+UPPER(@ObjectName)+'%'");
            param.Add("@ObjectName", objectName, DbType.AnsiString);
        }

        if (locationTypeIdList != null && locationTypeIdList.Any())
        {
            if (locationTypeIdList.Count == 1)
            {
                sbSql.Where("t.LocationTypeId=@LocationTypeId");
                param.Add("@LocationTypeId", locationTypeIdList[0]);
            }
            else
            {
                sbSql.Where("t.LocationTypeId IN @LocationTypeIdList");
                param.Add("@LocationTypeIdList", locationTypeIdList);
            }
        }

        if (!string.IsNullOrEmpty(localName))
        {
            sbSql.Where("t.LocalName LIKE '%'+@LocalName+'%'");
            param.Add("@LocalName", localName);
        }

        if (!string.IsNullOrEmpty(refNum))
        {
            sbSql.Where("t.ReferenceNumber LIKE '%'+@ReferenceNumber+'%'");
            param.Add("@ReferenceNumber", refNum, DbType.AnsiString);
        }
        #endregion

        sbSql.LeftJoin($"{Location.MsSqlTable} pr ON pr.Id=t.ParentId");
        sbSql.LeftJoin($"{LocationType.MsSqlTable} lt ON lt.Id=t.LocationTypeId");

        sbSql.OrderBy("t.ObjectName ASC");

        string sql;

        if (pgNo == 0 && pgSize == 0)
        {
            sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;
        }
        else
        {
            param.Add("@PageSize", pgSize);
            param.Add("@PageNo", pgNo);
            sql = sbSql.AddTemplate($";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                                              $"SELECT t.*, pr.*, lt.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<Location, Location, LocationType, Location>(sql, 
                                            (obj, p, type) => 
                                            {
                                                obj.Parent = p;
                                                obj.LocationType = type;

                                                return obj;
                                            }, param, splitOn: "Id")).AsList();

        return dataList;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
            int pgSize = 0,
            string? objectCode = null,
            string? objectName = null,
            List<int>? locationTypeIdList = null,
            string? localName = null,
            string? refNum = null)
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
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+UPPER(@ObjectName)+'%'");
            param.Add("@ObjectName", objectName, DbType.AnsiString);
        }

		if (locationTypeIdList != null && locationTypeIdList.Count != 0)
        {
            if (locationTypeIdList.Count == 1)
            {
                sbSql.Where("t.LocationTypeId=@LocationTypeId");
                param.Add("@LocationTypeId", locationTypeIdList[0]);
            }
            else
            {
                sbSql.Where("t.LocationTypeId IN @LocationTypeIdList");
                param.Add("@LocationTypeIdList", locationTypeIdList);
            }
        }

        if (!string.IsNullOrEmpty(localName))
        {
            sbSql.Where("t.LocalName LIKE '%'+@LocalName+'%'");
            param.Add("@LocalName", localName);
        }

        if (!string.IsNullOrEmpty(refNum))
        {
            sbSql.Where("t.ReferenceNumber LIKE '%'+@ReferenceNumber+'%'");
            param.Add("@ReferenceNumber", refNum, DbType.AnsiString);
        }
        #endregion

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = pgSize == 0 ? 1 : (int)Math.Ceiling(recordCount / pgSize);

        DataPagination pagingResult = new()
        {
            ObjectType = typeof(Country).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagingResult;
    }
}