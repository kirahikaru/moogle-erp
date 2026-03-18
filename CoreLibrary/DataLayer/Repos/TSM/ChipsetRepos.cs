using DataLayer.Models.TSM;

namespace DataLayer.Repos.TSM;

public interface IChipsetRepos : IBaseRepos<Chipset>
{
	
}

public class ChipsetRepos(IDbContext dbContext) : BaseRepos<Chipset>(dbContext, Chipset.DatabaseObject), IChipsetRepos
{
	public override async Task<KeyValuePair<int, IEnumerable<Chipset>>> SearchNewAsync(
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
			sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
		}
		else
		{
			param.Add("@PageSize", pgSize);
			param.Add("@PageNo", pgNo);
			sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) ROWS FETCH NEXT @PageSize ROWS ONLY").RawSql;
		}

		using var cn = DbContext.DbCxn;

		var dataList = await cn.QueryAsync<Chipset>(sql, param);

		string sqlCount = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int dataCount = await cn.ExecuteScalarAsync<int>(sqlCount, param);
		return new(dataCount, dataList);
	}

    #region NonPersistent
    public async Task<List<DropDownListItem>> GetForDropdownSelect1Async(string? searchText = null, int? includingObjId = null)
    {
        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql
            .Select("t.Id")
            .Select("t.ObjectCode")
            .Select("t.ObjectName")
            .Select("'ObjectNameEn'=t.NameEn")
            .Select("'ObjectNameKh'=t.NameKh");
		sbSql.Where("t.IsDeleted=0");
        sbSql.OrderBy("t.NameEn ASC");

        if (!string.IsNullOrEmpty(searchText))
        {
            param.Add("@SearchText", searchText!, DbType.AnsiString);

            if (includingObjId is not null)
            {
                param.Add("@IncludingObjectId", includingObjId!.Value);
                sbSql.Where("(LOWER(t.ObjectName) LIKE '%'+LOWER(@SearchText)+'%' OR t.Id=@IncludingObjectId)");
            }
            else
            {
                sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@SearchText)+'%'");
            }
        }

        using var cn = DbContext.DbCxn;
        string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
        var dataList = (await cn.QueryAsync<DropDownListItem>(sql, param)).AsList();
        return dataList;
    }
    
    public async Task<List<DropdownSelectItem>> GetForNationalitySelectAsync(string? searchText = null, int? includingObjId = null)
    {
        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql
            .Select("t.Id")
            .Select("'Key'=t.ObjectCode")
            .Select("'Value'=t.Nationality");

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("LEN(TRIM(ISNULL(t.Nationality,'')))>0");

        if (!string.IsNullOrEmpty(searchText))
        {
            param.Add("@SearchText", searchText, DbType.AnsiString);

            if (includingObjId is not null)
            {
                sbSql.Where($"UPPER(t.Nationality) LIKE '%'+UPPER(@SearchText)+'%'");
            }
            else
            {
                param.Add("@IncludingObjectId", includingObjId!.Value);
                sbSql.Where($"(UPPER(t.Nationality) LIKE '%'+UPPER(@SearchText)+'%' OR t.Id=@IncludingObjectId)");
            }
        }

        sbSql.OrderBy($"t.Nationality ASC");

        using var cn = DbContext.DbCxn;
        string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
        List<DropdownSelectItem> result = (await cn.QueryAsync<DropdownSelectItem>(sql, param)).AsList();

        return result;
    }
    #endregion
}