using DataLayer.Models.Music;

namespace DataLayer.Repos.Music;

public interface IMusicAlbumRepos : IBaseRepos<MusicAlbum>
{

}

public class MusicAlbumRepos(IConnectionFactory connectionFactory) : BaseRepos<MusicAlbum>(connectionFactory, MusicAlbum.DatabaseObject), IMusicAlbumRepos
{
}