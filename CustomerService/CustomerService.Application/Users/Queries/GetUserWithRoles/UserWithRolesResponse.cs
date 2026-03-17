namespace CustomerService.Application.Users.Queries.GetUserWithRoles;

public sealed record UserWithRolesResponse(
    Guid Id,
    string Email,
    string UserName,
    IReadOnlyCollection<string> Roles);

