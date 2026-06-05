using System.Linq.Expressions;
using BuildingBlocks.Domain.Specifications;

namespace Payment.Domain.Specifications;

/// <summary>
/// Specification for high-value payments that may require additional verification.
/// Use this for filtering payments that exceed a certain amount threshold.
/// </summary>
public class HighValuePaymentSpecification(decimal threshold = 1000m) 
    : Specification<Aggregates.PaymentAggregate.Payment>
{
    public override Expression<Func<Aggregates.PaymentAggregate.Payment, bool>> ToExpression()
    {
        return payment => payment.Amount.Amount >= threshold;
    }
}
