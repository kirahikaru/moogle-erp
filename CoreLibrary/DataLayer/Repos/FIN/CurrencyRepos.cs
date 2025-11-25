
using static Dapper.SqlMapper;

namespace DataLayer.Repos.FIN;

public interface ICurrencyRepos : IBaseRepos<Currency>
{
	Task<Currency?> GetFullAsync(int id);

	Task<List<DropdownSelectItem>> GetForDropdownSelect1Async(string? objectName = null, int? includingId = null);

	Task<List<Currency>> SearchAsync(int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		List<string>? countryCodeList = null);

	Task<DataPagination> GetSearchPaginationAsync(int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		List<string>? countryCodeList = null);

	/// <summary>
	/// 
	/// </summary>
	/// <param name="searchText"></param>
	/// <returns></returns>
	Task<List<DropdownSelectItem>> GetForDropdown1Async(string? searchText = null);

	/// <summary>
	/// Display Format: ObjectName (ObjectCode)
	/// </summary>
	/// <param name="searchText"></param>
	/// <returns></returns>
	Task<List<DropdownSelectItem>> GetForDropdown2Async(string? searchText = null);

	/// <summary>
	/// Display Format: ObjectCode (Symbol)
	/// </summary>
	/// <param name="searchText"></param>
	/// <returns></returns>
	Task<List<DropdownSelectItem>> GetForDropdown3Async(string? searchText = null);
}

public class CurrencyRepos(IDbContext dbContext) : BaseRepos<Currency>(dbContext, Currency.DatabaseObject), ICurrencyRepos
{
	public async Task<Currency?> GetFullAsync(int id)
    {
        string sql = $"SELECT * FROM {DbObject.MsSqlTable} t LEFT JOIN {Country.MsSqlTable} c ON c.IsDeleted=0 AND c.ObjectCode=t.CountryCode WHERE t.IsDeleted=0 AND t.Id=@Id";

        using var cn = DbContext.DbCxn;

        var param = new { Id = id };

        var data = (await cn.QueryAsync<Currency, Country, Currency>(sql, (currency, country) =>
        {
            currency.Country = country;
            return currency;
        }, param: param, splitOn: "Id")).FirstOrDefault();

        return data;
    }

