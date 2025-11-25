namespace DataLayer.Repos.SystemCore;

public interface IObjectStatusAuditTrailRepos : IBaseRepos<ObjectStatusAuditTrail>
{

}

public class ObjectStatusAuditTrailRepos(IConnectionFactory connectionFactory) : BaseRepos<ObjectStatusAuditTrail>(connectionFactory, ObjectStatusAuditTrail.DatabaseObject), IObjectStatusAuditTrailRepos
{
}