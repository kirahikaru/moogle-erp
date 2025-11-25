using DataLayer.Models.HomeInventory;
namespace DataLayer.Repos.HomeInventory;

public interface IOwnedItemAttachmentRepos : IBaseRepos<OwnedItemAttachment>
{

}

public class OwnedItemAttachmentRepos(IConnectionFactory connectionFactory) : BaseRepos<OwnedItemAttachment>(connectionFactory, OwnedItemAttachment.DatabaseObject), IOwnedItemAttachmentRepos
{
}