using DataLayer.Models.Hobby;

namespace DataLayer.Repos.Hobby;

public interface IComicIssueRepos : IBaseRepos<ComicIssue>
{

}

public class ComicIssueRepos(IConnectionFactory connectionFactory) : BaseRepos<ComicIssue>(connectionFactory, ComicIssue.DatabaseObject), IComicIssueRepos
{
}