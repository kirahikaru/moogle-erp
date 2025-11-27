using DataLayer.GlobalConstant.Pru;
using DataLayer.Models.Pru.Finance;
using static Dapper.SqlMapper;

namespace DataLayer.Repos.Pru.Finance;

public interface IVendorRepos : IBaseRepos<Vendor>
{
	Task<KeyValuePair<int, IEnumerable<Vendor>>> SearchAsync(
		int pgSize = 0,
		int pgNo = 0,
		string? searchText = null,
		IEnumerable<SqlSortCond>? sortConds = null,
		IEnumerable<SqlFilterCond>? filterConds = null,
		List<int>? excludeIdList = null);

	Task<IEnumerable<DropdownSelectItem>> GetForDropdownListAsync(string lbu, string? includingID = null);

	Task<bool> IsDuplicateCodeAsync(string objCode, string lbu, int id);
}

public class VendorRepos(IDbContext dbContext) : BaseRepos<Vendor>(dbContext, Vendor.DatabaseObject), IVendorRepos
{
	public async Task<bool> IsDuplicateCodeAsync(string objCode, string lbu, int id)
	{
		string sql = $"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t WHERE t.IsDeleted=0 AND t.LBU=@LBU AND t.ObjectCode=@ObjectCode AND t.Id<>@Id";
		DynamicParameters param = new();

		param.Add("@ObjectCode", objCode, DbType.AnsiString);
		param.Add("@LBU", lbu, DbType.AnsiString);
		param.Add("@Id", id);

		using var cn = DbContext.DbCxn;

		int count = await cn.ExecuteScalarAsync<int>(sql, param);
		return count > 0;
	}

	public async Task<KeyValuePair<int, IEnumerable<Vendor>>> SearchAsync(
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
			else if (searchText.StartsWith("sr:"))
			{
				sbSql.Where("UPPER(t.SerialNo) LIKE '%'+UPPER(@SearchText)+'%'");
				param.Add("@SearchText", searchText.Replace("sr:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
			}
			else
			{
				sbSql.Where("(UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.ContractName) LIKE '%'+UPPER(@SearchText)+'%')");
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

		var dataList = (await cn.QueryAsync<Vendor>(sql, param)).AsList();

		string countSql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int count = await cn.ExecuteScalarAsync<int>(countSql, param);

		return new KeyValuePair<int, IEnumerable<Vendor>>(count, dataList);
	}

	public async Task<IEnumerable<DropdownSelectItem>> GetForDropdownListAsync(string lbu, string? includingID = null)
	{
		SqlBuilder sbSql = new();

		using var cn = DbContext.DbCxn;

		DynamicParameters param = new();

		sbSql.Select("t.Id");
		sbSql.Select("'Key'=t.ObjectCode");
		sbSql.Select("'Value'=t.ObjectName");

		if (!string.IsNullOrEmpty(includingID))
		{
			sbSql.Where("t.IsDeleted=0");
			sbSql.Where("t.LBU=@LBU");
			sbSql.Where("t.[Status]=@VendorStatus");
		}
		else
		{
			sbSql.Where("(t.IsDeleted=0 AND t.LBU=@LBU AND t.[Status]=@VendorStatus) OR t.ObjectCode=@ObjectCode");
			param.Add("@ObjectCode", includingID, DbType.AnsiString);
		}

		param.Add("@LBU", lbu, DbType.AnsiString);
		param.Add("@VendorStatus", VendorStatuses.ACTIVE, DbType.AnsiString);

		sbSql.OrderBy("t.ObjectName ASC");

		string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;

		return await cn.QueryAsync<DropdownSelectItem>(sql, param);
	}
}