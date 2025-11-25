using DataLayer.Models.Music;

namespace DataLayer.Repos.Music;

public interface IMusicSongArtistRepos : IBaseRepos<MusicSongArtist>
{
}


public class MusicSongArtistRepos(IConnectionFactory connectionFactory) : BaseRepos<MusicSongArtist>(connectionFactory, MusicSongArtist.DatabaseObject), IMusicSongArtistRepos
{
}