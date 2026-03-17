using CustomerService.Application.Users.Interfaces;
using CustomerService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Infrastructure.Repositories;

public sealed class RoleRepository : IRoleRepository
{
    private readonly CustomerDbContext _dbContext;

    public RoleRepository(CustomerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Role?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return _dbContext.Roles
            .FirstOrDefaultAsync(r => r.Name == name, ct);
    }

    public async Task AddAsync(Role role, CancellationToken ct = default)
    {
        await _dbContext.Roles.AddAsync(role, ct);
    }

    public async Task<IReadOnlyCollection<Role>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var roles = await _dbContext.UserRoles
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Role)
            .Select(ur => ur.Role)
            .ToListAsync(ct);

        return roles;
    }
}

