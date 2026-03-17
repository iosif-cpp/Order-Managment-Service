using MediatR;

namespace CustomerService.Application.Users.Commands.RegisterUser;

public sealed record RegisterUserCommand(
    string Email,
    string UserName,
    string Password
) : IRequest<Guid>;

