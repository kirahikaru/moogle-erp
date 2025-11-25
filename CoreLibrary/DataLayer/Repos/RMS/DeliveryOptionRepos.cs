using DataLayer.Models.RMS;

namespace DataLayer.Repos.RMS;

public interface IDeliveryOptionRepos : IBaseRepos<DeliveryOption>
{

}

public class DeliveryOptionRepos(IDbContext dbContext) : BaseRepos<DeliveryOption>(dbContext, DeliveryOption.DatabaseObject), IDeliveryOptionRepos
{
}