using System.Linq.Expressions;
using BuildingBlocks.Domain;
using BuildingBlocks.Domain.Specifications;
using Order.Domain.Aggregates.OrderAggregate;

namespace Order.Domain.Specifications;

public class CancellableOrderSpecification : Specification<Aggregates.OrderAggregate.Order>
{
    public override Expression<Func<Aggregates.OrderAggregate.Order, bool>> ToExpression()
    {
        return order => order.Status == OrderStatus.Draft ||
                        order.Status == OrderStatus.Submitted ||
                        order.Status == OrderStatus.PaymentFailed;
    }
}
