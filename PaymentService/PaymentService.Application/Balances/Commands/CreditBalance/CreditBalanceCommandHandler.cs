using MediatR;
using PaymentService.Application.Balances.Interfaces;
using PaymentService.Application.Balances.Queries.GetBalance;
using PaymentService.Application.Common.Exceptions;

namespace PaymentService.Application.Balances.Commands.CreditBalance;

public sealed class CreditBalanceCommandHandler : IRequestHandler<CreditBalanceCommand, BalanceResult>
{
    private readonly IBalanceRepository _balanceRepository;

    public CreditBalanceCommandHandler(IBalanceRepository balanceRepository)
    {
        _balanceRepository = balanceRepository;
    }

    public async Task<BalanceResult> Handle(CreditBalanceCommand request, CancellationToken cancellationToken)
    {
        var balance = await _balanceRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);
        if (balance is null)
            throw new EntityNotFoundException($"Balance for customer '{request.CustomerId}' not found.");

        balance.Credit(request.Amount);
        await _balanceRepository.SaveChangesAsync(cancellationToken);

        return new BalanceResult(balance.CustomerId, balance.Amount);
    }
}

