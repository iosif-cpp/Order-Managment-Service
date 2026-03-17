namespace CustomerService.API.Requests;

public sealed class ChangeRoleRequest
{
    public string RoleName { get; init; } = null!;
}