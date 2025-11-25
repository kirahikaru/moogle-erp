using DataLayer.Models.Retail;

namespace DataLayer.Repos.Retail;

public interface IDeliveryOptionRepos : IBaseRepos<DeliveryOption>
{

}

public class DeliveryOptionRepos(IConnectionFactory connectionFactory) : BaseRepos<DeliveryOption>(connectionFactory, DeliveryOption.DatabaseObject), IDeliveryOptionRepos
{
}