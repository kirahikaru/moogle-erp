using DataLayer.Models.Pru.PruCORE;
using static Dapper.SqlMapper;

namespace DataLayer.Repos.Pru.PruCORE;

public interface IPruCoreProjectRepos : IBaseRepos<PruCoreProject>
{
	Task<KeyValuePair<int, IEnumerable<PruCoreProject>>> SearchAsync(
		int pgSize = 0,
		int pgNo = 0,
		string? searchText = null,
		List<SqlSortCond>? sortConds = null,
		List<SqlFilterCond>? filterConds = null,
		List<int>? excludeIdList = null);

	Task<PruCoreProject?> GetFullAsync(int id);

	Task<int> InsertOrUpdateFullAsync(PruCoreProject obj);
}

public class PruCoreProjectRepos(IDbContext dbContext) : BaseRepos<PruCoreProject>(dbContext, PruCoreProject.DatabaseObject), IPruCoreProjectRepos
{
	public async Task<KeyValuePair<int, IEnumerable<PruCoreProject>>> SearchAsync(
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

		if (filterConds != null && filterConds.Count > 0)
		{
			foreach (SqlFilterCond cond in filterConds)
			{
				sbSql.Where(cond.GetSqlQuery("t"));

				if (cond.Parameters.ParameterNames.Count() > 0)
					param.AddDynamicParams(cond.Parameters);
			}
		}

		if (excludeIdList != null && excludeIdList.Count > 0)
		{
			sbSql.Where("t.Id NOT IN @ExcludeIdList");
			param.Add("@ExcludeIdList", excludeIdList);
		}
		#endregion

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
				$";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
				$"SELECT * FROM {DbObject.MsSqlTable} t WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
		}

		using var cn = DbContext.DbCxn;

		var dataList = await cn.QueryAsync<PruCoreProject>(sql, param);

		string countSql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

		int count = await cn.ExecuteScalarAsync<int>(countSql, param);

		return new KeyValuePair<int, IEnumerable<PruCoreProject>>(count, dataList);
	}

	public async Task<PruCoreProject?> GetFullAsync(int id)
	{
		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.Id=@Id");
		param.Add("@Id", id);

		//sbSql.LeftJoin($"{Vendor.TableName} v ON v.IsDeleted=0 AND v.ObjectCode=t.VendorID");

		using var cn = DbContext.DbCxn;
		string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		var obj = await cn.QuerySingleOrDefaultAsync<PruCoreProject?>(sql, param);

		if (obj != null)
		{
			SqlBuilder sbSqlItem = new();
			DynamicParameters paramItem = new();
			sbSqlItem.Where("is.IsDeleted=0");
			sbSqlItem.Where("is.ProjectCode=@ProjectCode");
			paramItem.Add("@ProjectCode", obj.ObjectCode, DbType.AnsiString);

			string sqlItem = sbSqlItem.AddTemplate($"SELECT * FROM {PruCoreInfraStack.MsSqlTable} is /**where**/").RawSql;
			obj.InfraStacks = (await cn.QueryAsync<PruCoreInfraStack>(sqlItem, paramItem)).AsList();
		}

		return obj;
	}


	public async Task<int> InsertOrUpdateFullAsync(PruCoreProject obj)
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
		return ["t.ObjectName ASC"];
	}
}