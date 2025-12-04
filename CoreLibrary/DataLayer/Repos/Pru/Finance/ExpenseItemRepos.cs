using DataLayer.Models.Pru.Finance;
using static Dapper.SqlMapper;

namespace DataLayer.Repos.Pru.Finance;

public interface IExpenseItemRepos : IBaseRepos<ExpenseItem>
{
	Task<IEnumerable<ExpenseItem>> GetByYearAsync(int year);

	Task<List<ExpenseItem>> GetByAssetAsync(string assetID);

	//Task<KeyValuePair<int, IEnumerable<ExpenseItem>>> SearchAsync(
	//	int pgSize = 0,
	//	int pgNo = 0,
	//	string? searchText = null,
	//	IEnumerable<SqlSortCond>? sortConds = null,
	//	IEnumerable<SqlFilterCond>? filterConds = null,
	//	List<int>? excludeIdList = null);
}

public class ExpenseItemRepos(IDbContext dbContext) : BaseRepos<ExpenseItem>(dbContext, ExpenseItem.DatabaseObject), IExpenseItemRepos
{
	public async Task<List<ExpenseItem>> GetByAssetAsync(string assetID)
	{
		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.ObjectCode=@AssetID");
		param.Add("@AssetID", assetID, DbType.AnsiString);

		using var cn = DbContext.DbCxn;
		string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		var dataList = (await cn.QueryAsync<ExpenseItem>(sql, param)).AsList();
		return dataList;
	}

	public async Task<IEnumerable<ExpenseItem>> GetByYearAsync(int year)
	{
		SqlBuilder sbSql = new();
		DynamicParameters param = new();
		param.Add("@ExpenseYr", year);
		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.ExpenseYr=@ExpenseYr");
		sbSql.OrderBy("t.ObjectName");

		using var cn = DbContext.DbCxn;
		string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		var dataList = await cn.QueryAsync<ExpenseItem>(sql, param);
		return dataList;
	}

	public override async Task<KeyValuePair<int, IEnumerable<ExpenseItem>>> SearchNewAsync(
		int pgSize = 0, int pgNo = 0,
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
			else if (searchText.StartsWith("tracker:"))
			{
				sbSql.Where("UPPER(t.ActivityTrackID) LIKE '%'+UPPER(@SearchText)+'%'");
				param.Add("@SearchText", searchText.Replace("tracker:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
			}
			else
			{
				sbSql.Where("UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.PurchaseOrderNo) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.InvoiceCode) LIKE '%'+UPPER(@SearchText)+'%'");
				param.Add("@SearchText", searchText, DbType.AnsiString);
			}
		}

		if (excludeIdList != null && excludeIdList.Count > 0)
		{
			sbSql.Where("t.Id NOT IN @ExcludeIdList");
			param.Add("@ExcludeIdList", excludeIdList);
		}
		#endregion

		sbSql.LeftJoin($"{BudgetItem.MsSqlTable} bi ON bi.IsDeleted=0 AND bi.IsCurrent=1 AND bi.AccountCode=t.AccountCode AND bi.ActivityTrackID=t.ActivityTrackID");
		sbSql.LeftJoin($"{Vendor.MsSqlTable} v ON v.IsDeleted=0 AND v.LBU=t.LBU AND v.ObjectCode=t.VendorID");

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

		using var cn = DbContext.DbCxn;

		var dataList = (await cn.QueryAsync<ExpenseItem, BudgetItem, Vendor, ExpenseItem>(sql, (ei, bi, v) =>
		{
			ei.BudgetItem = bi;
			ei.Vendor = v;
			return ei;
		}, param: param, splitOn: "Id")).AsList();

		string countSql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int count = await cn.ExecuteScalarAsync<int>(countSql, param);

		return new KeyValuePair<int, IEnumerable<ExpenseItem>>(count, dataList);
	}

	public override List<string> GetSearchOrderbBy()
	{
		return ["t.ExpenseYr DESC", "t.ExpenseMth DESC", "t.InvoiceDate DESC"];
	}
}