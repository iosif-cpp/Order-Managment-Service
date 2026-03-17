namespace CatalogService.Domain.Entities;

public sealed class Category
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    private Category()
    {
    }

    public Category(string name, string? description)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
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
}

