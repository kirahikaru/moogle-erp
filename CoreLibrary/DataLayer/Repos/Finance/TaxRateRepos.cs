using DataLayer.Models.Finance;
namespace DataLayer.Repos.Finance;

public interface ITaxRateRepos : IBaseRepos<TaxRate>
{
	Task<List<TaxRate>> GetByTaxAsync(int taxId);
}

public class TaxRateRepos(IConnectionFactory connectionFactory) : BaseRepos<TaxRate>(connectionFactory, TaxRate.DatabaseObject), ITaxRateRepos
{
	public async Task<List<TaxRate>> GetByTaxAsync(int taxId)
    {
        string sql = $"SELECT * FROM {DbObject.MsSqlTable} t WHERE t.IsDeleted=0 AND t.TaxId=@TaxId";

        using var cn = ConnectionFactory.GetDbConnection()!;

        List<TaxRate> dataList = (await cn.QueryAsync<TaxRate>(sql, new { TaxId = taxId })).AsList();

        return dataList;
    }
}