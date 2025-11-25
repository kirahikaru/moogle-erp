namespace DataLayer.Repos.SystemCore;

public interface IObjectStateHistoryRepos : IBaseRepos<ObjectStateHistory>
{

}

public class ObjectStateHistoryRepos(IConnectionFactory connectionFactory) : BaseRepos<ObjectStateHistory>(connectionFactory, ObjectStateHistory.DatabaseObject), IObjectStateHistoryRepos
{
}