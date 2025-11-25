namespace DataLayer.Repos.SysCore;

public interface IObjectStatusAuditTrailRepos : IBaseRepos<ObjectStatusAuditTrail>
{

}

public class ObjectStatusAuditTrailRepos(IDbContext dbContext) : BaseRepos<ObjectStatusAuditTrail>(dbContext, ObjectStatusAuditTrail.DatabaseObject), IObjectStatusAuditTrailRepos
{
}