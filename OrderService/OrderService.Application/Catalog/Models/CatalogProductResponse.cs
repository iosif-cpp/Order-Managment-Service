namespace OrderService.Application.Catalog.Models;

public sealed record CatalogProductResponse(
    Guid ProductId,
    string Name,
    decimal Price,
    bool IsActive,
    int Stock,
    DateTime UpdatedAt);

