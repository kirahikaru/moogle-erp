using DataLayer.Models.Pru.PruCORE;
using static Dapper.SqlMapper;

namespace DataLayer.Repos.Pru.PruCORE;

public interface ICmdbToAppMappingRepos : IBaseRepos<CmdbToAppMapping>
{
	Task<KeyValuePair<int, IEnumerable<CmdbToAppMapping>>> SearchAsync(
		int pgSize = 0,
		int pgNo = 0,
		string? searchText = null,
		List<SqlSortCond>? sortConds = null,
		List<SqlFilterCond>? filterConds = null,
		List<int>? excludeIdList = null);

	Task<bool> HasExistingMappingAsync(string projectCode, string infraStackID, string meterCategory, int id);

	Task<CmdbToAppMapping?> GetFullAsync(int id);

	Task<int> InsertOrUpdateFullAsync(CmdbToAppMapping obj);
}

public class CmdbToAppMappingRepos(IDbContext dbContext) : BaseRepos<CmdbToAppMapping>(dbContext, CmdbToAppMapping.DatabaseObject), ICmdbToAppMappingRepos
{
	public async Task<bool> HasExistingMappingAsync(string projectCode, string infraStackID, string meterCategory, int id)
	{
		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.ProjectCode=@ProjectCode");
		sbSql.Where("t.InfraStackID=@InfraStackID");
		sbSql.Where("t.MeterCategory=@MeterCategory");
		sbSql.Where("t.Id<>@Id");

		param.Add("@ProjectCode", projectCode, DbType.AnsiString);
		param.Add("@InfraStackID", infraStackID, DbType.AnsiString);
		param.Add("@MeterCategory", meterCategory, DbType.AnsiString);
		param.Add("@Id", id, DbType.AnsiString);

		using var cn = DbContext.DbCxn;

		int count = await cn.ExecuteScalarAsync<int>(
			sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql,
			param);

		return count > 0;
	}

	public async Task<KeyValuePair<int, IEnumerable<CmdbToAppMapping>>> SearchAsync(
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
			else if (searchText.StartsWith("appref:"))
			{
				sbSql.Where("UPPER(t.InfraStackID) LIKE '%'+UPPER(@SearchText)+'%'");
				param.Add("@SearchText", searchText.Replace("appref:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
			}
			else
			{
				sbSql.Where("(UPPER(t.InfraStackID) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.MeterCategory) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.ProjectName) LIKE '%'+@SearchText+'%' OR UPPER(t.ProjectCode) LIKE '%'+@SearchText+'%')");
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

		var dataList = await cn.QueryAsync<CmdbToAppMapping>(sql, param);

		string countSql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

		int count = await cn.ExecuteScalarAsync<int>(countSql, param);

		return new KeyValuePair<int, IEnumerable<CmdbToAppMapping>>(count, dataList);
	}

	public async Task<CmdbToAppMapping?> GetFullAsync(int id)
	{
		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.Id=@Id");
		param.Add("@Id", id);

		//sbSql.LeftJoin($"{Vendor.TableName} v ON v.IsDeleted=0 AND v.ObjectCode=t.VendorID");

		using var cn = DbContext.DbCxn;
		string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		var obj = await cn.QuerySingleOrDefaultAsync<CmdbToAppMapping?>(sql, param);

		return obj;
	}


	public async Task<int> InsertOrUpdateFullAsync(CmdbToAppMapping obj)
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
		return ["t.ProjectName ASC", "t.InfraStackID ASC", "t.MeterCategory ASC"];
	}
}