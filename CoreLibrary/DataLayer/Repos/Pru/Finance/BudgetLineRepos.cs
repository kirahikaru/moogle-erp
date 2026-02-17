using DataLayer.Models.Pru.Finance;
using System.Text.RegularExpressions;
using static Dapper.SqlMapper;

namespace DataLayer.Repos.Pru.Finance;

public interface IBudgetLineRepos : IBaseRepos<BudgetLine>
{	
	Task<BudgetLine?> GetFullAsync(int id);

	Task<List<DropDownListItem>> GetValidParentsAsync(int? groupingLevel);
}

public class BudgetLineRepos(IDbContext dbContext) : BaseRepos<BudgetLine>(dbContext, BudgetLine.DatabaseObject), IBudgetLineRepos
{
	public async Task<BudgetLine?> GetFullAsync(int id)
	{
		SqlBuilder sbSql = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.Id=@Id");

		using var cn = DbContext.DbCxn;
		string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
		var obj = await cn.QuerySingleOrDefaultAsync<BudgetLine?>(sql, new { Id = id });

		return obj;
	}

	public async Task<List<DropDownListItem>> GetValidParentsAsync(int? groupingLevel)
	{
		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Select("t.Id")
			.Select("t.ObjectCode")
			.Select("t.ObjectName")
			.Select("'ObjectNameEn'=t.ObjectName + ' (' + t.DisplayOrder + ')'")
			.Select("t.HierarchyPath");

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.IsEnabled=1");

		if (groupingLevel is null)
		{
			sbSql.Where("t.GroupingLevel IS NOT NULL");
		}
		else
		{
			sbSql.Where("t.GroupingLevel IS NOT NULL");
			sbSql.Where("t.GroupingLevel < @GroupingLevel");
			param.Add("@GroupingLevel", groupingLevel.Value);
		}

		sbSql.OrderBy("t.DisplayOrder ASC");

		using var cn = DbContext.DbCxn;
		string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
		var dataList = (await cn.QueryAsync<DropDownListItem>(sql, param)).AsList();
		return dataList;
	}

	public override async Task<KeyValuePair<int, IEnumerable<BudgetLine>>> SearchNewAsync(
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
			else if (searchText.StartsWith("tracker:"))
			{
				sbSql.Where("UPPER(t.ActivityTrackID) LIKE '%'+UPPER(@SearchText)+'%'");
				param.Add("@SearchText", searchText.Replace("tracker:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
			}
			else
			{
				sbSql.Where("UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%'");
				param.Add("@SearchText", searchText, DbType.AnsiString);
			}
		}

		if (filterConds != null && filterConds.Any())
		{
			foreach (SqlFilterCond filterCond in filterConds)
			{
				sbSql.Where(filterCond.GetFilterSqlCommand("t"));

				if (filterCond.FilterValue != null)
					param.Add($"@{filterCond.FieldName}", filterCond.FilterValue);
				else if (filterCond.Parameters.ParameterNames.Count() > 0)
					param.AddDynamicParams(filterCond.Parameters);

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

			sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) ROWS FETCH NEXT @PageSize ROWS ONLY").RawSql;
		}

		using var cn = DbContext.DbCxn;

		var dataList = (await cn.QueryAsync<BudgetLine>(sql, param)).AsList();

		string countSql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int count = await cn.ExecuteScalarAsync<int>(countSql, param);

		return new KeyValuePair<int, IEnumerable<BudgetLine>>(count, dataList);
	}

	public override List<string> GetSearchOrderbBy()
	{
		return ["t.DisplayOrder ASC"];
	}
}