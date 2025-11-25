using DataLayer.Models.Hospital;

namespace DataLayer.Repos.Hospital;

public interface IMedicalAppointmentRepos : IBaseRepos<MedicalAppointment>
{

}

public class MedicalAppointmentRepos(IConnectionFactory connectionFactory) : BaseRepos<MedicalAppointment>(connectionFactory, MedicalAppointment.DatabaseObject), IMedicalAppointmentRepos
{
}