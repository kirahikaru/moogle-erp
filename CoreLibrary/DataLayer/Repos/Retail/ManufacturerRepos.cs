using DataLayer.Models.Retail;

namespace DataLayer.Repos.Retail;

public interface IManufacturerRepos : IBaseRepos<Manufacturer>
{

}

public class ManufacturerRepos(IConnectionFactory connectionFactory) : BaseRepos<Manufacturer>(connectionFactory, Manufacturer.DatabaseObject), IManufacturerRepos
{
}