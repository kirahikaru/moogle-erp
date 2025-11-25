namespace DataLayer.Repos.SystemCore;

public interface IUserRoleRepos : IBaseRepos<UserRole>
{
}

public class UserRoleRepos(IConnectionFactory connectionFactory) : BaseRepos<UserRole>(connectionFactory, UserRole.DatabaseObject), IUserRoleRepos
{
}