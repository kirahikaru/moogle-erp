using DataLayer.Models.EventManagement;

namespace DataLayer.Repos.EventManagement;

public interface IEventOrganizerRepos : IBaseRepos<EventOrganizer>
{

}

public class EventOrganizerRepos(IConnectionFactory connectionFactory) : BaseRepos<EventOrganizer>(connectionFactory, EventOrganizer.DatabaseObject), IEventOrganizerRepos
{
}