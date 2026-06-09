using System.Linq.Expressions;
using BuildingBlocks.Domain;
using BuildingBlocks.Domain.Specifications;

namespace Order.Domain.Specifications;

public class MaxItemsCountSpecification(int maxItems) : Specification<Aggregates.OrderAggregate.Order>
{
    public override Expression<Func<Aggregates.OrderAggregate.Order, bool>> ToExpression()
    {
        return order => order.OrderItems.Count <= maxItems;
    }
}
