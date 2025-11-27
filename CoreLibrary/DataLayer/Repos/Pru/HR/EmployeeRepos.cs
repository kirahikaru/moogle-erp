using DataLayer.GlobalConstant;
using static Dapper.SqlMapper;
using PruHR=DataLayer.Models.Pru.HR;
namespace DataLayer.Repos.Pru.HR;

public interface IEmployeeRepos : IBaseRepos<PruHR.Employee>
{
	Task<IEnumerable<DropdownSelectItem>> GetActiveForDropdownAsync(bool displayWithEmpID = false);
	Task<KeyValuePair<int, IEnumerable<PruHR.Employee>>> SearchAsync(
		int pgSize = 0,
		int pgNo = 0,
		string? searchText = null,
		IEnumerable<SqlSortCond>? sortConds = null,
		IEnumerable<SqlFilterCond>? filterConds = null,
		List<int>? excludeIdList = null);
}

public class EmployeeRepos(IDbContext dbContext) : BaseRepos<PruHR.Employee>(dbContext, PruHR.Employee.DatabaseObject), IEmployeeRepos
{
	public async Task<IEnumerable<DropdownSelectItem>> GetActiveForDropdownAsync(bool displayWithEmpID = false)
	{
		SqlBuilder sbSql = new();
		DynamicParameters param = new();
		sbSql.Select("t.Id");
		sbSql.Select("'Key'=t.EmpID");
		if (displayWithEmpID)
			sbSql.Select("'Value'=t.ObjectName + ' ('+t.EmpID+')'");
		else
			sbSql.Select("'Value'=t.ObjectName");

		sbSql.Select("'ValueKh'=t.LocalName");
		
		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("LEN(ISNULL(t.EmpID,''))>0");
		sbSql.Where("t.EmpStatus=@EmpStatus");
		sbSql.OrderBy("t.ObjectName ASC");

		param.Add("@EmpStatus", EmployeeStatuses.ACTIVE, DbType.AnsiString);

		using var cn = DbContext.DbCxn;
		string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
		var dataList = await cn.QueryAsync<DropdownSelectItem>(sql, param);
		return dataList;
	}

	public async Task<KeyValuePair<int, IEnumerable<PruHR.Employee>>> SearchAsync(
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
			else if (searchText.StartsWith("type:"))
			{
				sbSql.Where("UPPER(t.WorkerType) LIKE '%'+UPPER(@WorkerType)+'%'");
				param.Add("@SearchText", searchText.Replace("type:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
			}
			else
			{
				sbSql.Where("(UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%' OR t.EmpID LIKE '%'+UPPER(@SearchText)+'%')");
				param.Add("@SearchText", searchText, DbType.AnsiString);
			}
		}

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

		var dataList = (await cn.QueryAsync<PruHR.Employee>(sql, param)).AsList();

		string countSql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int count = await cn.ExecuteScalarAsync<int>(countSql, param);

		return new KeyValuePair<int, IEnumerable<PruHR.Employee>>(count, dataList);
	}

	public override List<string> GetSearchOrderbBy()
	{
		return ["t.EmpStatus","t.ObjectName ASC"];
	}
}