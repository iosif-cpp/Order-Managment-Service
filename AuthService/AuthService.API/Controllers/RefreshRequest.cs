namespace AuthService.API.Controllers;

public sealed class RefreshRequest
{
    public string RefreshToken { get; init; } = null!;
}

