namespace DataLayer.Repos.SystemCore;

public interface IMasterSettingRepos : IBaseRepos<MasterSetting>
{
}

public class MasterSettingRepos(IConnectionFactory connectionFactory) : BaseRepos<MasterSetting>(connectionFactory, MasterSetting.DatabaseObject), IMasterSettingRepos
{
}