using DataLayer.Models.Music;

namespace DataLayer.Repos.Music;

public interface IMusicAlbumRepos : IBaseRepos<MusicAlbum>
{

}

public class MusicAlbumRepos(IDbContext dbContext) : BaseRepos<MusicAlbum>(dbContext, MusicAlbum.DatabaseObject), IMusicAlbumRepos
{
}