using AuthService.Application.Auth.Interfaces;
using MediatR;

namespace AuthService.Application.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;
    private readonly ICustomerServiceClient _customerClient;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService,
        ICustomerServiceClient customerClient)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService = tokenService;
        _customerClient = customerClient;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var existing = await _refreshTokenRepository
            .GetByTokenAsync(request.RefreshToken, cancellationToken);

        if (existing is null || !existing.IsActive)
        {
            throw new InvalidOperationException("Invalid refresh token.");
        }

        var user = existing.User;

        existing.Revoke(null);

        var roles = await _customerClient.GetRolesByEmailAsync(user.Email, cancellationToken);

        var result = await _tokenService.GenerateTokensAsync(user, roles, cancellationToken);

        return result;
    }
}

