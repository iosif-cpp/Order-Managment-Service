using MediatR;

namespace CatalogService.Application.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(
    string Name,
    string? Description,
    decimal Price,
    string Sku,
    Guid? CategoryId,
    int Stock
) : IRequest<ProductResponse>;

