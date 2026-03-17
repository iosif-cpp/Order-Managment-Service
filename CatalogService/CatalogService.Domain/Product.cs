namespace CatalogService.Domain.Entities;

public sealed class Product
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public decimal Price { get; private set; }

    public string Sku { get; private set; } = null!;

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public Guid? CategoryId { get; private set; }

    public Category? Category { get; private set; }

    public int Stock { get; private set; }

    private Product()
    {
    }

    public Product(string name, string? description, decimal price, string sku, Guid? categoryId = null, int stock = 0)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Price = price;
        Sku = sku;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        CategoryId = categoryId;
        Stock = stock;
    }

    public void Rename(string name)
    {
        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeDescription(string? description)
    {
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangePrice(decimal price)
    {
        Price = price;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeSku(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU is required.", nameof(sku));

        Sku = sku;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeCategory(Guid? categoryId)
    {
        CategoryId = categoryId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetStock(int value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value));

        Stock = value;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncreaseStock(int value)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(nameof(value));

        Stock += value;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DecreaseStock(int value)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(nameof(value));
        if (value > Stock)
            throw new InvalidOperationException("Not enough stock.");

        Stock -= value;
        UpdatedAt = DateTime.UtcNow;
    }
}