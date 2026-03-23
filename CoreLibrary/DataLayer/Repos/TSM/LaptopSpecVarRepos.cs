using DataLayer.Models.TSM;

namespace DataLayer.Repos.TSM;

public interface ILaptopSpecVarRepos : IBaseRepos<LaptopSpecVar>
{
	Task<List<LaptopSpecVar>> GetByLaptopAsync(int laptopId);
}

public class LaptopSpecVarRepos(IDbContext dbContext) : BaseRepos<LaptopSpecVar>(dbContext, LaptopSpecVar.DatabaseObject), ILaptopSpecVarRepos
{
	public async Task<List<LaptopSpecVar>> GetByLaptopAsync(int laptopId)
	{
		DynamicParameters param = new();
		SqlBuilder sbSql = new();
		string sql;
		if (DbContext.DbType == DatabaseTypes.POSTGRESQL)
		{
			sql = $"SELECT * FROM {DbObject.PgTable} WHERE is_deleted=false AND laptop_id=@laptop_id ORDER BY seq_no ASC";
			param.Add("@laptop_id", laptopId);
		}
		else
		{
			sql = $"SELECT * FROM {DbObject.MsSqlTable} WHERE IsDeleted=0 AND LaptopId=@LaptopId ORDER BY SeqNo ASC";
			param.Add("@LaptopId", laptopId);
		}

		using var cn = DbContext.DbCxn;

		var dataList = (await cn.QueryAsync<LaptopSpecVar>(sql, param)).AsList();

		return dataList;
	}

	public override async Task<KeyValuePair<int, IEnumerable<LaptopSpecVar>>> SearchNewAsync(
        int pgSize = 0, int pgNo = 0, string? searchText = null, 
        IEnumerable<SqlSortCond>? sortConds = null, 
        IEnumerable<SqlFilterCond>? filterConds = null, 
        List<int>? excludeIdList = null)
	{
		DynamicParameters param = new();
		SqlBuilder sbSql = new();
		string sqlCount;
		string sql;
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

		var dataList = await cn.QueryAsync<LaptopSpecVar>(sql, param);

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