using DataLayer.Models.Hospital;

namespace DataLayer.Repos.Hospital;

public interface IMedicalPrescriptionItemRepos : IBaseRepos<MedicalPrescriptionItem>
{

}

public class MedicalPrescriptionItemRepos(IConnectionFactory connectionFactory) : BaseRepos<MedicalPrescriptionItem>(connectionFactory, MedicalPrescriptionItem.DatabaseObject), IMedicalPrescriptionItemRepos
{
}