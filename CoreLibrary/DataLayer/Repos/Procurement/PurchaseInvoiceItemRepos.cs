using DataLayer.Models.Procurement;

namespace DataLayer.Repos.Procurement;

public interface IPurchaseInvoiceItemRepos : IBaseRepos<PurchaseInvoiceItem>
{

}

public class PurchaseInvoiceItemRepos(IConnectionFactory connectionFactory) : BaseRepos<PurchaseInvoiceItem>(connectionFactory, PurchaseInvoiceItem.DatabaseObject), IPurchaseInvoiceItemRepos
{
}