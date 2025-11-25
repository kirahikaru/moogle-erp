using DataLayer.Models.Retail;

namespace DataLayer.Repos.Retail;

public interface ISupplierBranchRepos : IBaseRepos<SupplierBranch>
{
	Task<List<SupplierBranch>> GetBySupplierAsync(int supplierId);
}

public class SupplierBranchRepos(IConnectionFactory connectionFactory) : BaseRepos<SupplierBranch>(connectionFactory, SupplierBranch.DatabaseObject), ISupplierBranchRepos
{
	public async Task<List<SupplierBranch>> GetBySupplierAsync(int supplierId)
    {
        var sql = $"SELECT * FROM {DbObject.MsSqlTable} WHERE IsDeleted=0 AND SupplierId=@SupplierId";

        using var cn = ConnectionFactory.GetDbConnection()!;

        List<SupplierBranch> dataList = (await cn.QueryAsync<SupplierBranch>(sql, new { SupplierId = supplierId })).ToList();

        return dataList;
    }
}