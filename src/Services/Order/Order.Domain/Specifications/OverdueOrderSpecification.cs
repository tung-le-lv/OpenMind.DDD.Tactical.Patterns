using System.Linq.Expressions;
using BuildingBlocks.Domain.Specifications;
using Order.Domain.Aggregates.OrderAggregate;

namespace Order.Domain.Specifications;

public class OverdueOrderSpecification(int hours = 24) : Specification<Aggregates.OrderAggregate.Order>
{
    public override Expression<Func<Aggregates.OrderAggregate.Order, bool>> ToExpression()
    {
        var cutoffTime = DateTime.UtcNow.AddHours(-hours);
        return order => order.Status == OrderStatus.Submitted &&
                        order.SubmittedAt.HasValue &&
                        order.SubmittedAt.Value < cutoffTime;
    }
}
