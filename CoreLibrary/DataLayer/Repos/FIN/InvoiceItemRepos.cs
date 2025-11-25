namespace DataLayer.Repos.FIN;

public interface IInvoiceItemRepos : IBaseRepos<InvoiceItem>
{

}

public class InvoiceItemRepos(IDbContext dbContext) : BaseRepos<InvoiceItem>(dbContext, Invoice.DatabaseObject), IInvoiceItemRepos
{
}