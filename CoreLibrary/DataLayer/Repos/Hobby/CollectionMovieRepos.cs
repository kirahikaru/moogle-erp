using DataLayer.Models.Hobby;

namespace DataLayer.Repos.Hobby;

public interface ICollectionMovieRepos : IBaseRepos<CollectionMovie>
{

}

public class CollectionMovieRepos(IDbContext dbContext) : BaseRepos<CollectionMovie>(dbContext, CollectionMovie.DatabaseObject), ICollectionMovieRepos
{
}