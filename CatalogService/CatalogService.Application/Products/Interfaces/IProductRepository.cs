using CatalogService.Domain.Entities;

namespace CatalogService.Application.Products.Interfaces;

public interface IProductRepository
{
    Task AddAsync(Product product, CancellationToken cancellationToken = default);

    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Product>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Product>> GetAllAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

