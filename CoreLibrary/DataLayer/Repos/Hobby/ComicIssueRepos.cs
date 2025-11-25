using DataLayer.Models.Hobby;

namespace DataLayer.Repos.Hobby;

public interface IComicIssueRepos : IBaseRepos<ComicIssue>
{

}

public class ComicIssueRepos(IDbContext dbContext) : BaseRepos<ComicIssue>(dbContext, ComicIssue.DatabaseObject), IComicIssueRepos
{
}