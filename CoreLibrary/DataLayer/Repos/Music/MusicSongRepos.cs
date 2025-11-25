using DataLayer.Models.Music;

namespace DataLayer.Repos.Music;

public interface IMusicSongRepos : IBaseRepos<MusicSong>
{
}

public class MusicSongRepos(IDbContext dbContext) : BaseRepos<MusicSong>(dbContext, MusicSong.DatabaseObject), IMusicSongRepos
{
}