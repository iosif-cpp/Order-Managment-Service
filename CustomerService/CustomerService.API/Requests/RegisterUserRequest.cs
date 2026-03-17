namespace CustomerService.API.Requests;

public sealed class RegisterUserRequest
{
    public string Email { get; init; } = null!;
    public string UserName { get; init; } = null!;
    public string Password { get; init; } = null!;
}