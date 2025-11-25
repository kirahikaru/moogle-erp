using DataLayer.Models.RMS;

namespace DataLayer.Repos.RMS;

public interface ISupplierBranchRepos : IBaseRepos<SupplierBranch>
{
	Task<List<SupplierBranch>> GetBySupplierAsync(int supplierId);
}

public class SupplierBranchRepos(IDbContext dbContext) : BaseRepos<SupplierBranch>(dbContext, SupplierBranch.DatabaseObject), ISupplierBranchRepos
{
	public async Task<List<SupplierBranch>> GetBySupplierAsync(int supplierId)
    {
        var sql = $"SELECT * FROM {DbObject.MsSqlTable} WHERE IsDeleted=0 AND SupplierId=@SupplierId";

        using var cn = DbContext.DbCxn;

        List<SupplierBranch> dataList = (await cn.QueryAsync<SupplierBranch>(sql, new { SupplierId = supplierId })).ToList();

        return dataList;
    }
}