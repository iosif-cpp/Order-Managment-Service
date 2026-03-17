using CatalogService.Domain.Entities;

namespace CatalogService.Application.Categories.Interfaces;

public interface ICategoryRepository
{
    Task AddAsync(Category category, CancellationToken cancellationToken = default);

    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Category>> GetAllAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

