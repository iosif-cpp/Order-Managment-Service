using MediatR;

namespace AuthService.Application.Auth.Commands.Logout;

public sealed record LogoutCommand(
    string RefreshToken
) : IRequest;

