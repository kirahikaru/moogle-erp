using DataLayer.Models.Retail;

namespace DataLayer.Repos.Retail;

public interface IRetailTaxItemRepos : IBaseRepos<RetailTaxItem>
{

}

public class RetailTaxItemRepos(IConnectionFactory connectionFactory) : BaseRepos<RetailTaxItem>(connectionFactory, RetailTaxItem.DatabaseObject), IRetailTaxItemRepos
{
}