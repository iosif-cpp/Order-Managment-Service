using MediatR;

namespace PaymentService.Application.Balances.Queries.GetBalance;

public sealed record GetBalanceQuery(Guid CustomerId) : IRequest<BalanceResult>;

public sealed record BalanceResult(Guid CustomerId, decimal Amount);

