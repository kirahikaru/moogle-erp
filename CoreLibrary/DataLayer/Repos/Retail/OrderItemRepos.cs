// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using DataLayer.Models.Retail;

namespace DataLayer.Repos.Retail;

public interface IOrderItemRepos : IBaseRepos<OrderItem>
{

}

public class OrderItemRepos(IConnectionFactory connectionFactory) : BaseRepos<OrderItem>(connectionFactory, OrderItem.DatabaseObject), IOrderItemRepos
{
}