using OrderService.Application.Catalog.Models;

namespace OrderService.Application.Catalog.Interfaces;

public interface ICatalogProductsReadRepository
{
    Task<IReadOnlyDictionary<Guid, CatalogProductResponse>> GetByIdsAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken = default);
}

