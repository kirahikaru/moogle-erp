namespace DataLayer.Repos.SystemCore;

public interface IUserLocationHistoryRepos : IBaseRepos<UserLocationHistory>
{

}

public class UserLocatinoHistoryRepos(IConnectionFactory connectionFactory) : BaseRepos<UserLocationHistory>(connectionFactory, UserLocationHistory.DatabaseObject), IUserLocationHistoryRepos
{
}