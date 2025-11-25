namespace DataLayer.Repos.FIN;

public interface IPurchaseInvoiceRepos : IBaseRepos<PurchaseInvoice>
{

}

public class PurchaseInvoiceRepos(IDbContext dbContext) : BaseRepos<PurchaseInvoice>(dbContext, PurchaseInvoice.DatabaseObject), IPurchaseInvoiceRepos
{
}