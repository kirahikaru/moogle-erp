namespace DataLayer.Repos.SysCore;

public interface IObjStatusAuditTrailRepos : IBaseRepos<ObjStatusAuditTrail>
{

}

public class ObjStatusAuditTrailRepos(IDbContext dbContext) : BaseRepos<ObjStatusAuditTrail>(dbContext, ObjStatusAuditTrail.DatabaseObject), IObjStatusAuditTrailRepos
{
}