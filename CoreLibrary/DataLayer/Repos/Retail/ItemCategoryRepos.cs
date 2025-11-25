using DataLayer.Models.Retail;
using DataLayer.Models.SystemCore.NonPersistent;
using static Dapper.SqlMapper;

namespace DataLayer.Repos.Retail;

public interface IItemCategoryRepos : IBaseRepos<ItemCategory>
{
	Task<List<DropDownListItem>> GetValidParentsAsync(string? objectCode, string? hierarchyPath, string? searchText = null);
	Task<List<DropdownSelectItem>> GetForDropdownSelectAsync(string? searchText);
	Task<List<ItemCategory>> GetCategoryWithChildrenAsync();

	Task<List<ItemCategory>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		int? parentId = null,
		string? parentCode = null,
		string? objectCode = null,
		string? objectName = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		int? parentId = null,
		string? parentCode = null,
		string? objectCode = null,
		string? objectName = null);
}

public class ItemCategoryRepos(IConnectionFactory connectionFactory) : BaseRepos<ItemCategory>(connectionFactory, ItemCategory.DatabaseObject), IItemCategoryRepos
{
	public async Task<List<DropDownListItem>> GetValidParentsAsync(
        string? objectCode, 
        string? hierarchyPath, 
        string? searchText = null)
    {
        if (string.IsNullOrEmpty(objectCode) && string.IsNullOrEmpty(hierarchyPath))
            return new List<DropDownListItem>();

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
        
        using var cn = ConnectionFactory.GetDbConnection()!;
        var sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        List<DropDownListItem> dataList = (await cn.QueryAsync<DropDownListItem>(sql, param)).ToList();
        return dataList;
    }

    public async Task<List<DropdownSelectItem>> GetForDropdownSelectAsync(string? searchText)
    {
        string sql = $"SELECT Id, 'Key'=ObjectCode, 'Value'=ObjectName FROM {DbObject.MsSqlTable} WHERE IsDeleted=0";
        DynamicParameters param = new();

        if (!string.IsNullOrEmpty(searchText))
        {
            sql += " AND UPPER(ObjectName) LIKE '%'+@SearchText+'%'";
            param.Add("@SearchText", searchText.ToUpper(), DbType.AnsiString);
        }

        using var cn = ConnectionFactory.GetDbConnection()!;

        List<DropdownSelectItem> dataList = (await cn.QueryAsync<DropdownSelectItem>(sql, param)).AsList();
        return dataList;
    }

    public async Task<List<ItemCategory>> GetCategoryWithChildrenAsync()
    {
        var sql = $"WITH p AS (SELECT ParentId FROM {ItemCategory.MsSqlTable} WHERE IsDeleted=0 AND ParentId IS NOT NULL) " +
                  $"SELECT DISTINCT ic.* FROM {ItemCategory.MsSqlTable} ic " +
                  $"INNER JOIN p ON p.ParentId = ic.Id";

        using var cn = ConnectionFactory.GetDbConnection()!;

        var dataList = (await cn.QueryAsync<ItemCategory>(sql)).OrderBy(x => x.ObjectName).AsList();

        return dataList;
    }

	public override async Task<List<ItemCategory>> QuickSearchAsync(int pgSize = 0, int pgNo = 0, string? searchText = null, List<int>? excludeIdList = null)
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

        sbSql.LeftJoin($"{ItemCategory.MsSqlTable} pr ON pr.IsDeleted=0 AND pr.Id=t.ParentId");

		foreach (string orderByClause in GetSearchOrderbBy())
			sbSql.OrderBy(orderByClause);

		string sql;

		if (pgNo == 0 && pgSize == 0)
		{
			sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
		}
		else
		{
			param.Add("@PageSize", pgSize);
			param.Add("@PageNo", pgNo);

			sql = sbSql.AddTemplate(
				$";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
				$"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
		}

		using var cn = ConnectionFactory.GetDbConnection()!;

		var dataList = (await cn.QueryAsync<ItemCategory, ItemCategory, ItemCategory>(sql, 
                                            (obj, parent) => {
                                                obj.Parent = parent;
                                                return obj;
                                            }, param, splitOn:"Id")).AsList();

		return dataList;
	}

