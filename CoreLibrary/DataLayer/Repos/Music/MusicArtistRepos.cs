using DataLayer.Models.Music;

namespace DataLayer.Repos.Music;

public interface IMusicArtistRepos : IBaseRepos<MusicArtist>
{

}

public class MusicArtistRepos(IConnectionFactory connectionFactory) : BaseRepos<MusicArtist>(connectionFactory, MusicArtist.DatabaseObject), IMusicArtistRepos
{
}