namespace DataLayer.Repos.SysCore;

public interface ICredentialRepos : IBaseRepos<Credential>
{
	Task<Credential?> GetByUserId(int userId);
	Task<Credential?> GetByUsername(string username);
}

public class CredentialRepos(IDbContext dbContext) : BaseRepos<Credential>(dbContext, Credential.DatabaseObject), ICredentialRepos
{
	public async Task<Credential?> GetByUserId(int userId)
    {
        if (userId <= 0)
            throw new ArgumentOutOfRangeException(nameof(userId), "ObjectId cannot be negative integer.");

        var sql = $"SELECT * FROM {Credential.MsSqlTable} WHERE IsDeleted = 0 AND UserId=@UserId";
        var parameters = new { @UserId = userId };

        using var cn = DbContext.DbCxn;

        return await cn.QuerySingleOrDefaultAsync<Credential>(sql, parameters);
    }

    public async Task<Credential?> GetByUsername(string username)
    {
        if (string.IsNullOrEmpty(username))
            return null;

        var sql = $"SELECT * FROM {Credential.MsSqlTable} WHERE IsDeleted = 0 AND Username=@Username";
        var parameters = new { @Username = username };

        using var cn = DbContext.DbCxn;

        return await cn.QuerySingleOrDefaultAsync<Credential>(sql, parameters);
    }
}