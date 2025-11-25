using DataLayer.Models.RMS;

namespace DataLayer.Repos.RMS;

public interface IManufacturerRepos : IBaseRepos<Manufacturer>
{

}

public class ManufacturerRepos(IDbContext dbContext) : BaseRepos<Manufacturer>(dbContext, Manufacturer.DatabaseObject), IManufacturerRepos
{
}