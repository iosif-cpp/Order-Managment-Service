namespace AuthService.API.Controllers;

public sealed class LogoutRequest
{
    public string RefreshToken { get; init; } = null!;
}

