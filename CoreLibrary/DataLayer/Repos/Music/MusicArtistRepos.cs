using DataLayer.Models.Music;

namespace DataLayer.Repos.Music;

public interface IMusicArtistRepos : IBaseRepos<MusicArtist>
{

}

public class MusicArtistRepos(IDbContext dbContext) : BaseRepos<MusicArtist>(dbContext, MusicArtist.DatabaseObject), IMusicArtistRepos
{
}