using MediatR;
using Payment.Application.DTOs;

namespace Payment.Application.Queries;

public record GetPaymentByIdQuery(Guid PaymentId) : IRequest<PaymentDto?>;
