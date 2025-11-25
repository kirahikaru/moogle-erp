namespace DataLayer.Repos.SystemCore;

public interface ISysLangLocalizationRepos : IBaseRepos<SysLangLocalization>
{

}

public class SysLangLocalizationRepos(IConnectionFactory connectionFactory) : BaseRepos<SysLangLocalization>(connectionFactory, SysLangLocalization.DatabaseObject), ISysLangLocalizationRepos
{
}