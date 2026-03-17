namespace CustomerService.Domain.Entities;

public sealed class User
{
    public Guid Id { get; private set; }

    public string Email { get; private set; } = null!;

    public string UserName { get; private set; } = null!;
    
    public bool IsLocked { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    private readonly List<UserRole> _userRoles = new();

    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    public User(string email, string userName)
    {
        Id = Guid.NewGuid();
        Email = email;
        UserName = userName;
        CreatedAt = DateTime.UtcNow;
        IsLocked = false;
    }
    public void Lock() => IsLocked = true;
    public void Unlock() => IsLocked = false;
    public void AddRole(Role role)
    {
        if (_userRoles.Any(ur => ur.RoleId == role.Id))
        {
            return;
        }

        _userRoles.Add(new UserRole(Id, role.Id));
    }
    public void RemoveRole(Role role)
    {
        var link = _userRoles.FirstOrDefault(ur => ur.RoleId == role.Id);
        if (link is null)
        {
            return;
        }

        _userRoles.Remove(link);
    }
}