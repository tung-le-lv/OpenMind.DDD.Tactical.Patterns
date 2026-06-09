using MediatR;

namespace Payment.Application.Commands;

public record CancelPaymentCommand(Guid PaymentId, string Reason) : IRequest<bool>;
