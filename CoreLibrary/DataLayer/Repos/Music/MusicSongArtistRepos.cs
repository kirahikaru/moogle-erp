using DataLayer.Models.Music;

namespace DataLayer.Repos.Music;

public interface IMusicSongArtistRepos : IBaseRepos<MusicSongArtist>
{
}


public class MusicSongArtistRepos(IDbContext dbContext) : BaseRepos<MusicSongArtist>(dbContext, MusicSongArtist.DatabaseObject), IMusicSongArtistRepos
{
}