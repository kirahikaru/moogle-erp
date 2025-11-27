using DataLayer.Models.Pru.PruCORE;
using static Dapper.SqlMapper;

namespace DataLayer.Repos.Pru.PruCORE;

public interface IPruCoreInfraStackRepos : IBaseRepos<PruCoreInfraStack>
{
	Task<KeyValuePair<int, IEnumerable<PruCoreInfraStack>>> SearchAsync(
		int pgSize = 0,
		int pgNo = 0,
		string? searchText = null,
		List<SqlSortCond>? sortConds = null,
		List<SqlFilterCond>? filterConds = null,
		List<int>? excludeIdList = null);

	Task<PruCoreInfraStack?> GetFullAsync(int id);

	Task<int> InsertOrUpdateFullAsync(PruCoreInfraStack obj);
}

public class PruCoreInfraStackRepos(IDbContext dbContext) : BaseRepos<PruCoreInfraStack>(dbContext, PruCoreInfraStack.DatabaseObject), IPruCoreInfraStackRepos
{
	public async Task<KeyValuePair<int, IEnumerable<PruCoreInfraStack>>> SearchAsync(
		int pgSize = 0,
		int pgNo = 0,
		string? searchText = null,
		List<SqlSortCond>? sortConds = null,
		List<SqlFilterCond>? filterConds = null,
		List<int>? excludeIdList = null)
	{
		if (pgNo < 0 && pgSize < 0)
			throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.ProjectCode IS NOT NULL");

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
			else if (searchText.StartsWith("sr:"))
			{
				sbSql.Where("UPPER(t.SerialNo) LIKE '%'+UPPER(@SearchText)+'%'");
				param.Add("@SearchText", searchText.Replace("sr:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
			}
			else
			{
				sbSql.Where("(UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%')");
				param.Add("@SearchText", searchText, DbType.AnsiString);
			}
		}

		if (excludeIdList != null && excludeIdList.Count > 0)
		{
			sbSql.Where("t.Id NOT IN @ExcludeIdList");
			param.Add("@ExcludeIdList", excludeIdList);
		}
		#endregion

		sbSql.LeftJoin($"{PruCoreProject.MsSqlTable} proj ON proj.IsDeleted=0 AND proj.ObjectCode=t.ProjectCode");

		if (sortConds != null && sortConds.Count != 0)
		{
			foreach (SqlSortCond sortCond in sortConds)
			{
				sbSql.OrderBy(sortCond.GetSortCommand("t"));
			}
		}
		else
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
				$";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
				$"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
		}

		using var cn = DbContext.DbCxn;

		var dataList = await cn.QueryAsync<PruCoreInfraStack, PruCoreProject, PruCoreInfraStack>(sql, 
				(infraStack, proj) =>
				{
					infraStack.Project = proj;
					return infraStack;
				}, param, splitOn: "Id");

		string countSql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;
		int count = await cn.ExecuteScalarAsync<int>(countSql, param);

		return new KeyValuePair<int, IEnumerable<PruCoreInfraStack>>(count, dataList);
	}

	public async Task<PruCoreInfraStack?> GetFullAsync(int id)
	{
		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.Id=@Id");
		param.Add("@Id", id);

		sbSql.LeftJoin($"{PruCoreProject.MsSqlTable} proj ON proj.IsDeleted=0 AND proj.ObjectCode=t.ProjectCode");

		using var cn = DbContext.DbCxn;
		string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

		var obj = await cn.QuerySingleOrDefaultAsync<PruCoreInfraStack?>(sql, param);

		return obj;
	}


	public async Task<int> InsertOrUpdateFullAsync(PruCoreInfraStack obj)
	{
		using var cn = DbContext.DbCxn;
		if (cn.State != ConnectionState.Open)
			cn.Open();

		using var tran = cn.BeginTransaction();

		try
		{
			if (obj.Id > 0) // Update
			{
				bool isUpdated = await cn.UpdateAsync(obj, tran);
			}
			else
			{
				int objId = await cn.InsertAsync(obj, tran);
			}
			tran.Commit();
			return obj.Id;
		}
		catch
		{
			tran.Rollback();
			throw;
		}
	}

	public override List<string> GetSearchOrderbBy()
	{
		return ["t.ProjectCode ASC", "t.ObjectName ASC"];
	}
}