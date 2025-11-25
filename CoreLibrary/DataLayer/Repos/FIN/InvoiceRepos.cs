namespace DataLayer.Repos.FIN;

public interface IInvoiceRepos : IBaseRepos<Invoice>
{

}

public class InvoiceRepos(IDbContext dbContext) : BaseRepos<Invoice>(dbContext, Invoice.DatabaseObject), IInvoiceRepos
{
}