using DataLayer.Models.Finance;

namespace DataLayer.Repos.Finance;

public interface IInvoiceItemRepos : IBaseRepos<InvoiceItem>
{

}

public class InvoiceItemRepos(IConnectionFactory connectionFactory) : BaseRepos<InvoiceItem>(connectionFactory, Invoice.DatabaseObject), IInvoiceItemRepos
{
}