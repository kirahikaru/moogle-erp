namespace DataLayer.Repos.SystemCore;

public interface INotificationRepos : IBaseRepos<Notification>
{
}

public class NotificationRepos(IConnectionFactory connectionFactory) : BaseRepos<Notification>(connectionFactory, Notification.DatabaseObject), INotificationRepos
{
}