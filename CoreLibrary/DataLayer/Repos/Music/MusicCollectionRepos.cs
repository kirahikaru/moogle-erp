using DataLayer.Models.Music;

namespace DataLayer.Repos.Music;

public interface IMusicCollectionRepos : IBaseRepos<MusicCollection>
{
}


public class MusicCollectionRepos(IConnectionFactory connectionFactory) : BaseRepos<MusicCollection>(connectionFactory, MusicCollection.DatabaseObject), IMusicCollectionRepos
{
}