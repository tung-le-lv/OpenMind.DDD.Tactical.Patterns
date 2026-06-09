using System.Linq.Expressions;
using BuildingBlocks.Domain;
using BuildingBlocks.Domain.Specifications;

namespace Order.Domain.Specifications;

public class MinimumOrderValueSpecification(decimal minimumValue) : Specification<Aggregates.OrderAggregate.Order>
{
    public override Expression<Func<Aggregates.OrderAggregate.Order, bool>> ToExpression()
    {
        return order => order.TotalAmount.Amount >= minimumValue;
    }
}
