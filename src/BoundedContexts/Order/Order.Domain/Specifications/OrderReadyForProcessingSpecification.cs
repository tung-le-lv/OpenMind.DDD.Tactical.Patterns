using System.Linq.Expressions;
using BuildingBlocks.Domain;
using BuildingBlocks.Domain.Specifications;
using Order.Domain.Aggregates.OrderAggregate;

namespace Order.Domain.Specifications;

/// <summary>
/// DDD Specification Pattern:
/// Encapsulates a business rule that can be reused for querying and validation.
/// </summary>
public class OrderReadyForProcessingSpecification : Specification<Aggregates.OrderAggregate.Order>
{
    public override Expression<Func<Aggregates.OrderAggregate.Order, bool>> ToExpression()
    {
        return order => order.Status == OrderStatus.Paid;
    }
}
