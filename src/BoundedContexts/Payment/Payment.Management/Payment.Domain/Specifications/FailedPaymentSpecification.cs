using System.Linq.Expressions;
using BuildingBlocks.Domain.Specifications;
using Payment.Domain.Aggregates.PaymentAggregate;

namespace Payment.Domain.Specifications;

/// <summary>
/// Specification for failed payments that might need retry or investigation.
/// </summary>
public class FailedPaymentSpecification : Specification<Aggregates.PaymentAggregate.Payment>
{
    public override Expression<Func<Aggregates.PaymentAggregate.Payment, bool>> ToExpression()
    {
        return payment => payment.Status == PaymentStatus.Failed;
    }
}
