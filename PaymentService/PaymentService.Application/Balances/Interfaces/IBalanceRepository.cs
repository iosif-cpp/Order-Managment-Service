using PaymentService.Domain.Entities;

namespace PaymentService.Application.Balances.Interfaces;

public interface IBalanceRepository
{
    Task<Balance?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task AddAsync(Balance balance, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

