namespace DataLayer.Repos.FIN;

public interface IPurchaseOrderItemRepos : IBaseRepos<PurchaseOrderItem>
{

}

public class PurchaseOrderItemRepos(IDbContext dbContext) : BaseRepos<PurchaseOrderItem>(dbContext, PurchaseOrderItem.DatabaseObject), IPurchaseOrderItemRepos
{
}