using MediatR;
using Payment.Application.DTOs;

namespace Payment.Application.Queries;

public record GetPaymentByOrderIdQuery(Guid OrderId) : IRequest<PaymentDto?>;
