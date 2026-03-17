namespace CatalogService.Application.Products.Events;

public sealed class ProductUpsertedEvent
{
    public Guid ProductId { get; init; }
    public string Name { get; init; } = null!;
    public decimal Price { get; init; }
    public bool IsActive { get; init; }
    public int Stock { get; init; }
    public DateTime UpdatedAt { get; init; }
}

