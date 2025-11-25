namespace DataLayer.Repos.FIN;

public interface ITaxRateRepos : IBaseRepos<TaxRate>
{
	Task<List<TaxRate>> GetByTaxAsync(int taxId);
}

public class TaxRateRepos(IDbContext dbContext) : BaseRepos<TaxRate>(dbContext, TaxRate.DatabaseObject), ITaxRateRepos
{
	public async Task<List<TaxRate>> GetByTaxAsync(int taxId)
    {
        string sql = $"SELECT * FROM {DbObject.MsSqlTable} t WHERE t.IsDeleted=0 AND t.TaxId=@TaxId";

        using var cn = DbContext.DbCxn;

        List<TaxRate> dataList = (await cn.QueryAsync<TaxRate>(sql, new { TaxId = taxId })).AsList();

        return dataList;
    }
}