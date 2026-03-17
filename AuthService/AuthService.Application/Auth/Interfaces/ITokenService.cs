using AuthService.Domain.Entities;

namespace AuthService.Application.Auth.Interfaces;

public interface ITokenService
{
    Task<AuthResponse> GenerateTokensAsync(
        User user,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken = default);
}

