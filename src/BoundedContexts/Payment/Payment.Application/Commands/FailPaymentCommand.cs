using MediatR;

namespace Payment.Application.Commands;

public record FailPaymentCommand(Guid PaymentId, string Reason) : IRequest<bool>;
