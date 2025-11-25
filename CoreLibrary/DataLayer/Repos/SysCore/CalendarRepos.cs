using SysCoreModel = DataLayer.Models.SysCore;

namespace DataLayer.Repos.SysCore;

public interface ICalendarRepos : IBaseRepos<SysCoreModel.Calendar>
{

}

public class CalendarRepos(IDbContext dbContext) : BaseRepos<SysCoreModel.Calendar>(dbContext, SysCoreModel.Calendar.DatabaseObject), ICalendarRepos
{
}