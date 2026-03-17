using MediatR;
using PaymentService.Application.Balances.Interfaces;
using PaymentService.Application.Balances.Queries.GetBalance;
using PaymentService.Application.Common.Exceptions;

namespace PaymentService.Application.Balances.Commands.DebitBalance;

public sealed class DebitBalanceCommandHandler : IRequestHandler<DebitBalanceCommand, BalanceResult>
{
    private readonly IBalanceRepository _balanceRepository;

    public DebitBalanceCommandHandler(IBalanceRepository balanceRepository)
    {
        _balanceRepository = balanceRepository;
    }

    public async Task<BalanceResult> Handle(DebitBalanceCommand request, CancellationToken cancellationToken)
    {
        var balance = await _balanceRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);
        if (balance is null)
            throw new EntityNotFoundException($"Balance for customer '{request.CustomerId}' not found.");

        if (request.Amount > balance.Amount)
            throw new InsufficientFundsException(balance.CustomerId, request.Amount, balance.Amount);

        balance.Debit(request.Amount);
        await _balanceRepository.SaveChangesAsync(cancellationToken);

        return new BalanceResult(balance.CustomerId, balance.Amount);
    }
}

