namespace AuthService.Application.Auth.Interfaces;

public interface ICustomerServiceClient
{
    Task<IReadOnlyCollection<string>> GetRolesByEmailAsync(string email, CancellationToken cancellationToken = default);
}

