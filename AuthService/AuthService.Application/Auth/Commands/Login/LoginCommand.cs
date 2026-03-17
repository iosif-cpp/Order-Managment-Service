using MediatR;

namespace AuthService.Application.Auth.Commands.Login;

public sealed record LoginCommand(
    string Email,
    string Password
) : IRequest<AuthResponse>;

