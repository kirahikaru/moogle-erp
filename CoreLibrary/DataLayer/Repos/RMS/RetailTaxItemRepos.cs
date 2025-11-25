using DataLayer.Models.RMS;

namespace DataLayer.Repos.RMS;

public interface IRetailTaxItemRepos : IBaseRepos<RetailTaxItem>
{

}

public class RetailTaxItemRepos(IDbContext dbContext) : BaseRepos<RetailTaxItem>(dbContext, RetailTaxItem.DatabaseObject), IRetailTaxItemRepos
{
}