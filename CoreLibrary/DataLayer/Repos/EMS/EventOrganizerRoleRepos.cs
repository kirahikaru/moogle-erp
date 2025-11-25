using DataLayer.Models.Event;

namespace DataLayer.Repos.Event;
public interface IEventOrganizerRoleRepos : IBaseRepos<EventOrganizerRole>
{

}

public class EventOrganizerRoleRepos(IDbContext dbContext) : BaseRepos<EventOrganizerRole>(dbContext, EventOrganizerRole.DatabaseObject), IEventOrganizerRoleRepos
{
}