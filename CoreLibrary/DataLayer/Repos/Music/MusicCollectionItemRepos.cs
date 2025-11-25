using DataLayer.Models.Music;


namespace DataLayer.Repos.Music;

public interface IMusicCollectionItemRepos : IBaseRepos<MusicCollectionItem>
{
}


public class MusicCollectionItemRepos(IConnectionFactory connectionFactory) : BaseRepos<MusicCollectionItem>(connectionFactory, MusicCollectionItem.DatabaseObject), IMusicCollectionItemRepos
{
}