using MediatR;

namespace CustomerService.Application.Users.Commands.AssignRole;

public sealed record AssignRoleCommand(Guid UserId, string RoleName) : IRequest;

