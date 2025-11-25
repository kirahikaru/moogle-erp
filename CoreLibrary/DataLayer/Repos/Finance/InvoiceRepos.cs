using DataLayer.Models.Finance;

namespace DataLayer.Repos.Finance;

public interface IInvoiceRepos : IBaseRepos<Invoice>
{

}

public class InvoiceRepos(IConnectionFactory connectionFactory) : BaseRepos<Invoice>(connectionFactory, Invoice.DatabaseObject), IInvoiceRepos
{
}