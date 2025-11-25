using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Repos.SystemCore;

public interface ICountryRepos : IBaseRepos<Country>
{
	Task<Country?> GetExistingRecordAsync(int currentObjId, string objectCode, string codeAlpha2, string codeAlpha3, string unCode);
	Task<Country?> GetByCodeAlpha2Async(string codeAlpha2);
	Task<Country?> GetByCodeAlpha3Async(string codeAlpha3);
	Task<Country?> GetByUNCodeAsync(string unCode);
	Task<List<Country>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		string? nameEn = null,
		string? nameKh = null,
		string? codeAlpha2 = null,
		string? codeAlpha3 = null,
		string? unCode = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		string? nameEn = null,
		string? nameKh = null,
		string? codeAlpha2 = null,
		string? codeAlpha3 = null,
		string? unCode = null);

	// Functions for UI population
	Task<List<DropDownListItem>> GetForDropdownSelect1Async(string? searchText = null, int? includingObjId = null);
	Task<List<DropdownSelectItem>> GetForNationalitySelectAsync(string? searchText = null, int? includingObjId = null);
}

public class CountryRepos(IConnectionFactory connectionFactory) : BaseRepos<Country>(connectionFactory, Country.DatabaseObject), ICountryRepos
{
	public async Task<Country?> GetExistingRecordAsync(int currentObjId, string objectCode, string codeAlpha2, string codeAlpha3, string unCode)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Where("t.IsDeleted=0")
            .Where("t.Id<>@CurrentObjectId")
            .Where("(t.ObjectCode=@ObjectCode OR CodeAlpha2=@CodeAlpha2 OR CodeAlpha3=@CodeAlpha3 OR UNCode=@UNCode)");

        var sbSqlTempl = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/");
        
        param.Add("@CurrentObjectId", currentObjId);
        param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        param.Add("@CodeAlpha2", codeAlpha2, DbType.AnsiString);
        param.Add("@CodeAlpha3", codeAlpha3, DbType.AnsiString);
        param.Add("@UNCode", unCode, DbType.AnsiString);

        using var cn = ConnectionFactory.GetDbConnection()!;

