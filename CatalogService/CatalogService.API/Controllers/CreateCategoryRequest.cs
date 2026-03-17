namespace CatalogService.API.Controllers;

public sealed class CreateCategoryRequest
{
    public string Name { get; init; } = null!;

    public string? Description { get; init; }
}

