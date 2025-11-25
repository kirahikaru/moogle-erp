using DataLayer.Models.HMS;
namespace DataLayer.Repos.HMS;

public interface IIllnessRepos : IBaseRepos<Illness>
{

}

public class IllnessRepos(IDbContext dbContext) : BaseRepos<Illness>(dbContext, Illness.DatabaseObject), IIllnessRepos
{
}