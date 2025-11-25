namespace DataLayer.Repos.SysCore;

public interface IRunNumGenCounterRepos : IBaseRepos<RunNumGeneratorCounter>
{

}

public class RunNumGenCounterRepos(IDbContext dbContext) : BaseRepos<RunNumGeneratorCounter>(dbContext, RunNumGeneratorCounter.DatabaseObject), IRunNumGenCounterRepos
{
}