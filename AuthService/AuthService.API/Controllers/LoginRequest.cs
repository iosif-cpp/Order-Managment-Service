namespace AuthService.API.Controllers;

public sealed class LoginRequest
{
    public string Email { get; init; } = null!;
    public string Password { get; init; } = null!;
}

