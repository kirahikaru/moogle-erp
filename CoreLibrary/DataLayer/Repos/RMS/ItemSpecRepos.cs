using DataLayer.Models.RMS;
using DataLayer.Models.SysCore.NonPersistent;

namespace DataLayer.Repos.SysCore;

public interface IItemSpecRepos : IBaseRepos<ItemSpec>
{
	Task<List<ItemSpec>> GetByItemAsync(int itemId);
}

public class ItemSpecRepos(IDbContext dbContext) : BaseRepos<ItemSpec>(dbContext, ItemSpec.DatabaseObject), IItemSpecRepos
{
	public async Task<List<ItemSpec>> GetByItemAsync(int itemId)
	{
		DynamicParameters param = new();
		SqlBuilder sbSql = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.ItemId=@ItemId");
		param.Add("@ItemId", itemId, DbType.Int32);

		sbSql.OrderBy("t.OrderNo ASC");
		sbSql.OrderBy("t.SpecType ASC");

		string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
		using var cn = DbContext.DbCxn;
		
		var dataList = (await cn.QueryAsync<ItemSpec>(sql, param)).AsList();
		return dataList;
	}

	public override async Task<KeyValuePair<int, IEnumerable<ItemSpec>>> SearchNewAsync(
        int pgSize = 0, int pgNo = 0, string? searchText = null, 
        IEnumerable<SqlSortCond>? sortConds = null, 
        IEnumerable<SqlFilterCond>? filterConds = null, 
        List<int>? excludeIdList = null)
	{
		DynamicParameters param = new();
		SqlBuilder sbSql = new();

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

        sbSql.LeftJoin($"{Item.MsSqlTable} i ON i.Id=t.ItemId");

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

		var dataList = await cn.QueryAsync<ItemSpec>(sql, param);

		string sqlCount = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int dataCount = await cn.ExecuteScalarAsync<int>(sqlCount, param);
		return new(dataCount, dataList);
	}
}