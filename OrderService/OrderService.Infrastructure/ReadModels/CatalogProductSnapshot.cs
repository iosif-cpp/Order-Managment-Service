namespace OrderService.Infrastructure.ReadModels;

public sealed class CatalogProductSnapshot
{
    public Guid ProductId { get; private set; }
    public string Name { get; private set; } = null!;
    public decimal Price { get; private set; }
    public bool IsActive { get; private set; }
    public int Stock { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private CatalogProductSnapshot()
    {
    }

    public CatalogProductSnapshot(Guid productId, string name, decimal price, bool isActive, int stock, DateTime updatedAt)
    {
        ProductId = productId;
        Name = name;
        Price = price;
        IsActive = isActive;
        Stock = stock;
        UpdatedAt = updatedAt;
    }

    public void Update(string name, decimal price, bool isActive, int stock, DateTime updatedAt)
    {
        Name = name;
        Price = price;
        IsActive = isActive;
        Stock = stock;
        UpdatedAt = updatedAt;
    }
}

