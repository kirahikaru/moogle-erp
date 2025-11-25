using DataLayer.Models.Hobby;

namespace DataLayer.Repos.Hobby;

public interface IComicCharacterRepos : IBaseRepos<ComicCharacter>
{

}

public class ComicCharacterRepos(IConnectionFactory connectionFactory) : BaseRepos<ComicCharacter>(connectionFactory, ComicCharacter.DatabaseObject), IComicCharacterRepos
{
}