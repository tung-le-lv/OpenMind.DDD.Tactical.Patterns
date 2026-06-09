using MediatR;
using Payment.Application.DTOs;

namespace Payment.Application.Queries;

public record GetPendingPaymentsQuery : IRequest<IReadOnlyList<PaymentDto>>;
