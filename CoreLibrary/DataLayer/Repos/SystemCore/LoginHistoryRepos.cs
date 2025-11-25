namespace DataLayer.Repos.SystemCore;

public interface ILoginHistoryRepos : IBaseRepos<LoginHistory>
{
	Task<List<LoginHistory>> GetByUserAsync(string userId);
}

public class LoginHistoryRepos(IConnectionFactory connectionFactory) : BaseRepos<LoginHistory>(connectionFactory, LoginHistory.DatabaseObject), ILoginHistoryRepos
{
	public async Task<List<LoginHistory>> GetByUserAsync(string userId)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Where("IsDeleted=0");
        sbSql.Where("Username=@Username");

        param.Add("@Username", userId, DbType.AnsiString);

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        return (await cn.QueryAsync<LoginHistory>(sql, param)).ToList();
    }
}