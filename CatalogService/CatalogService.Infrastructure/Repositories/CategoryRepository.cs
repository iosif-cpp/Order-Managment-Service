using CatalogService.Application.Categories.Interfaces;
using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly CatalogDbContext _dbContext;

    public CategoryRepository(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        await _dbContext.Categories.AddAsync(category, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}

