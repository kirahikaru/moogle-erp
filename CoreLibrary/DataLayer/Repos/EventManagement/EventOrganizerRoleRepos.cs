using DataLayer.Models.EventManagement;

namespace DataLayer.Repos.EventManagement;

public interface IEventOrganizerRoleRepos : IBaseRepos<EventOrganizerRole>
{

}

public class EventOrganizerRoleRepos(IConnectionFactory connectionFactory) : BaseRepos<EventOrganizerRole>(connectionFactory, EventOrganizerRole.DatabaseObject), IEventOrganizerRoleRepos
{
}