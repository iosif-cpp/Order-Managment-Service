using CustomerService.Application.Users.Interfaces;
using CustomerService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Infrastructure.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly CustomerDbContext _dbContext;

    public CustomerRepository(CustomerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return _dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await _dbContext.Users.AddAsync(user, ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        return _dbContext.SaveChangesAsync(ct);
    }
}

