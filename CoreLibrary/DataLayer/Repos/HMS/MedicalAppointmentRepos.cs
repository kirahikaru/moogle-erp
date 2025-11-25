using DataLayer.Models.HMS;

namespace DataLayer.Repos.HMS;

public interface IMedicalAppointmentRepos : IBaseRepos<MedicalAppointment>
{

}

public class MedicalAppointmentRepos(IDbContext dbContext) : BaseRepos<MedicalAppointment>(dbContext, MedicalAppointment.DatabaseObject), IMedicalAppointmentRepos
{
}