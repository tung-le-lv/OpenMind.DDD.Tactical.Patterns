using Payment.Contracts;

namespace Payment.Application.Services;

/// <summary>
/// Port to the external payment gateway.
/// Defined in the application layer so the domain stays free of infrastructure
/// concerns. Infrastructure provides the adapter (fake or real).
/// </summary>
public interface IPaymentGateway
{
    Task<GatewayChargeResult> ChargeAsync(GatewayChargeRequest request, CancellationToken cancellationToken = default);
}

public record GatewayChargeRequest(
    string CustomerEmail,
    BillingAddress BillingAddress,
    decimal Amount,
    string Currency,
    string? CardLast4Digits,
    string? CardType,
    string? CardHolderName);

public record GatewayChargeResult
{
    public bool Succeeded { get; init; }
    public string? TransactionId { get; init; }
    public string? ErrorMessage { get; init; }

    public static GatewayChargeResult Success(string transactionId) =>
        new() { Succeeded = true, TransactionId = transactionId };

    public static GatewayChargeResult Failure(string errorMessage) =>
        new() { Succeeded = false, ErrorMessage = errorMessage };
}
