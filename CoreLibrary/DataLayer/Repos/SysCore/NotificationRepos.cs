namespace DataLayer.Repos.SysCore;

public interface INotificationRepos : IBaseRepos<Notification>
{
}

public class NotificationRepos(IDbContext dbContext) : BaseRepos<Notification>(dbContext, Notification.DatabaseObject), INotificationRepos
{
}