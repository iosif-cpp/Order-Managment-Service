namespace AuthService.Domain.Entities;

public sealed class User
{
    public Guid Id { get; private set; }

    public string Email { get; private set; } = null!;

    public string UserName { get; private set; } = null!;

    public string PasswordHash { get; private set; } = null!;

    public bool IsEmailConfirmed { get; private set; }

    public bool IsLocked { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    private readonly List<RefreshToken> _refreshTokens = new();

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    public User(string email, string userName, string passwordHash)
    {
        Id = Guid.NewGuid();
        Email = email;
        UserName = userName;
        PasswordHash = passwordHash;
        CreatedAt = DateTime.UtcNow;
    }

    public void Lock() => IsLocked = true;
    public void Unlock() => IsLocked = false;
    public void SetEmailConfirmed() => IsEmailConfirmed = true;

    public void SetPasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddRefreshToken(RefreshToken token)
    {
        _refreshTokens.Add(token);
    }
}
