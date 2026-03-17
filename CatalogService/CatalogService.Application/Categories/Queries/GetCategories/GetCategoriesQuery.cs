using MediatR;

namespace CatalogService.Application.Categories.Queries.GetCategories;

public sealed record GetCategoriesQuery : IRequest<IReadOnlyCollection<CategoryResponse>>;

