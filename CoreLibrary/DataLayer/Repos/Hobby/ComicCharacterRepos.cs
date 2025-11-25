using DataLayer.Models.Hobby;

namespace DataLayer.Repos.Hobby;

public interface IComicCharacterRepos : IBaseRepos<ComicCharacter>
{

}

public class ComicCharacterRepos(IDbContext dbContext) : BaseRepos<ComicCharacter>(dbContext, ComicCharacter.DatabaseObject), IComicCharacterRepos
{
}