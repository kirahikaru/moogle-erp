using DataLayer.Models.Hobby;

namespace DataLayer.Repos.Hobby;

public interface IComicRepos : IBaseRepos<Comic>
{

}

public class ComicRepos(IDbContext dbContext) : BaseRepos<Comic>(dbContext, Comic.DatabaseObject), IComicRepos
{
}