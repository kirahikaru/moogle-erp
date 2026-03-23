using DataLayer.Models.TSM;

namespace DataLayer.Repos.TSM;

public interface ISSDRepos : IBaseRepos<SSD>
{
	
}

public class SSDRepos(IDbContext dbContext) : BaseRepos<SSD>(dbContext, SSD.DatabaseObject), ISSDRepos
{
	public override async Task<KeyValuePair<int, IEnumerable<SSD>>> SearchNewAsync(
		int pgSize = 0, int pgNo = 0, string? searchText = null,
		IEnumerable<SqlSortCond>? sortConds = null,
		IEnumerable<SqlFilterCond>? filterConds = null,
		List<int>? excludeIdList = null)
	{
		DynamicParameters param = new();
		SqlBuilder sbSql = new();
		string sql;
		string sqlCount;

		if (DbContext.DbType == DatabaseTypes.POSTGRESQL)
		{
			sbSql.Where("t.is_deleted=false");
			if (!string.IsNullOrEmpty(searchText))
			{
				if (searchText.StartsWith("id:"))
				{
					sbSql.Where("UPPER(t.object_code) LIKE '%'+@SearchText+'%'");
					param.Add("@search_txt", searchText.Replace("id:", "", StringComparison.CurrentCultureIgnoreCase), DbType.AnsiString);
				}
				else
				{
					sbSql.Where("(UPPER(t.object_name) LIKE '%'+UPPER(@search_txt)+'%' OR UPPER(t.object_code) LIKE '%'+UPPER(@search_txt)+'%')");
					param.Add("@search_txt", searchText, DbType.AnsiString);
				}
			}

			if (excludeIdList != null && excludeIdList.Count != 0)
			{
				sbSql.Where("t.id NOT IN @excl_id_list");
				param.Add("@excl_id_list", excludeIdList);
			}

			if (pgNo == 0 && pgSize == 0)
			{
				sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.PgTable} t /**where**/ /**orderby**/").RawSql;
			}
			else
			{
				param.Add("@pg_size", pgSize);
				param.Add("@pg_no", pgNo);
				sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.PgTable} t /**where**/ /**orderby**/ LIMIT @pg_size OFFSET @pg_size * (@pg_no - 1) ").RawSql;
			}

			sqlCount = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.PgTable} t /**where**/").RawSql;
		}
		else
		{
			sbSql.Where("t.IsDeleted=0");

			#region Form Search Conditions
			if (!string.IsNullOrEmpty(searchText))
			{
				if (searchText.StartsWith("id:"))
				{
					sbSql.Where("UPPER(t.ObjectCode) LIKE '%'+@SearchText+'%'");
					param.Add("@SearchText", searchText.Replace("id:", "", StringComparison.CurrentCultureIgnoreCase), DbType.AnsiString);
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

			if (sortConds != null && sortConds.Any())
			{
				foreach (var sortCond in sortConds)
					sbSql.OrderBy(sortCond.GetSortCommand("t"));
			}
			else
			{
				foreach (string orderBy in GetSearchOrderbBy())
					sbSql.OrderBy(orderBy);
			}

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

			sqlCount = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		}

		using var cn = DbContext.DbCxn;

		var dataList = await cn.QueryAsync<SSD>(sql, param);

		int dataCount = await cn.ExecuteScalarAsync<int>(sqlCount, param);
		return new(dataCount, dataList);
	}
}