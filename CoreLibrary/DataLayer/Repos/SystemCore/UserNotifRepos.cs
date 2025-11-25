namespace DataLayer.Repos.SystemCore;

public interface IUserNotifRepos : IBaseRepos<UserNotification>
{
}

public class UserNotifRepos(IConnectionFactory connectionFactory) : BaseRepos<UserNotification>(connectionFactory, UserNotification.DatabaseObject), IUserNotifRepos
{
}