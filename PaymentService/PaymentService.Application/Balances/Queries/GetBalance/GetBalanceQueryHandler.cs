using MediatR;
using PaymentService.Application.Balances.Interfaces;
using PaymentService.Application.Common.Exceptions;

namespace PaymentService.Application.Balances.Queries.GetBalance;

public sealed class GetBalanceQueryHandler : IRequestHandler<GetBalanceQuery, BalanceResult>
{
    private readonly IBalanceRepository _balanceRepository;

    public GetBalanceQueryHandler(IBalanceRepository balanceRepository)
    {
        _balanceRepository = balanceRepository;
    }

    public async Task<BalanceResult> Handle(GetBalanceQuery request, CancellationToken cancellationToken)
    {
        var balance = await _balanceRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);
        if (balance is null)
            throw new EntityNotFoundException($"Balance for customer '{request.CustomerId}' not found.");

        return new BalanceResult(balance.CustomerId, balance.Amount);
    }
}