        var data = await cn.QueryFirstOrDefaultAsync<Country?>(sbSqlTempl.RawSql, param);
        return data;
    }

    public async Task<Country?> GetByCodeAlpha2Async(string codeAlpha2)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Where("t.IsDeleted=0")
            .Where("t.CodeAlpha2=@CodeAlpha2");

        param.Add("@CodeAlpha2", codeAlpha2, DbType.AnsiString);

        using var cn = ConnectionFactory.GetDbConnection()!;
        var sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        return await cn.QuerySingleOrDefaultAsync<Country>(sql, param);
    }

    public async Task<Country?> GetByCodeAlpha3Async(string codeAlpha3)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Where("t.IsDeleted=0")
            .Where("t.CodeAlpha3=@CodeAlpha3");

        param.Add("@CodeAlpha3", codeAlpha3, DbType.AnsiString);

        using var cn = ConnectionFactory.GetDbConnection()!;
        var sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        return await cn.QuerySingleOrDefaultAsync<Country?>(sql, param);
    }

    public async Task<Country?> GetByUNCodeAsync(string unCode)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Where("t.IsDeleted=0")
            .Where("t.UNCode=@UNCode");

        param.Add("@UNCode", unCode, DbType.AnsiString);

        using var cn = ConnectionFactory.GetDbConnection()!;
        var sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        return await cn.QuerySingleOrDefaultAsync<Country>(sql, param);
    }

    public async Task<List<CountryCallingCodeItem>> GetAllCountryCallingCodesAsync()
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Select("t.CodeAlpha3");
        sbSql.Select("t.CodeAlpha2");
        sbSql.Select("t.FullName");
        sbSql.Select("t.CountryCallingCode");

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.CountryCallingCode IS NOT NULL");
        sbSql.Where("LEN(t.CountryCallingCode)>0");

        using var cn = ConnectionFactory.GetDbConnection()!;
        var sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        return (await cn.QueryAsync<CountryCallingCodeItem>(sql)).OrderBy(x => x.FullName).AsList();
    }

    public async Task<List<CountryCallingCodeItem>> GetSupportedCountryCallingCodesAsync()
    {
        
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Select("t.CodeAlpha3");
        sbSql.Select("t.CodeAlpha2");
        sbSql.Select("t.FullName");
        sbSql.Select("t.CountryCallingCode");

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.IsCallCodeSupported=1");

        using var cn = ConnectionFactory.GetDbConnection()!;
        var sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        return (await cn.QueryAsync<CountryCallingCodeItem>(sql)).OrderBy(x => x.FullName).ToList();
    }

	public override async Task<KeyValuePair<int, IEnumerable<Country>>> SearchNewAsync(
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
				$";WITH pg AS (SELECT Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
				$"SELECT * FROM {DbObject.MsSqlTable} t WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
		}

		using var cn = ConnectionFactory.GetDbConnection()!;

		var dataList = await cn.QueryAsync<Country>(sql, param);

		string sqlCount = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int dataCount = await cn.ExecuteScalarAsync<int>(sqlCount, param);
		return new(dataCount, dataList);
	}

    public async Task<List<Country>> SearchAsync(
        int pgSize = 0, int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
        string? nameEn = null,
        string? nameKh = null,
        string? codeAlpha2 = null,
        string? codeAlpha3 = null,
        string? unCode = null)
    {
        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (objectCode.HasValue())
        {
            sbSql.Where("UPPER(t.ObjectCode) LIKE '%'+UPPER(@ObjectCode)+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (objectName.HasValue())
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@ObjectName)+'%'");
            param.Add("@ObjectName", objectName, DbType.AnsiString);
        }

        if (nameEn.HasValue())
        {
            sbSql.Where("t.NameEn LIKE @NameEn+'%'");
            param.Add("@NameEn", nameEn, DbType.AnsiString);
        }

        if (nameKh.HasValue())
        {
            sbSql.Where("t.NameKh LIKE '%'+@NameKh+'%'");
            param.Add("@NameKh", nameKh!);
        }

        if (codeAlpha2.HasValue())
        {
            sbSql.Where("t.CodeAlpha2=@CodeAlpha2");
            param.Add("@CodeAlpha2", codeAlpha2, DbType.AnsiString);
        }

        if (codeAlpha3.HasValue())
        {
            sbSql.Where("t.CodeAlpha3=@CodeAlpha3");
            param.Add("@CodeAlpha3", codeAlpha3, DbType.AnsiString);
        }

        if (unCode.HasValue())
        {
            sbSql.Where("t.UNCode=@UNCode");
            param.Add("@UNCode", unCode, DbType.AnsiString);
        }
        #endregion

        sbSql.OrderBy("t.ObjectName AS").OrderBy("t.NameEn ASC");

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
                    $";WITH pg AS (SELECT Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                    $"SELECT p.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**orderby**/").RawSql;
        }

        using var cn = ConnectionFactory.GetDbConnection()!;

        List<Country> result = (await cn.QueryAsync<Country>(sql, param)).AsList();

        return result;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        string? objectCode = null,
        string? objectName = null,
        string? nameEn = null,
        string? nameKh = null,
        string? codeAlpha2 = null,
        string? codeAlpha3 = null,
        string? unCode = null)
    {
        if (pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (objectCode.HasValue())
        {
            sbSql.Where("UPPER(t.ObjectCode) LIKE '%'+UPPER(@ObjectCode)+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (objectName.HasValue())
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@ObjectName)+'%'");
            param.Add("@ObjectName", objectName, DbType.AnsiString);
        }

        if (nameEn.HasValue())
        {
            sbSql.Where("t.NameEn LIKE @NameEn+'%'");
            param.Add("@NameEn", nameEn, DbType.AnsiString);
        }

        if (nameKh.HasValue())
        {
            sbSql.Where("t.NameKh LIKE '%'+@NameKh+'%'");
            param.Add("@NameKh", nameKh!);
        }

        if (codeAlpha2.HasValue())
        {
            sbSql.Where("t.CodeAlpha2=@CodeAlpha2");
            param.Add("@CodeAlpha2", codeAlpha2, DbType.AnsiString);
        }

        if (codeAlpha3.HasValue())
        {
            sbSql.Where("t.CodeAlpha3=@CodeAlpha3");
            param.Add("@CodeAlpha3", codeAlpha3, DbType.AnsiString);
        }

        if (unCode.HasValue())
        {
            sbSql.Where("t.UNCode=@UNCode");
            param.Add("@UNCode", unCode, DbType.AnsiString);
        }
        #endregion

        var sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;
        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = pgSize == 0 ? 1 : (int)Math.Ceiling(recordCount / pgSize);

        DataPagination pagingResult = new()
        {
            ObjectType = typeof(Country).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagingResult;
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

        using var cn = ConnectionFactory.GetDbConnection()!;
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

        using var cn = ConnectionFactory.GetDbConnection()!;
        string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
        List<DropdownSelectItem> result = (await cn.QueryAsync<DropdownSelectItem>(sql, param)).AsList();

        return result;
    }
    #endregion
}