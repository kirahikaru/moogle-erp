using DataLayer.Models.HomeInventory;

namespace DataLayer.Repos.HomeInventory;

public interface IOwnedItemCategoryRepos : IBaseRepos<OwnedItemCategory>
{
	Task<List<OwnedItemCategory>> GetChildrenAsync(int objId, string hierarchyPath, bool getOnlyDirectChild = true);
	Task<List<DropDownListItem>> GetValidParentsAsync(string? objectCode, string? hierarchyPath, string? searchText = null);

	Task<List<BasicObjectSelectListItem>> GetValidParentsAsync(int objectId, string objectCode, int? includingId = null);
}

public class OwnedItemCategoryRepos(IDbContext dbContext) : BaseRepos<OwnedItemCategory>(dbContext, OwnedItemCategory.DatabaseObject), IOwnedItemCategoryRepos
{
	public override async Task<KeyValuePair<int, IEnumerable<OwnedItemCategory>>> SearchNewAsync(
        int pgSize = 0, int pgNo = 0, string? searchText = null, IEnumerable<SqlSortCond>? sortConds = null, IEnumerable<SqlFilterCond>? filterConds = null, List<int>? excludeIdList = null)
	{
		DynamicParameters param = new();
		SqlBuilder sbSql = new();

		sbSql.Where("t.IsDeleted=0");

		#region Form Search Conditions
		if (!string.IsNullOrEmpty(searchText))
		{
			sbSql.Where("(UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%')");
			param.Add("@SearchText", searchText);
		}

		if (excludeIdList != null && excludeIdList.Count != 0)
		{
			sbSql.Where("t.Id NOT IN @ExcludeIdList");
			param.Add("@ExcludeIdList", excludeIdList);
		}

		if (filterConds != null && filterConds.Any())
		{
			foreach (SqlFilterCond filterCond in filterConds)
			{

			}
		}

		#endregion

		sbSql.LeftJoin($"{OwnedItemCategory.MsSqlTable} pr ON pr.Id=t.ParentId");

		if (sortConds is null || !sortConds.Any())
		{
			foreach (string order in GetSearchOrderbBy())
			{
				sbSql.OrderBy(order);
			}
		}
		else
		{
			foreach (SqlSortCond sortCond in sortConds)
			{
				sbSql.OrderBy(sortCond.GetSortCommand("t"));
			}
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

		using var cn = DbContext.DbCxn;

		var dataList = await cn.QueryAsync<OwnedItemCategory, OwnedItemCategory, OwnedItemCategory>(sql,
										(obj, pr) =>
										{
                                            obj.Parent = pr;
											return obj;
										}, param, splitOn: "Id");

		string sqlCount = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int dataCount = await cn.ExecuteScalarAsync<int>(sqlCount, param);
		return new(dataCount, dataList);
	}
	public async Task<List<OwnedItemCategory>> GetChildrenAsync(int objId, string hierarchyPath, bool getOnlyDirectChild = true)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Where("t.IsDeleted=0");

        if (getOnlyDirectChild)
        {
            sbSql.Where("t.ParentId IS NOT NULL");
            sbSql.Where("t.ParentId=@ParentId");
            param.Add("@ParentId", objId);
        }
        else
        {
            sbSql.Where("t.HierarchyPath LIKE @HierarchyPath+'%'");
            sbSql.Where("t.Id<>@ObjectId");
            param.Add("@HierarchyPath", hierarchyPath, DbType.AnsiString);
            param.Add("@ObjectId", objId);
        }

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        List<OwnedItemCategory> dataList = (await cn.QueryAsync<OwnedItemCategory>(sql, param)).AsList();
        return dataList;
    }

    public async Task<List<DropDownListItem>> GetValidParentsAsync(
        string? objectCode,
        string? hierarchyPath,
        string? searchText = null)
    {
        if (string.IsNullOrEmpty(objectCode) && string.IsNullOrEmpty(hierarchyPath))
			return [];

        SqlBuilder sbSql = new();

        sbSql.Select("t.Id")
            .Select("'ObjectType'='ItemCategory'")
            .Select("t.ObjectCode")
            .Select("t.ObjectName")
            .Select("t.HierarchyPath");

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.ObjectCode<>@ObjectCode");
        sbSql.Where("t.HierarchyPath NOT LIKE @HierarchyPath+'%'");

        DynamicParameters param = new();

        param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        param.Add("@HierarchyPath", hierarchyPath, DbType.AnsiString);

        if (!string.IsNullOrEmpty(searchText))
        {
            sbSql.Where("UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%'");
            param.Add("@SearchText", searchText, DbType.AnsiString);
        }

        sbSql.OrderBy("t.ObjectName ASC");

        using var cn = DbContext.DbCxn;
        var sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        List<DropDownListItem> dataList = (await cn.QueryAsync<DropDownListItem>(sql, param)).AsList();
        return dataList;
    }

    public async Task<List<BasicObjectSelectListItem>> GetValidParentsAsync(int objectId, string objectCode, int? includingId = null)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Select("'ObjectId'=t.Id")
            .Select("t.ObjectCode")
            .Select("t.ObjectName")
            .Select("t.HierarchyPath");

        param.Add("@Id", objectId);
        param.Add("@ObjectCode", objectCode, DbType.AnsiString);

        if (includingId.HasValue)
        {
            sbSql.Where("(t.IsDeleted=0 AND t.Id<>@Id AND t.HierarchyPath NOT LIKE '%'+@ObjectCode+'%') OR t.Id=@IncludingId");
            param.Add("@IncludingId", includingId.Value);
        }
        else
        {
            sbSql.Where("t.IsDeleted=0 AND t.Id<>@Id AND t.HierarchyPath NOT LIKE '%'+@ObjectCode+'%'");
        }

        sbSql.OrderBy("t.ObjectName ASC");

        string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;

        using var cn = DbContext.DbCxn;

        List<BasicObjectSelectListItem> result = (await cn.QueryAsync<BasicObjectSelectListItem>(sql, param)).AsList();

        return result;
    }
}