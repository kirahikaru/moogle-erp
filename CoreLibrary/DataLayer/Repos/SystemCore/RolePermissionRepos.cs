using DataLayer.Models.SystemCore.ManyToManyLink;

namespace DataLayer.Repos.SystemCore;

public interface IRolePermissionRepos : IBaseRepos<RolePermission>
{
}

public class RolePermissionRepos(IConnectionFactory connectionFactory) : BaseRepos<RolePermission>(connectionFactory, RolePermission.DatabaseObject), IRolePermissionRepos
{
}