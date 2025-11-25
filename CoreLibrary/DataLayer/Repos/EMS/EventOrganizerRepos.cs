using DataLayer.Models.Event;

namespace DataLayer.Repos.EventManagement;

public interface IEventOrganizerRepos : IBaseRepos<EventOrganizer>
{

}

public class EventOrganizerRepos(IDbContext dbContext) : BaseRepos<EventOrganizer>(dbContext, EventOrganizer.DatabaseObject), IEventOrganizerRepos
{
}