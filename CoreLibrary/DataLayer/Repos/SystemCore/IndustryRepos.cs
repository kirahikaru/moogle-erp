using DataLayer.Models.SystemCore.NonPersistent;
using System.Text.RegularExpressions;

namespace DataLayer.Repos.SystemCore;

public interface IIndustryRepos : IBaseRepos<Industry>
{
	Task<List<BasicObjectSelectListItem>> GetValidParentAsync(int objectId, string objectCode, int? includingId = null);
	Task<List<Industry>> GetByParentAsync(int id);
	Task<List<Industry>> GetAllChildrenAsync(string objectCode);
}

public class IndustryRepos(IConnectionFactory connectionFactory) : BaseRepos<Industry>(connectionFactory, Industry.DatabaseObject), IIndustryRepos
{
	public override async Task<KeyValuePair<int, IEnumerable<Industry>>> SearchNewAsync(
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
			if (Regex.IsMatch(searchText, @"$\d{5,}^"))
			{
				sbSql.Where("t.Barcode=@SearchText");
				param.Add("@SearchText", searchText, DbType.AnsiString);
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
				$"SELECT * FROM {DbObject.MsSqlTable} t WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
		}

		using var cn = ConnectionFactory.GetDbConnection()!;

		var dataList = await cn.QueryAsync<Industry>(sql, param);

		string sqlCount = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int dataCount = await cn.ExecuteScalarAsync<int>(sqlCount, param);
		return new(dataCount, dataList);
	}

	public async Task<List<Industry>> GetByParentAsync(int id)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.ParentId=@ParentId");

        sbSql.LeftJoin($"{DbObject.MsSqlTable} p ON p.IsDeleted=0 AND p.Id=t.ParentId");


        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        List<Industry> dataList = (await cn.QueryAsync<Industry, Industry, Industry>(sql,
                                                (obj, p) =>
                                                {
                                                    obj.Parent = p;
                                                    return obj;
                                                }, new { ParentId = id }, splitOn: "Id")).ToList();

        return dataList;
    }

    public async Task<List<BasicObjectSelectListItem>> GetValidParentAsync(
        int objectId, 
        string objectCode, 
        int? includingId = null)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Select("'ObjectId'=t.Id")
            .Select("t.ObjectCode")
            .Select("t.ObjectName")
            .Select("'ObjectNameEn'=t.ObjectName")
            .Select("'ObjectNameKh'=t.ObjectNameKh")
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

        using var cn = ConnectionFactory.GetDbConnection()!;

        List<BasicObjectSelectListItem> result = (await cn.QueryAsync<BasicObjectSelectListItem>(sql, param)).AsList();

        return result;
    }

    public async Task<List<Industry>> GetAllChildrenAsync(string objectCode)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.HierarchyPath LIKE @ObjectCode+'%'");
        sbSql.LeftJoin($"{DbObject.MsSqlTable} p ON p.IsDeleted=0 AND p.Id=t.ParentId");
        param.Add("@ObjectCode", objectCode, DbType.AnsiString);

        using var cn = ConnectionFactory.GetDbConnection()!;
        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;
        List<Industry> dataList = (await cn.QueryAsync<Industry, Industry, Industry>(sql,
                                                (obj, p) =>
                                                {
                                                    obj.Parent = p;
                                                    return obj;
                                                }, param, splitOn: "Id")).ToList();

        return dataList;
    }
}