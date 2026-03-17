namespace CustomerService.Application.Users.Events;

public sealed class CustomerRegisteredEvent
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = null!;
    public string UserName { get; init; } = null!;
    public string Password { get; init; } = null!;
    public IReadOnlyCollection<string> Roles { get; init; } = Array.Empty<string>();
}

