using CatalogService.Domain.Entities;

namespace CatalogService.Application.Products.Interfaces;

public interface IProductEventsPublisher
{
    Task PublishProductUpsertedAsync(Product product, CancellationToken cancellationToken = default);
}

