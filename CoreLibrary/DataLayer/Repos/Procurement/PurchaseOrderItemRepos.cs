using DataLayer.Models.Procurement;

namespace DataLayer.Repos.Procurement;

public interface IPurchaseOrderItemRepos : IBaseRepos<PurchaseOrderItem>
{

}

public class PurchaseOrderItemRepos(IConnectionFactory connectionFactory) : BaseRepos<PurchaseOrderItem>(connectionFactory, PurchaseOrderItem.DatabaseObject), IPurchaseOrderItemRepos
{
}