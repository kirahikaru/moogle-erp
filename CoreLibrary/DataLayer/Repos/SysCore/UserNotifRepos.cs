namespace DataLayer.Repos.SysCore;

public interface IUserNotifRepos : IBaseRepos<UserNotification>
{
}

public class UserNotifRepos(IDbContext dbContext) : BaseRepos<UserNotification>(dbContext, UserNotification.DatabaseObject), IUserNotifRepos
{
}