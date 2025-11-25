namespace DataLayer.Repos.FIN;

public interface IPurchaseInvoiceItemRepos : IBaseRepos<PurchaseInvoiceItem>
{

}

public class PurchaseInvoiceItemRepos(IDbContext dbContext) : BaseRepos<PurchaseInvoiceItem>(dbContext, PurchaseInvoiceItem.DatabaseObject), IPurchaseInvoiceItemRepos
{
}