using AuthService.Application.Auth.Interfaces;
using MediatR;

namespace AuthService.Application.Auth.Commands.Logout;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public LogoutCommandHandler(IRefreshTokenRepository refreshTokenRepository)
    {
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var existing = await _refreshTokenRepository
            .GetByTokenAsync(request.RefreshToken, cancellationToken);

        if (existing is null || !existing.IsActive)
        {
            return Unit.Value;
        }

        existing.Revoke(null);

        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

