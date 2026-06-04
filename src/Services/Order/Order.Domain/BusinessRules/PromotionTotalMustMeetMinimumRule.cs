using BuildingBlocks.Domain.BusinessRules;
using Order.Domain.ValueObjects;

namespace Order.Domain.BusinessRules;

/// <summary>
/// Business rule: the order total after a promotional discount is applied must
/// not fall below the required minimum order value.
///
/// Receives the already-computed projected total (produced by a side-effect-free
/// function in the aggregate) rather than recomputing it — the rule only decides
/// whether that computed value satisfies the invariant.
/// </summary>
public class PromotionTotalMustMeetMinimumRule(
    Money projectedTotal,
    decimal minimumOrderValue,
    string currency) : IBusinessRule
{
    public bool IsBroken()
        => minimumOrderValue > 0
           && !projectedTotal.IsGreaterThanOrEqualTo(Money.FromDecimal(minimumOrderValue, currency));

    public string Message
        => $"Order total after discount ({projectedTotal}) falls below the required minimum of {minimumOrderValue:C}.";

    public string Code => "PROMOTION_TOTAL_BELOW_MINIMUM";
}
