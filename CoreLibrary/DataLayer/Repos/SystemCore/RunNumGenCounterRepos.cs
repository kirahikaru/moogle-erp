namespace DataLayer.Repos.SystemCore;

public interface IRunNumGenCounterRepos : IBaseRepos<RunNumGeneratorCounter>
{

}

public class RunNumGenCounterRepos(IConnectionFactory connectionFactory) : BaseRepos<RunNumGeneratorCounter>(connectionFactory, RunNumGeneratorCounter.DatabaseObject), IRunNumGenCounterRepos
{
}