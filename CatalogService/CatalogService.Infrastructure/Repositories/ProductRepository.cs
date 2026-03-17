using CatalogService.Application.Products.Interfaces;
using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly CatalogDbContext _dbContext;

    public ProductRepository(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _dbContext.Products.AddAsync(product, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Sku == sku, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Product>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .Include(p => p.Category)
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .Include(p => p.Category)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}

