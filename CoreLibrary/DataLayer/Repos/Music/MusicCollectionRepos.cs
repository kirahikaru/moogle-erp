using DataLayer.Models.Music;

namespace DataLayer.Repos.Music;

public interface IMusicCollectionRepos : IBaseRepos<MusicCollection>
{
}


public class MusicCollectionRepos(IDbContext dbContext) : BaseRepos<MusicCollection>(dbContext, MusicCollection.DatabaseObject), IMusicCollectionRepos
{
}