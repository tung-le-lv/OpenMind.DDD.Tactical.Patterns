using Microsoft.Extensions.Logging;
using Payment.Application.Services;

namespace Payment.Infrastructure;

/// <summary>
/// Fake payment gateway for development and demos.
///
/// Simulates realistic gateway behaviour without hitting a real provider:
///   card ending "0000" → declined by issuer
///   card ending "9999" → gateway timeout / retriable error
///   amount > 10 000    → declined by risk engine
///   everything else    → approved, returns a generated transaction ID
/// </summary>
public class FakePaymentGateway(ILogger<FakePaymentGateway> logger) : IPaymentGateway
{
    public Task<GatewayChargeResult> ChargeAsync(GatewayChargeRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "FakePaymentGateway: charging {Amount} {Currency} for {Email}",
            request.Amount, request.Currency, request.CustomerEmail);

        var result = Evaluate(request);

        if (result.Succeeded)
            logger.LogInformation("FakePaymentGateway: approved — TXN {TransactionId}", result.TransactionId);
        else
            logger.LogWarning("FakePaymentGateway: declined — {Reason}", result.ErrorMessage);

        return Task.FromResult(result);
    }

    private static GatewayChargeResult Evaluate(GatewayChargeRequest request)
    {
        if (request.CardLast4Digits == "0000")
            return GatewayChargeResult.Failure("Card declined by issuer");

        if (request.CardLast4Digits == "9999")
            return GatewayChargeResult.Failure("Gateway timeout — please retry");

        if (request.Amount > 10_000m)
            return GatewayChargeResult.Failure("Transaction blocked by risk engine — amount exceeds limit");

        var transactionId = $"TXN-{Guid.NewGuid():N}".ToUpperInvariant();
        return GatewayChargeResult.Success(transactionId);
    }
}
