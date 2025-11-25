using DataLayer.Models.Retail;

namespace DataLayer.Repos.Retail;

public interface IRetailOtherChargeRepos : IBaseRepos<RetailOtherCharge>
{

}

public class RetailOtherChargeRepos(IConnectionFactory connectionFactory) : BaseRepos<RetailOtherCharge>(connectionFactory, RetailOtherCharge.DatabaseObject), IRetailOtherChargeRepos
{
}