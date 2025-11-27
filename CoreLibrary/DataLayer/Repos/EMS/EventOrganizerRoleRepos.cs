using DataLayer.Models.EMS;

namespace DataLayer.Repos.EMS;
public interface IEventOrganizerRoleRepos : IBaseRepos<EventOrganizerRole>
{

}

public class EventOrganizerRoleRepos(IDbContext dbContext) : BaseRepos<EventOrganizerRole>(dbContext, EventOrganizerRole.DatabaseObject), IEventOrganizerRoleRepos
{
}