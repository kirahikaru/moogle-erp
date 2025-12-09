using DataLayer.Models.HMS;

namespace DataLayer.Repos.HMS;

public interface IMedicalAppointmentRepos : IBaseRepos<MedAppt>
{

}

public class MedApptRepos(IDbContext dbContext) : BaseRepos<MedAppt>(dbContext, MedAppt.DatabaseObject), IMedicalAppointmentRepos
{
}