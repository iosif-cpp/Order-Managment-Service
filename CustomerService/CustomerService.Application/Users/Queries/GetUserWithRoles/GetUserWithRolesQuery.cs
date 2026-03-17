using MediatR;

namespace CustomerService.Application.Users.Queries.GetUserWithRoles;

public sealed record GetUserWithRolesQuery(Guid UserId)
    : IRequest<UserWithRolesResponse>;

