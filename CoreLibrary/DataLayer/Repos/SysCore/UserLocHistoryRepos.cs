namespace DataLayer.Repos.SysCore;

/// <summary>
/// Repository : User Location History
/// </summary>
public interface IUserLocHistoryRepos : IBaseRepos<UserLocationHistory>
{

}


/// <summary>
/// 
/// </summary>
/// <param name="dbContext"></param>
public class UserLocHistoryRepos(IDbContext dbContext) : BaseRepos<UserLocationHistory>(dbContext, UserLocationHistory.DatabaseObject), IUserLocHistoryRepos
{
}