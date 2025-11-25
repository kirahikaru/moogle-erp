using DataLayer.Models.Hospital;

namespace DataLayer.Repos.Hospital;

public interface IIllnessRepos : IBaseRepos<Illness>
{

}

public class IllnessRepos(IConnectionFactory connectionFactory) : BaseRepos<Illness>(connectionFactory, Illness.DatabaseObject), IIllnessRepos
{
}