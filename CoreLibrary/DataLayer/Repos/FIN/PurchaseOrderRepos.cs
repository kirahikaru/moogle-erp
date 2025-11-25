namespace DataLayer.Repos.FIN;

public interface IPurchaseOrderRepos : IBaseRepos<PurchaseOrder>
{

}

public class PurchaseOrderRepos(IDbContext dbContext) : BaseRepos<PurchaseOrder>(dbContext, PurchaseOrder.DatabaseObject), IPurchaseOrderRepos
{
}