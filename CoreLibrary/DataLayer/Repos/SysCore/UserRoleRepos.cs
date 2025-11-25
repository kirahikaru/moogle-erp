namespace DataLayer.Repos.SysCore;

public interface IUserRoleRepos : IBaseRepos<UserRole>
{
}

public class UserRoleRepos(IDbContext dbContext) : BaseRepos<UserRole>(dbContext, UserRole.DatabaseObject), IUserRoleRepos
{
}