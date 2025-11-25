namespace DataLayer.Repos.SysCore;

public interface IUserLocationHistoryRepos : IBaseRepos<UserLocationHistory>
{

}

public class UserLocatinoHistoryRepos(IDbContext dbContext) : BaseRepos<UserLocationHistory>(dbContext, UserLocationHistory.DatabaseObject), IUserLocationHistoryRepos
{
}