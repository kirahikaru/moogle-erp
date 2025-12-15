using DataLayer.Models.HMS;

namespace DataLayer.Repos.HMS;

public interface IMedRxItemRepos : IBaseRepos<MedRxItem>
{

}

public class MedRxItemRepos(IDbContext dbContext) : BaseRepos<MedRxItem>(dbContext, MedRxItem.DatabaseObject), IMedRxItemRepos
{
	public override async Task<KeyValuePair<int, IEnumerable<MedRxItem>>> SearchNewAsync(
		int pgSize = 0, int pgNo = 0,
		string? searchText = null,
		IEnumerable<SqlSortCond>? sortConds = null,
		IEnumerable<SqlFilterCond>? filterConds = null,
		List<int>? excludeIdList = null
	)
	{
		DynamicParameters param = new();
		SqlBuilder sbSql = new();

		sbSql.Where("t.IsDeleted=0");

		#region Form Search Conditions
		if (!string.IsNullOrEmpty(searchText))
		{
			if (searchText.StartsWith("id:"))
			{
				sbSql.Where("t.ObjectCode=@SearchText");
				param.Add("@SearchText", searchText.Replace("id:", "", StringComparison.CurrentCultureIgnoreCase), DbType.AnsiString);
			}
			else
			{
				sbSql.Where("(UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%')");
				param.Add("@SearchText", searchText);
			}
		}

		if (excludeIdList != null && excludeIdList.Count != 0)
		{
			sbSql.Where("t.Id NOT IN @ExcludeIdList");
			param.Add("@ExcludeIdList", excludeIdList);
		}

		if (filterConds != null && filterConds.Any())
		{
			foreach (SqlFilterCond filterCond in filterConds)
			{
				sbSql.Where(filterCond.GetFilterSqlCommand("t"));
			}
		}

		#endregion

		sbSql.LeftJoin($"{MedRx.MsSqlTable} rx ON rx.Id=t.MedRxId");

		if (sortConds is null || !sortConds.Any())
		{
			foreach (string order in GetSearchOrderbBy())
				sbSql.OrderBy(order);
		}
		else
		{
			foreach (SqlSortCond sortCond in sortConds)
				sbSql.OrderBy(sortCond.GetSortCommand("t"));
		}

		string sql;

		if (pgNo == 0 && pgSize == 0)
		{
			sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
		}
		else
		{
			param.Add("@PageSize", pgSize);
			param.Add("@PageNo", pgNo);
			sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) ROWS FETCH NEXT @PageSize ROWS ONLY;").RawSql;
		}

		using var cn = DbContext.DbCxn;

		var dataList = await cn.QueryAsync<MedRxItem, MedRx, MedRxItem>(sql, 
			(obj, medRx) => {
				obj.MedRx = medRx; 
				return obj;
			}, param, splitOn: "Id");

		string sqlCount = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int dataCount = await cn.ExecuteScalarAsync<int>(sqlCount, param);
		return new(dataCount, dataList);
	}
}