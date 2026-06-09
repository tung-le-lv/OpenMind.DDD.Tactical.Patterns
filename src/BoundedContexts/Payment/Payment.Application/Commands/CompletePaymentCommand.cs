using MediatR;

namespace Payment.Application.Commands;

public record CompletePaymentCommand(Guid PaymentId, string TransactionId) : IRequest<bool>;
