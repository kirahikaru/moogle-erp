using DataLayer.Models.EMS;

namespace DataLayer.Repos.EMS;

public interface IEventOrganizerRepos : IBaseRepos<EventOrganizer>
{

}

public class EventOrganizerRepos(IDbContext dbContext) : BaseRepos<EventOrganizer>(dbContext, EventOrganizer.DatabaseObject), IEventOrganizerRepos
{
}