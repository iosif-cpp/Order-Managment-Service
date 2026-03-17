using MediatR;
using PaymentService.Application.Balances.Queries.GetBalance;

namespace PaymentService.Application.Balances.Commands.CreditBalance;

public sealed record CreditBalanceCommand(Guid CustomerId, decimal Amount) : IRequest<BalanceResult>;

