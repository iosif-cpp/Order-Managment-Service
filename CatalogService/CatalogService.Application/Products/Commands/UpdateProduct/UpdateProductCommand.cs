using MediatR;

namespace CatalogService.Application.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    string Sku,
    Guid? CategoryId,
    int Stock
) : IRequest<ProductResponse>;

