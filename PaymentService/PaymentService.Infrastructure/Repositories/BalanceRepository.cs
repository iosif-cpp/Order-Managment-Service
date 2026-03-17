using Microsoft.EntityFrameworkCore;
using PaymentService.Application.Balances.Interfaces;
using PaymentService.Domain.Entities;

namespace PaymentService.Infrastructure.Repositories;

public sealed class BalanceRepository : IBalanceRepository
{
    private readonly PaymentDbContext _dbContext;

    public BalanceRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Balance?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Balances.FirstOrDefaultAsync(x => x.CustomerId == customerId, cancellationToken);
    }

    public async Task AddAsync(Balance balance, CancellationToken cancellationToken = default)
    {
        await _dbContext.Balances.AddAsync(balance, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}

