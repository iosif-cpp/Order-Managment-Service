using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Application.Auth;
using AuthService.Application.Auth.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Infrastructure.Security;

public sealed class JwtOptions
{
    public string Issuer { get; init; } = null!;
    public string Audience { get; init; } = null!;
    public string SigningKey { get; init; } = null!;
    public int AccessTokenLifetimeMinutes { get; init; } = 60;
    public int RefreshTokenLifetimeDays { get; init; } = 7;
}

public sealed class TokenService : ITokenService
{
    private readonly JwtOptions _options;
    private readonly AuthDbContext _dbContext;

    public TokenService(IOptions<JwtOptions> options, AuthDbContext dbContext)
    {
        _options = options.Value;
        _dbContext = dbContext;
    }

    public async Task<AuthResponse> GenerateTokensAsync(
        User user,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName)
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = now.AddMinutes(_options.AccessTokenLifetimeMinutes);

        var jwt = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: creds);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);

        var refreshTokenValue = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        var refreshToken = new RefreshToken(
            user.Id,
            refreshTokenValue,
            now.AddDays(_options.RefreshTokenLifetimeDays),
            null);

        await _dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            ExpiresAt = expires
        };
    }
}

