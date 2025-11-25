using DataLayer.Models.Hobby;

namespace DataLayer.Repos.Hobby;

public interface IComicRepos : IBaseRepos<Comic>
{

}

public class ComicRepos(IConnectionFactory connectionFactory) : BaseRepos<Comic>(connectionFactory, Comic.DatabaseObject), IComicRepos
{
}