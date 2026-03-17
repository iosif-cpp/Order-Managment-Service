using CustomerService.Domain.Entities;

namespace CustomerService.Application.Users.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(string name, CancellationToken ct = default);
    Task AddAsync(Role role, CancellationToken ct = default);
    Task<IReadOnlyCollection<Role>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
}

