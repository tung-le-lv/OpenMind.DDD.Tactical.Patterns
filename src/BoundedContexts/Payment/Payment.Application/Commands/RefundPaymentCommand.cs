using MediatR;

namespace Payment.Application.Commands;

public record RefundPaymentCommand(Guid PaymentId, string Reason) : IRequest<bool>;
