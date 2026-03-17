using MediatR;

namespace AuthService.Application.Auth.Commands.RefreshToken;

public sealed record RefreshTokenCommand(
    string RefreshToken
) : IRequest<AuthResponse>;

