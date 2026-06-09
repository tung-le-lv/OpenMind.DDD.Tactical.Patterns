using System.Linq.Expressions;
using BuildingBlocks.Domain.Specifications;
using Payment.Domain.Aggregates.PaymentAggregate;

namespace Payment.Domain.Specifications;

/// <summary>
/// Specification for payments that can be refunded.
/// Use this for filtering/querying refundable payments in repositories.
/// </summary>
public class RefundablePaymentSpecification : Specification<Aggregates.PaymentAggregate.Payment>
{
    public override Expression<Func<Aggregates.PaymentAggregate.Payment, bool>> ToExpression()
    {
        return payment => payment.Status == PaymentStatus.Completed;
    }
}
