using MediatR;

namespace Payment.Application.Commands;

public record ProcessPaymentCommand(Guid PaymentId) : IRequest<bool>;
