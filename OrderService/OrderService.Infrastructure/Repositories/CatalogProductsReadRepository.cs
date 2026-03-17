using Microsoft.EntityFrameworkCore;
using OrderService.Application.Catalog.Interfaces;
using OrderService.Application.Catalog.Models;

namespace OrderService.Infrastructure.Repositories;

public sealed class CatalogProductsReadRepository : ICatalogProductsReadRepository
{
    private readonly OrderDbContext _dbContext;

    public CatalogProductsReadRepository(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyDictionary<Guid, CatalogProductResponse>> GetByIdsAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken = default)
    {
        var products = await _dbContext.CatalogProducts
            .Where(x => productIds.Contains(x.ProductId))
            .ToListAsync(cancellationToken);

        return products.ToDictionary(
            x => x.ProductId,
            x => new CatalogProductResponse(x.ProductId, x.Name, x.Price, x.IsActive, x.Stock, x.UpdatedAt));
    }
}

