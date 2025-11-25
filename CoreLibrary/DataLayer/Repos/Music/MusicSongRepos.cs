using DataLayer.Models.Music;

namespace DataLayer.Repos.Music;

public interface IMusicSongRepos : IBaseRepos<MusicSong>
{
}

public class MusicSongRepos(IConnectionFactory connectionFactory) : BaseRepos<MusicSong>(connectionFactory, MusicSong.DatabaseObject), IMusicSongRepos
{
}