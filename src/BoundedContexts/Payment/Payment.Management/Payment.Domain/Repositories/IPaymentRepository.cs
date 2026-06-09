using BuildingBlocks.Domain;
using BuildingBlocks.Domain.Specifications;
using Payment.Domain.Aggregates.PaymentAggregate;
using Payment.Domain.ValueObjects;

namespace Payment.Domain.Repositories;

/// <summary>
/// Repository for Payment Aggregate Root.
/// </summary>
public interface IPaymentRepository : IRepository<Aggregates.PaymentAggregate.Payment, PaymentId>
{
    Task<Aggregates.PaymentAggregate.Payment?> GetByOrderIdAsync(
        OrderReference orderId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Aggregates.PaymentAggregate.Payment>> GetByCustomerIdAsync(
        CustomerReference customerId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Aggregates.PaymentAggregate.Payment>> GetByStatusAsync(
        PaymentStatus status,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Aggregates.PaymentAggregate.Payment>> GetPendingPaymentsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds payments matching the given specification.
    /// Specifications can be composed using And, Or, Not operators.
    /// </summary>
    Task<IReadOnlyList<Aggregates.PaymentAggregate.Payment>> FindAsync(
        Specification<Aggregates.PaymentAggregate.Payment> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets payments that can be refunded.
    /// Uses RefundablePaymentSpecification internally.
    /// </summary>
    Task<IReadOnlyList<Aggregates.PaymentAggregate.Payment>> GetRefundablePaymentsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets failed payments for retry or investigation.
    /// Uses FailedPaymentSpecification internally.
    /// </summary>
    Task<IReadOnlyList<Aggregates.PaymentAggregate.Payment>> GetFailedPaymentsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets high-value payments that may require additional verification.
    /// Uses HighValuePaymentSpecification internally.
    /// </summary>
    Task<IReadOnlyList<Aggregates.PaymentAggregate.Payment>> GetHighValuePaymentsAsync(
        decimal threshold = 1000m,
        CancellationToken cancellationToken = default);
}
