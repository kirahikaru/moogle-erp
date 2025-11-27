using DataLayer.Models.Pru.Finance;
using System.Text.RegularExpressions;
using static Dapper.SqlMapper;

namespace DataLayer.Repos.Pru.Finance;

public interface IBudgetItemRepos : IBaseRepos<BudgetItem>
{
	Task<bool> HasExistingAsync(string lbu, int budgetYr, string budgetID, int id);

	Task<BudgetItem?> GetFullAsync(int id);

	Task<IEnumerable<BudgetItem>> GetByYearAsync(int year);

	Task<KeyValuePair<int, IEnumerable<BudgetItem>>> SearchAsync(
		int pgSize = 0,
		int pgNo = 0,
		string? searchText = null,
		IEnumerable<SqlSortCond>? sortConds = null,
		IEnumerable<SqlFilterCond>? filterConds = null,
		List<int>? excludeIdList = null);
}

public class BudgetItemRepos(IDbContext dbContext) : BaseRepos<BudgetItem>(dbContext, BudgetItem.DatabaseObject), IBudgetItemRepos
{

	public async Task<BudgetItem?> GetFullAsync(int id)
	{
		SqlBuilder sbSql = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.Id=@Id");

		using var cn = DbContext.DbCxn;
		string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
		var obj = await cn.QuerySingleOrDefaultAsync<BudgetItem?>(sql, new { Id = id });

		if (obj is not null)
		{
			SqlBuilder sbSqlExp = new();
			sbSqlExp.Where("ei.IsDeleted=0");
			sbSqlExp.Where("ei.ObjectCode=@BudgetItemID");
			sbSqlExp.OrderBy("ei.EffectiveDate DESC");
			sbSqlExp.OrderBy("ei.InvoiceDate DESC");

			sbSqlExp.LeftJoin($"{Vendor.MsSqlTable} v ON v.IsDeleted=0 AND v.LBU=ei.LBU AND v.ObjectCode=ei.VendorID");

			string sqlExp = sbSqlExp.AddTemplate($"SELECT * FROM {ExpenseItem.MsSqlTable} ei /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
			obj.ExpenseItems = (await cn.QueryAsync<ExpenseItem, Vendor, ExpenseItem>(sqlExp, (expItem, vendor) =>
			{
				expItem.Vendor = vendor;
				return expItem;
			}, new { BudgetItemID = obj.ObjectCode }, splitOn: "Id")).AsList();
		}

		return obj;
	}

	public async Task<bool> HasExistingAsync(string lbu, int budgetYr, string budgetID, int id)
	{
		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.IsCurrent=1");
		sbSql.Where("t.LBU=@LBU");
		sbSql.Where("t.BudgetYear=@BudgetYear");
		sbSql.Where("t.ObjectCode=@ObjectCode");
		sbSql.Where("t.Id<>@Id");

		param.Add("@LBU", lbu, DbType.AnsiString);
		param.Add("@BudgetYear", budgetYr);
		param.Add("@ObjectCode", budgetID);
		param.Add("@Id", id);

		using var cn = DbContext.DbCxn;
		string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int count = await cn.ExecuteScalarAsync<int>(sql, param);
		return count > 0;
	}

	public async Task<IEnumerable<BudgetItem>> GetByYearAsync(int year)
	{
		SqlBuilder sbSql = new();
		DynamicParameters param = new();
		param.Add("@BudgetYear", year);
		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.BudgetYear=@BudgetYear");
		sbSql.OrderBy("t.GroupingL1");
		sbSql.OrderBy("t.GroupingL2");
		sbSql.OrderBy("t.GroupingL3");
		sbSql.OrderBy("t.ObjectName");

		using var cn = DbContext.DbCxn;
		string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		var dataList = await cn.QueryAsync<BudgetItem>(sql, param);
		return dataList;
	}

	public async Task<KeyValuePair<int, IEnumerable<BudgetItem>>> SearchAsync(
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
		sbSql.Where("t.IsCurrent=1");

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
			else if (searchText.StartsWith("tracker:"))
			{
				sbSql.Where("UPPER(t.ActivityTrackID) LIKE '%'+UPPER(@SearchText)+'%'");
				param.Add("@SearchText", searchText.Replace("tracker:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
			}
			else if (Regex.IsMatch(searchText, @"^[0-9]{4}[A-Z]{0,1}$"))
			{
				sbSql.Where("t.ActivityTrackID=@SearchText");
				param.Add("@SearchText", searchText, DbType.AnsiString);
			}
			else
			{
				sbSql.Where("UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%'");
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

		var dataList = (await cn.QueryAsync<BudgetItem>(sql, param)).AsList();

		string countSql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int count = await cn.ExecuteScalarAsync<int>(countSql, param);

		return new KeyValuePair<int, IEnumerable<BudgetItem>>(count, dataList);
	}

	public override List<string> GetSearchOrderbBy()
	{
		return ["t.IsCurrent DESC", "t.BudgetYear DESC", "t.LBU", "t.VersionName DESC", "t.GroupingL1 ASC", "t.GroupingL2 ASC", "t.GroupingL3 ASC", "t.ObjectName ASC"];
	}
}