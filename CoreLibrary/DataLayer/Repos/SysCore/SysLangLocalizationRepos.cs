namespace DataLayer.Repos.SysCore;

public interface ISysLangLocalizationRepos : IBaseRepos<SysLangLocalization>
{

}

public class SysLangLocalizationRepos(IDbContext dbContext) : BaseRepos<SysLangLocalization>(dbContext, SysLangLocalization.DatabaseObject), ISysLangLocalizationRepos
{
}