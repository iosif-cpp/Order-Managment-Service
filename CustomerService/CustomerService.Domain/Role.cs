namespace CustomerService.Domain.Entities;

public sealed class Role
{ 
    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    private readonly List<UserRole> _userRoles = new();

    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    public Role(string name, string? description = null)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
    }
}