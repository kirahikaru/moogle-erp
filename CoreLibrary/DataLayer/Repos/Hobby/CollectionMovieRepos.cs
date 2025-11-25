using DataLayer.Models.Hobby;

namespace DataLayer.Repos.Hobby;

public interface ICollectionMovieRepos : IBaseRepos<CollectionMovie>
{

}

public class CollectionMovieRepos(IConnectionFactory connectionFactory) : BaseRepos<CollectionMovie>(connectionFactory, CollectionMovie.DatabaseObject), ICollectionMovieRepos
{
}