using MediatR;
using PaymentService.Application.Balances.Queries.GetBalance;

namespace PaymentService.Application.Balances.Commands.DebitBalance;

public sealed record DebitBalanceCommand(Guid CustomerId, decimal Amount) : IRequest<BalanceResult>;

