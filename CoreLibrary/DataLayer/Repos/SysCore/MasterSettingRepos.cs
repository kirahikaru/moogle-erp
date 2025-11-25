namespace DataLayer.Repos.SysCore;

public interface IMasterSettingRepos : IBaseRepos<MasterSetting>
{
}

public class MasterSettingRepos(IDbContext dbContext) : BaseRepos<MasterSetting>(dbContext, MasterSetting.DatabaseObject), IMasterSettingRepos
{
}