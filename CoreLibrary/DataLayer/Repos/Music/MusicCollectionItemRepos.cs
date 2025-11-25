using DataLayer.Models.Music;


namespace DataLayer.Repos.Music;

public interface IMusicCollectionItemRepos : IBaseRepos<MusicCollectionItem>
{
}


public class MusicCollectionItemRepos(IDbContext dbContext) : BaseRepos<MusicCollectionItem>(dbContext, MusicCollectionItem.DatabaseObject), IMusicCollectionItemRepos
{
}