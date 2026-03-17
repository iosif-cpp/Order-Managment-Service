namespace CustomerService.Application.Users.Queries.GetUserWithRolesByEmail;

public sealed record UserWithRolesByEmailResponse(
    Guid Id,
    string Email,
    string UserName,
    IReadOnlyCollection<string> Roles);

