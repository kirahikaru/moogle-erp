using DataLayer.Models.HMS;

namespace DataLayer.Repos.HMS;

public interface IMedicalPrescriptionItemRepos : IBaseRepos<MedRxItem>
{

}

public class MedApptItemRepos(IDbContext dbContext) : BaseRepos<MedRxItem>(dbContext, MedRxItem.DatabaseObject), IMedicalPrescriptionItemRepos
{
}