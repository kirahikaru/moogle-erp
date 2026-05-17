namespace DataLayer.Repos.SysCore;

public interface IObjStateHistoryRepos : IBaseRepos<ObjStateHistory>
{

}

public class ObjStateHistoryRepos(IDbContext dbContext) : BaseRepos<ObjStateHistory>(dbContext, ObjStateHistory.DatabaseObject), IObjStateHistoryRepos
{
}