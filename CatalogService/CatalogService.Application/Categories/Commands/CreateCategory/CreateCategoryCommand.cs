using MediatR;

namespace CatalogService.Application.Categories.Commands.CreateCategory;

public sealed record CreateCategoryCommand(
    string Name,
    string? Description
) : IRequest<CategoryResponse>;

