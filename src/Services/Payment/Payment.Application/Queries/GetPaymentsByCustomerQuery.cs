using MediatR;
using Payment.Application.DTOs;

namespace Payment.Application.Queries;

public record GetPaymentsByCustomerQuery(Guid CustomerId) : IRequest<IReadOnlyList<PaymentDto>>;
