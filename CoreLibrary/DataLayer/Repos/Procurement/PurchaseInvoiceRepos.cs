using DataLayer.Models.Procurement;

namespace DataLayer.Repos.Procurement;

public interface IPurchaseInvoiceRepos : IBaseRepos<PurchaseInvoice>
{

}

public class PurchaseInvoiceRepos(IConnectionFactory connectionFactory) : BaseRepos<PurchaseInvoice>(connectionFactory, PurchaseInvoice.DatabaseObject), IPurchaseInvoiceRepos
{
}