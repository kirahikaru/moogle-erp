namespace DataLayer.Repos.FIN;

public interface ICustChgHistoryRepos : IBaseRepos<CustomerChangeHistory>
{
	Task<List<CustomerChangeHistory>> GetByCustomerAsync(int customerId);
}

public class CustChgHistoryRepos(IDbContext dbContext) : BaseRepos<CustomerChangeHistory>(dbContext, CustomerChangeHistory.DatabaseObject), ICustChgHistoryRepos
{
	public async Task<List<CustomerChangeHistory>> GetByCustomerAsync(int customerId)
    {
        string sql = $"SELECT * FROM {DbObject.MsSqlTable} t WHERE t.IsDeleted=0 AND t.CustomerId=@CustomerId ORDER BY t.CreeatedDateTime DESC";

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<CustomerChangeHistory>(sql, new { CustomerId = customerId })).AsList();

        return dataList;
    }
}