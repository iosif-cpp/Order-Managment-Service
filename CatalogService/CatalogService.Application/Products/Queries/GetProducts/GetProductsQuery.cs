using MediatR;

namespace CatalogService.Application.Products.Queries.GetProducts;

public sealed record GetProductsQuery : IRequest<IReadOnlyCollection<ProductResponse>>;

