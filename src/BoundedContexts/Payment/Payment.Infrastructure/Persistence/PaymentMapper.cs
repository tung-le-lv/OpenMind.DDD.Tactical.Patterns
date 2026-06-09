using BuildingBlocks.Domain;
using Payment.Domain.Aggregates.PaymentAggregate;
using Payment.Domain.ValueObjects;
using Payment.Infrastructure.Persistence.Documents;

namespace Payment.Infrastructure.Persistence;

public static class PaymentMapper
{
    public static PaymentDocument ToDocument(Domain.Aggregates.PaymentAggregate.Payment payment) => new()
    {
        Id           = payment.Id.Value,
        OrderId      = payment.OrderId.Value,
        CustomerId   = payment.CustomerId.Value,
        AmountValue  = payment.Amount.Amount,
        Currency     = payment.Amount.Currency,
        Status       = payment.Status.Name,
        Method       = payment.Method.Name,
        CardDetails  = payment.CardDetails is null ? null : ToCardDetailsDocument(payment.CardDetails),
        TransactionId = payment.TransactionId,
        FailureReason = payment.FailureReason,
        CreatedAt    = payment.CreatedAt,
        ProcessedAt  = payment.ProcessedAt,
        CompletedAt  = payment.CompletedAt,
        Version      = payment.Version
    };

    public static Domain.Aggregates.PaymentAggregate.Payment ToDomain(PaymentDocument doc) =>
        Domain.Aggregates.PaymentAggregate.Payment.Reconstitute(
            id:            PaymentId.From(doc.Id),
            orderId:       OrderReference.From(doc.OrderId),
            customerId:    CustomerReference.From(doc.CustomerId),
            amount:        new Money(doc.AmountValue, doc.Currency),
            status:        Enumeration.FromDisplayName<PaymentStatus>(doc.Status),
            method:        Enumeration.FromDisplayName<PaymentMethod>(doc.Method),
            cardDetails:   doc.CardDetails is null ? null : ToCardDetailsDomain(doc.CardDetails),
            transactionId: doc.TransactionId,
            failureReason: doc.FailureReason,
            createdAt:     doc.CreatedAt,
            processedAt:   doc.ProcessedAt,
            completedAt:   doc.CompletedAt,
            version:       doc.Version
        );

    private static CardDetailsDocument ToCardDetailsDocument(CardDetails card) => new()
    {
        Last4Digits    = card.Last4Digits,
        CardType       = card.CardType,
        ExpiryMonth    = card.ExpiryMonth,
        ExpiryYear     = card.ExpiryYear,
        CardHolderName = card.CardHolderName
    };

    private static CardDetails ToCardDetailsDomain(CardDetailsDocument doc) =>
        CardDetails.Reconstitute(
            last4Digits:    doc.Last4Digits,
            cardType:       doc.CardType,
            expiryMonth:    doc.ExpiryMonth,
            expiryYear:     doc.ExpiryYear,
            cardHolderName: doc.CardHolderName
        );
}
