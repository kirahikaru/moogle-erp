using DataLayer.Models.HomeInventory;

namespace DataLayer.Repos.HomeInventory;

public interface IOwnedItemCategoryRepos : IBaseRepos<OwnedItemCategory>
{
	Task<List<OwnedItemCategory>> GetChildrenAsync(int objId, string hierarchyPath, bool getOnlyDirectChild = true);
	Task<List<DropDownListItem>> GetValidParentsAsync(string? objectCode, string? hierarchyPath, string? searchText = null);

	Task<List<BasicObjectSelectListItem>> GetValidParentAsync(int objectId, string objectCode, int? includingId = null);
}

public class OwnedItemCategoryRepos(IDbContext dbContext) : BaseRepos<OwnedItemCategory>(dbContext, OwnedItemCategory.DatabaseObject), IOwnedItemCategoryRepos
{
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
        List<DropDownListItem> dataList = (await cn.QueryAsync<DropDownListItem>(sql, param)).ToList();
        return dataList;
    }

    public async Task<List<BasicObjectSelectListItem>> GetValidParentAsync(int objectId, string objectCode, int? includingId = null)
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