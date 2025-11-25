namespace DataLayer.Repos.SysCore;

public interface IObjectStateHistoryRepos : IBaseRepos<ObjectStateHistory>
{

}

public class ObjectStateHistoryRepos(IDbContext dbContext) : BaseRepos<ObjectStateHistory>(dbContext, ObjectStateHistory.DatabaseObject), IObjectStateHistoryRepos
{
}