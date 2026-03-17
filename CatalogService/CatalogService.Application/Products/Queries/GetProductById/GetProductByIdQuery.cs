using MediatR;

namespace CatalogService.Application.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(Guid Id) : IRequest<ProductResponse?>;

