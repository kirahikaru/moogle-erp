using DataLayer.Models.RMS;

namespace DataLayer.Repos.RMS;

public interface IRetailOtherChargeRepos : IBaseRepos<RetailOtherCharge>
{

}

public class RetailOtherChargeRepos(IDbContext dbContext) : BaseRepos<RetailOtherCharge>(dbContext, RetailOtherCharge.DatabaseObject), IRetailOtherChargeRepos
{
}