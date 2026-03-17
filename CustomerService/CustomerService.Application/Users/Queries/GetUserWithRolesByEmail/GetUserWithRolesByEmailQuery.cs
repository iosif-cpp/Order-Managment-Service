using MediatR;

namespace CustomerService.Application.Users.Queries.GetUserWithRolesByEmail;

public sealed record GetUserWithRolesByEmailQuery(string Email)
    : IRequest<UserWithRolesByEmailResponse>;

