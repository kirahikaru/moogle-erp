using DataLayer.Models.SysCore.ManyToManyLink;

namespace DataLayer.Repos.SysCore;

public interface IRolePermissionRepos : IBaseRepos<RolePermission>
{
}

public class RolePermissionRepos(IDbContext dbContext) : BaseRepos<RolePermission>(dbContext, RolePermission.DatabaseObject), IRolePermissionRepos
{
}