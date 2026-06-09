using MediatR;
using Payment.Application.DTOs;

namespace Payment.Application.Queries;

public record GetPaymentsByStatusQuery(string Status) : IRequest<IReadOnlyList<PaymentDto>>;
