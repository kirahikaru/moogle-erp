using DataLayer.Models.HMS;

namespace DataLayer.Repos.HMS;

public interface IMedicalPrescriptionItemRepos : IBaseRepos<MedicalPrescriptionItem>
{

}

public class MedicalPrescriptionItemRepos(IDbContext dbContext) : BaseRepos<MedicalPrescriptionItem>(dbContext, MedicalPrescriptionItem.DatabaseObject), IMedicalPrescriptionItemRepos
{
}