    public override async Task<List<Currency>> QuickSearchAsync(int pgSize = 0, int pgNo = 0, 
        string? searchText = null, 
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
            else
            {
                sbSql.Where("UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%'");
                param.Add("@SearchText", searchText, DbType.AnsiString);
            }
        }

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }
        #endregion

        sbSql.LeftJoin($"{Country.MsSqlTable} c ON c.IsDeleted=0 AND c.ObjectCode=t.CountryCode");
        sbSql.OrderBy("t.ObjectName ASC");

        string sql;

        if (pgNo == 0 && pgSize == 0)
        {
            sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
        }
        else
        {
            param.Add("@PageSize", pgSize);
            param.Add("@PageNo", pgNo);

            sql = sbSql.AddTemplate(
                $";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                $"SELECT t.*, c.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<Currency, Country, Currency>(sql, (obj, cty) => {
                            obj.Country = cty;
                            return obj;
                        }, param, splitOn:"Id")).AsList();

        return dataList;
    }

    public async Task<List<Currency>> SearchAsync(int pgSize = 0, int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
        List<string>? countryCodeList = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(nameof(pgSize), _errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("LOWER(t.ObjectCode) LIKE '%'+@ObjectCode+'%'");
            param.Add("@ObjectCode", objectCode.ToLower(CultureInfo.CurrentUICulture), DbType.AnsiString);
        }
        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+@ObjectName+'%'");
            param.Add("@ObjectName", objectName.ToLower(CultureInfo.CurrentUICulture), DbType.AnsiString);
        }

        if (countryCodeList is not null && countryCodeList.Count > 0)
        {
            sbSql.Where("t.CountryCode IN @CountryCodeList");
            param.Add("@CountryCodeList", countryCodeList);
        }
        #endregion

        sbSql.LeftJoin($"{Country.MsSqlTable} c ON c.IsDeleted=0 AND c.ObjectCode=t.CountryCode");

        sbSql.OrderBy("t.ObjectName ASC");

        string sql;

        if (pgNo == 0 && pgSize == 0)
        {
            sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
        }
        else
        {
            param.Add("@PageSize", pgSize);
            param.Add("@PageNo", pgNo);
            sql = sbSql.AddTemplate(
                $";WITH pg AS (SELECT Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY ) " +
                $"SELECT t.*, c.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<Currency, Country, Currency>(sql, (currency, country) =>
        {
            currency.Country = country;
            return currency;
        }, param: param, splitOn: "Id")).AsList();

        return dataList;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(int pgSize = 0,
        string? objectCode = null,
        string? objectName = null,
        List<string>? countryCodeList = null)
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(nameof(pgSize), _errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        //Implement lower-case convert for text-type base search to lower is to ensure searching is case-insensitive
        //since now PCLAAPP DB is a unicode enabled database, i.e. all search are case sensitive

        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("LOWER(t.ObjectCode) LIKE '%'+@ObjectCode+'%'");
            param.Add("@ObjectCode", objectCode.ToLower(CultureInfo.CurrentUICulture), DbType.AnsiString);
        }
        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+@ObjectName+'%'");
            param.Add("@ObjectName", objectName.ToLower(CultureInfo.CurrentUICulture), DbType.AnsiString);
        }

        if (countryCodeList is not null && countryCodeList.Count > 0)
        {
            sbSql.Where("t.CountryCode IN @CountryCodeList");
            param.Add("@CountryCodeList", countryCodeList);
        }
        #endregion

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)(Math.Ceiling(recordCount / pgSize));

        DataPagination pagination = new()
        {
            ObjectType = typeof(Currency).Name,
            RecordCount = (int)recordCount,
            PageCount = pageCount,
            PageSize = pgSize
        };

        return pagination;
    }

    public override async Task<List<Currency>> SearchAsync(
            int pgSize = 0,
            int pgNo = 0,
            string? objectCode = null,
            string? objectName = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(nameof(pgSize), _errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");

		#region Form Search Conditions
		if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("LOWER(t.ObjectCode) LIKE '%'+@ObjectCode+'%'");
            param.Add("@ObjectCode", objectCode.ToLower(CultureInfo.CurrentUICulture), DbType.AnsiString);
        }
        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+@ObjectName+'%'");
            param.Add("@ObjectName", objectName.ToLower(CultureInfo.CurrentUICulture), DbType.AnsiString);
        }
        #endregion

        sbSql.LeftJoin($"{Country.MsSqlTable} c ON c.IsDeleted=0 AND c.ObjectCode=t.CountryCode");

		sbSql.OrderBy("t.ObjectName ASC");

        string sql;

        if (pgNo == 0 && pgSize == 0)
        {
            sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
        }
        else
        {
            param.Add("@PageSize", pgSize);
            param.Add("@PageNo", pgNo);
            sql = sbSql.AddTemplate(
                $";WITH pg AS (SELECT Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY ) " +
                $"SELECT t.*,c.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        List<Currency> result = (await cn.QueryAsync<Currency, Country, Currency>(sql, (currency, country) =>
                                                            {
                                                                currency.Country = country;
                                                                return currency;
                                                            }, param: param, splitOn: "Id")).AsList();

        return result;
    }

    public override async Task<DataPagination> GetSearchPaginationAsync(
            int pgSize = 0,
            string? objectCode = null,
            string? objectName = null
        )
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(nameof(pgSize), _errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        //Implement lower-case convert for text-type base search to lower is to ensure searching is case-insensitive
        //since now PCLAAPP DB is a unicode enabled database, i.e. all search are case sensitive

        sbSql.Where("t.IsDeleted=0");

		#region Form Search Conditions
		if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("LOWER(t.ObjectCode) LIKE '%'+@ObjectCode+'%'");
            param.Add("@ObjectCode", objectCode.ToLower(CultureInfo.CurrentUICulture), DbType.AnsiString);
        }
        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+@ObjectName+'%'");
            param.Add("@ObjectName", objectName.ToLower(CultureInfo.CurrentUICulture), DbType.AnsiString);
        }
		#endregion

		string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)(Math.Ceiling(recordCount / pgSize));

        DataPagination pagination = new()
        {
            ObjectType = typeof(Currency).Name,
            RecordCount = (int)recordCount,
            PageCount = pageCount,
            PageSize = pgSize
        };

        return pagination;
    }

    public async Task<List<DropdownSelectItem>> GetForDropdownSelect1Async(string? objectName = null, int? includingId = null)
    {
        SqlBuilder sbSql = new();

        using var cn = DbContext.DbCxn;

        List<DropdownSelectItem> result = new();
        DynamicParameters param = new();

        sbSql.Select("t.Id")
            .Select("'Key'=t.ObjectCode")
            .Select("'Value'=t.ObjectCode+' ('+t.CurrencySymbol+')'");

        if (includingId.HasValue)
        {
            param.Add("@IncludingId", includingId.Value);

            if (!string.IsNullOrEmpty(objectName))
            {
                sbSql.Where("(t.IsDeleted=0 AND LOWER(t.ObjectName) LIKE '%'+LOWER(@ObjectName)+'%') OR t.Id=@IncludingId");
                param.Add("@ObjectName", objectName, DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.IsDeleted=0 OR t.Id=@IncludingId");
            }
        }
        else if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("t.IsDeleted=0");
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@ObjectName)+'%'");
            param.Add("@ObjectName", objectName, DbType.AnsiString);
        }
        else
        {
            sbSql.Where("t.IsDeleted=0");
        }

        sbSql.OrderBy("t.ObjectName ASC");

        string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
        result = (await cn.QueryAsync<DropdownSelectItem>(sql, param)).AsList();

        return result;
    }

    public async Task<List<DropdownSelectItem>> GetForDropdown1Async(string? searchText = null)
    {
        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Select("t.Id")
            .Select("'Key'=t.ObjectCode")
            .Select("'Value'=t.ObjectName+' ('+t.ObjectCode+')'");

        sbSql.Where("t.IsDeleted=0");
        
        if (!string.IsNullOrEmpty(searchText))
        {
            sbSql.Where("(LOWER(t.ObjectName) LIKE '%'+LOWER(@SearchText)+'%' OR LOWER(t.ObjectCode) LIKE '%'+LOWER(@SearchText)+'%')");
            param.Add("@SearchText", searchText, DbType.AnsiString);
        }

        using var cn = DbContext.DbCxn;
        string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        var result = (await cn.QueryAsync<DropdownSelectItem>(sql, param)).AsList();

        return result;
    }

    public async Task<List<DropdownSelectItem>> GetForDropdown2Async(string? searchText = null)
    {
        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Select("t.Id")
            .Select("'Key'=t.ObjectCode")
            .Select("'Value'=t.ObjectName+' ('+t.ObjectCode+')'");

        sbSql.Where("t.IsDeleted=0");

        if (!string.IsNullOrEmpty(searchText))
        {
            sbSql.Where("LOWER(t.ObjectCode) LIKE '%'+LOWER(@SearchText)+'%'");
            param.Add("@SearchText", searchText, DbType.AnsiString);
        }

        using var cn = DbContext.DbCxn;
        string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        var result = (await cn.QueryAsync<DropdownSelectItem>(sql, param)).AsList();

        return result;
    }

    public async Task<List<DropdownSelectItem>> GetForDropdown3Async(string? searchText = null)
    {
        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Select("t.Id")
            .Select("'Key'=t.ObjectCode")
            .Select("'Value'=t.ObjectCode+' ('+t.CurrencySymbol+')'");

        sbSql.Where("t.IsDeleted=0");

        if (!string.IsNullOrEmpty(searchText))
        {
            sbSql.Where("UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%'");
            param.Add("@SearchText", searchText, DbType.AnsiString);
        }

        using var cn = DbContext.DbCxn;
        string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        var result = (await cn.QueryAsync<DropdownSelectItem>(sql, param)).AsList();

        return result;
    }
}