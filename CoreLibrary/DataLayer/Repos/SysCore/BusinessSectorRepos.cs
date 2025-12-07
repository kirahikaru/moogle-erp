namespace DataLayer.Repos.SysCore;

public interface IBusinessSectorRepos : IBaseRepos<BusinessSector>
{
	Task<List<DropDownListItem>> GetValidParentsAsync(int objectId, string objectCode, int? includingId = null);
	Task<List<BusinessSector>> GetByParentAsync(int id);
	Task<List<BusinessSector>> GetAllChildrenAsync(string objectCode);
}

public class BusinessSectorRepos(IDbContext dbContext) : BaseRepos<BusinessSector>(dbContext, BusinessSector.DatabaseObject), IBusinessSectorRepos
{
	public async Task<List<BusinessSector>> GetByParentAsync(int id)
    {
        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.ParentId=@ParentId");
        sbSql.LeftJoin($"{DbObject.MsSqlTable} p ON p.IsDeleted=0 AND p.Id=t.ParentId");

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        List<BusinessSector> dataList = (await cn.QueryAsync<BusinessSector, BusinessSector, BusinessSector>(sql,
                                                (obj, p) =>
                                                {
                                                    obj.Parent = p;
                                                    return obj;
                                                }, new { ParentId = id }, splitOn: "Id")).ToList();

        return dataList;
    }

	public override async Task<KeyValuePair<int, IEnumerable<BusinessSector>>> SearchNewAsync(
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

		sbSql.LeftJoin($"{BusinessSector.MsSqlTable} pr ON pr.Id=0 AND pr.Id=t.ParentId");

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

		using var cn = DbContext.DbCxn;

		var dataList = await cn.QueryAsync<BusinessSector, BusinessSector, BusinessSector>(sql,
			(obj, pr) => {
				obj.Parent = pr;
				return obj;
			}, param, splitOn: "Id");

		string sqlCount = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int dataCount = await cn.ExecuteScalarAsync<int>(sqlCount, param);
		return new(dataCount, dataList);
	}

	public async Task<List<BusinessSector>> GetAllChildrenAsync(string objectCode)
    {
        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.HierarchyPath=@ObjectCode+'%'");
        sbSql.LeftJoin($"{DbObject.MsSqlTable} p ON p.IsDeleted=0 AND p.Id=t.ParentId");

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        param.Add("@ObjectCode", objectCode, DbType.AnsiString);

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<BusinessSector, BusinessSector, BusinessSector>(sql,
                                                (obj, p) =>
                                                {
                                                    obj.Parent = p;
                                                    return obj;
                                                }, param, splitOn: "Id")).ToList();

        return dataList;
    }

    public async Task<List<DropDownListItem>> GetValidParentsAsync(int objectId, string objectCode, int? includingId = null)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();
        param.Add("@Id", objectId);
        param.Add("@ObjectCode", objectCode, DbType.AnsiString);

        sbSql.Select("'ObjectId'=t.Id");
        sbSql.Select("t.ObjectCode");
        sbSql.Select("t.ObjectName");
        sbSql.Select("'ObjectNameEn'=t.ObjectName");
        sbSql.Select("'ObjectNameKh'=t.ObjectNameKh");
        sbSql.Select("t.HierarchyPath");

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

        using var cn = DbContext.DbCxn;
        string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
        var dataList = (await cn.QueryAsync<DropDownListItem>(sql, param)).AsList();

        return dataList;
    }
}