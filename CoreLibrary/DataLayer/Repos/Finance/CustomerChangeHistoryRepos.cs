using DataLayer.Models.Finance;

namespace DataLayer.Repos.Finance;

public interface ICustomerChangeHistoryRepos : IBaseRepos<CustomerChangeHistory>
{
	Task<List<CustomerChangeHistory>> GetByCustomerAsync(int customerId);
}

public class CustomerChangeHistoryRepos(IConnectionFactory connectionFactory) : BaseRepos<CustomerChangeHistory>(connectionFactory, CustomerChangeHistory.DatabaseObject), ICustomerChangeHistoryRepos
{
	public async Task<List<CustomerChangeHistory>> GetByCustomerAsync(int customerId)
    {
        string sql = $"SELECT * FROM {DbObject.MsSqlTable} t WHERE t.IsDeleted=0 AND t.CustomerId=@CustomerId ORDER BY t.CreeatedDateTime DESC";

        using var cn = ConnectionFactory.GetDbConnection()!;

        var dataList = (await cn.QueryAsync<CustomerChangeHistory>(sql, new { CustomerId = customerId })).AsList();

        return dataList;
    }
}