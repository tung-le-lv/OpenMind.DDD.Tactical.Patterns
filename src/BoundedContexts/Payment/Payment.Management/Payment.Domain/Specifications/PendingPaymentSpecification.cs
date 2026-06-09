using System.Linq.Expressions;
using BuildingBlocks.Domain.Specifications;
using Payment.Domain.Aggregates.PaymentAggregate;

namespace Payment.Domain.Specifications;

/// <summary>
/// Specification for payments that are awaiting processing.
/// Use this for filtering/querying pending payments in repositories.
/// </summary>
public class PendingPaymentSpecification : Specification<Aggregates.PaymentAggregate.Payment>
{
    public override Expression<Func<Aggregates.PaymentAggregate.Payment, bool>> ToExpression()
    {
        return payment => payment.Status == PaymentStatus.Pending;
    }
}
