namespace CatalogService.Application.Products;

public sealed record ProductResponse(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    string Sku,
    bool IsActive,
    int Stock,
    Guid? CategoryId);

