using System.Security.Cryptography;
using System.Text;
using AuthService.Application.Auth.Interfaces;

namespace AuthService.Infrastructure.Security;

public sealed class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        var hashOfInput = HashPassword(password);
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(hashOfInput),
            Convert.FromBase64String(passwordHash));
    }
}

