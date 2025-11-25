using DataLayer.Models.Procurement;
namespace DataLayer.Repos.Procurement;

public interface IPurchaseOrderRepos : IBaseRepos<PurchaseOrder>
{

}

public class PurchaseOrderRepos(IConnectionFactory connectionFactory) : BaseRepos<PurchaseOrder>(connectionFactory, PurchaseOrder.DatabaseObject), IPurchaseOrderRepos
{
}