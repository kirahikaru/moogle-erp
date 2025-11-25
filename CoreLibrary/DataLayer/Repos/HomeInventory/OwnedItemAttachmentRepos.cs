using DataLayer.Models.HomeInventory;
namespace DataLayer.Repos.HomeInventory;

public interface IOwnedItemAttachmentRepos : IBaseRepos<OwnedItemAttachment>
{

}

public class OwnedItemAttachmentRepos(IDbContext dbContext) : BaseRepos<OwnedItemAttachment>(dbContext, OwnedItemAttachment.DatabaseObject), IOwnedItemAttachmentRepos
{
}