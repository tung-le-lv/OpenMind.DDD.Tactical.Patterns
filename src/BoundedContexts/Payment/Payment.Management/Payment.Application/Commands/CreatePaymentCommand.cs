using MediatR;
using Payment.Application.DTOs;

namespace Payment.Application.Commands;

public record CreatePaymentCommand(
    Guid OrderId,
    Guid CustomerId,
    decimal Amount,
    string Currency = "USD",
    string Method = "CreditCard",
    CardDetailsDto? CardDetails = null) : IRequest<Guid>;
