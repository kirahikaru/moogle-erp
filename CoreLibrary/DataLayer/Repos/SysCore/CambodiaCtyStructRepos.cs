namespace DataLayer.Repos.SysCore;

public interface ICambodiaCtyStructRepos : IBaseRepos<CambodiaCountryStructure>
{
	Task<CambodiaCountryStructure?> GetByCode2Async(string code2);
	Task<CambodiaCountryStructure?> GetByCode3Async(string code3);
	Task<List<CambodiaCountryStructure>> GetByLevelAsync(int level);
	Task<List<CambodiaCountryStructure>> GetByTypeAsync(string typeCode);
	Task<List<CambodiaCountryStructure>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		string? nameKh = null,
		string? nameEn = null,
		string? code2 = null,
		string? code3 = null,
		int? level = null,
		string? typeCode = null);
}

public class CambodiaCtyStructRepos(IDbContext dbContext) : BaseRepos<CambodiaCountryStructure>(dbContext, CambodiaCountryStructure.DatabaseObject), ICambodiaCtyStructRepos
{
	public async Task<CambodiaCountryStructure?> GetByCode2Async(string code2)
    {
        var sql = $"SELECT * FROM {CambodiaCountryStructure.MsSqlTable} WHERE IsDeleted=0 AND Code2=@Code2";
        var param = new { Code2 = code2 };

        using var cn = DbContext.DbCxn;

        return await cn.QuerySingleOrDefaultAsync<CambodiaCountryStructure>(sql, param);
    }

    public async Task<CambodiaCountryStructure?> GetByCode3Async(string code3)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.Code3=@Code3");

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        param.Add("@Code3", code3, DbType.AnsiString);

        using var cn = DbContext.DbCxn;

        return await cn.QuerySingleOrDefaultAsync<CambodiaCountryStructure>(sql, param);
    }

    public async Task<List<CambodiaCountryStructure>> GetByLevelAsync(int level)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.[Level]=@Level");

        param.Add("@Level", level);

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        return (await cn.QueryAsync<CambodiaCountryStructure>(sql, param)).AsList();
    }

    public async Task<List<CambodiaCountryStructure>> GetByTypeAsync(string typeCode)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.TypeCode=@TypeCode");

        param.Add("@TypeCode", typeCode, DbType.AnsiString);

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        return (await cn.QueryAsync<CambodiaCountryStructure>(sql, param)).AsList();
    }

    public async Task<List<CambodiaCountryStructure>> SearchAsync(
        int pgSize = 0, int pgNo = 0,
        string? nameKh = null,
        string? nameEn = null,
        string? code2 = null,
        string? code3 = null,
        int? level = null,
        string? typeCode = null)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Where("t.IsDeleted=0");

        if (!string.IsNullOrEmpty(nameKh))
        {
            sbSql.Where("t.NameKh LIKE @NameKh+'%'");
            param.Add("@NameKh", nameKh);
        }

        if (!string.IsNullOrEmpty(nameEn))
        {
            sbSql.Where("LOWER(t.NameEn) LIKE '%'+LOWER(@NameEn)+'%'");
            param.Add("@NameEn", nameEn, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(code2))
        {
            sbSql.Where("UPPER(t.Code2)=UPPER(@Code2)");
            param.Add("@Code2", code2, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(code3))
        {
            sbSql.Where("UPPER(t.Code3)=UPPER(@Code3)");
            param.Add("@Code3", code3, DbType.AnsiString);
        }

        if (level >= 0)
        {
            sbSql.Where("t.[Level]=@Level");
            param.Add("@Level", level);
        }

        if (!string.IsNullOrEmpty(typeCode))
        {
            sbSql.Where("t.TypeCode=@TypeCode");
            param.Add("@TypeCode", typeCode, DbType.AnsiString);
        }

        sbSql.OrderBy("t.Id ASC");

        //SqlBuilder.Template sbSqlTempl;
        string sql;

        if (pgNo == 0 && pgSize == 0)
        {
            sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        }
        else
        {
            param.Add("@PageSize", pgSize);
            param.Add("@PageNo", pgNo);
            sql = sbSql.AddTemplate($";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                                    $"SELECT * FROM {DbObject.MsSqlTable} t WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<CambodiaCountryStructure>(sql, param)).AsList();

        return dataList;
    }
}