	public override async Task<KeyValuePair<int, IEnumerable<ItemCategory>>> SearchNewAsync(int pgSize = 0, int pgNo = 0, string? searchText = null, IEnumerable<SqlSortCond>? sortConds = null, IEnumerable<SqlFilterCond>? filterConds = null, List<int>? excludeIdList = null)
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

		sbSql.LeftJoin($"{ItemCategory.MsSqlTable} pr ON pr.IsDeleted=0 AND pr.Id=t.ParentId");

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
				$";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
				$"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
		}

		using var cn = ConnectionFactory.GetDbConnection()!;

		var dataList = await cn.QueryAsync<ItemCategory, ItemCategory, ItemCategory>(sql, (obj, parent) =>
		{
            obj.Parent = parent;
			return obj;
		}, param, splitOn: "Id");

		string sqlCount = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int dataCount = await cn.ExecuteScalarAsync<int>(sqlCount, param);
		return new(dataCount, dataList);
	}

	public override List<string> GetSearchOrderbBy()
	{
		return ["pr.ObjectName ASC", "t.ObjectName ASC"];
	}

	public async Task<List<ItemCategory>> SearchAsync(
        int pgSize = 0, int pgNo = 0,
        int? parentId = null,
        string? parentCode = null,
        string? objectCode = null,
        string? objectName = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(nameof(pgSize), _errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted = 0");

        if (parentId.HasValue)
        {
            sbSql.Where("t.ParentId=@ParentId");
            param.Add("@ParentId", parentId);
        }

        if (!string.IsNullOrEmpty(parentCode))
        {
            sbSql.Where("LOWER(t.ParentCode) LIKE '%'+@ParentCode+'%'");
            param.Add("@ParentCode", parentCode, DbType.AnsiString);
        }
        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("LOWER(t.ObjectCode) LIKE '%'+@ObjectCode+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }
        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+@ObjectName+'%'");
            param.Add("@ObjectName", objectName, DbType.AnsiString);
        }

        sbSql.LeftJoin($"{ItemCategory.MsSqlTable} ic on ic.IsDeleted=0 AND ic.ObjectCode=t.ParentCode");

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
                $";WITH pg AS (SELECT Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY ) " +
                $"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ WHERE t.Id IN (SELECT Id FROM pg) /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = ConnectionFactory.GetDbConnection()!;

        List<ItemCategory> result = (await cn.QueryAsync<ItemCategory, ItemCategory, ItemCategory>(
            sql, (ItemCategory, parentCategory) =>
            {
                ItemCategory.Parent = parentCategory;
                return ItemCategory;
            }, param, splitOn: "Id")).ToList();

        return result;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        int? parentId = null,
        string? parentCode = null,
        string? objectCode = null,
        string? objectName = null)
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(nameof(pgSize), _errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        //Implement lower-case convert for text-type base search to lower is to ensure searching is case-insensitive
        //since now PCLAAPP DB is a unicode enabled database, i.e. all search are case sensitive

        sbSql.Where("t.IsDeleted=0");

        if (parentId.HasValue)
        {
            sbSql.Where("t.ParentId=@ParentId");
            param.Add("@ParentId", parentId);
        }

        if (!string.IsNullOrEmpty(parentCode))
        {
            sbSql.Where("LOWER(t.ParentCode) LIKE '%'+LOWER(@ParentCode)+'%'");
            param.Add("@ParentCode", parentCode, DbType.AnsiString);
        }
        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("LOWER(t.ObjectCode) LIKE '%'+LOWER(@ObjectCode)+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }
        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@ObjectName)+'%'");
            param.Add("@ObjectName", objectName, DbType.AnsiString);
        }

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)Math.Ceiling(recordCount / pgSize);

        DataPagination pagination = new()
        {
            ObjectType = typeof(ItemCategory).Name,
            RecordCount = (int)recordCount,
            PageCount = pageCount,
            PageSize = pgSize
        };

        return pagination;
    }
}