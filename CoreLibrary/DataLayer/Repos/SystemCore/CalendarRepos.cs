using SysCoreModel = DataLayer.Models.SystemCore;

namespace DataLayer.Repos.SystemCore;

public interface ICalendarRepos : IBaseRepos<SysCoreModel.Calendar>
{

}

public class CalendarRepos(IConnectionFactory connectionFactory) : BaseRepos<SysCoreModel.Calendar>(connectionFactory, SysCoreModel.Calendar.DatabaseObject), ICalendarRepos
{
}