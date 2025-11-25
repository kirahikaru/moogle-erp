// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using DataLayer.Models.RMS;

namespace DataLayer.Repos.RMS;

public interface IOrderItemRepos : IBaseRepos<OrderItem>
{

}

public class OrderItemRepos(IDbContext dbContext) : BaseRepos<OrderItem>(dbContext, OrderItem.DatabaseObject), IOrderItemRepos
{
}