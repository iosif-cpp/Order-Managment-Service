namespace CatalogService.API.Controllers;

public sealed class UpdateProductRequest
{
    public string Name { get; init; } = null!;

    public string? Description { get; init; }

    public decimal Price { get; init; }

    public string Sku { get; init; } = null!;

    public Guid? CategoryId { get; init; }

     public int Stock { get; init; }
}

