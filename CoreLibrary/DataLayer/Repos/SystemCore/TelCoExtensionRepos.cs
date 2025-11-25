namespace DataLayer.Repos.SystemCore;

public interface ITelCoExtensionRepos : IBaseRepos<TelCoExtension>
{
	Task<List<TelCoExtension>> GetByCountryAsync(string countryCode);
}

public class TelCoExtensionRepos(IConnectionFactory connectionFactory) : BaseRepos<TelCoExtension>(connectionFactory, TelCoExtension.DatabaseObject), ITelCoExtensionRepos
{
	public async Task<List<TelCoExtension>> GetByCountryAsync(string countryCode)
    {
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.CountryCode=@CountryCode");
        sbSql.OrderBy("t.TelCoExtNum ASC");

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
        DynamicParameters param = new();
        param.Add("@CountryCode", countryCode, DbType.AnsiString);
        using var cn = ConnectionFactory.GetDbConnection()!;
        List<TelCoExtension> result = (await cn.QueryAsync<TelCoExtension>(sql, new { CountryCode = new DbString { Value = countryCode, IsAnsi = true } })).AsList();

        return result;
    }
}