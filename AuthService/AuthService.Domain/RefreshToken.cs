namespace AuthService.Domain.Entities;

public sealed class RefreshToken
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }
    public string Token { get; private set; } = null!;

    public DateTime ExpiresAt { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? RevokedAt { get; private set; }

    public string? CreatedByIp { get; private set; }

    public string? RevokedByIp { get; private set; }

    public string? ReplacedByToken { get; private set; }

    public User User { get; private set; } = null!;

    public RefreshToken(Guid userId, string token, DateTime expiresAt, string? createdByIp)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
        CreatedByIp = createdByIp;
    }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => RevokedAt is null && !IsExpired;

    public void Revoke(string? revokedByIp, string? replacedByToken = null)
    {
        if (RevokedAt is not null)
        {
            return;
        }

        RevokedAt = DateTime.UtcNow;
        RevokedByIp = revokedByIp;
        ReplacedByToken = replacedByToken;
    }
}

