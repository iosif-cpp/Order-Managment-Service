using System.Net.Http.Json;
using AuthService.Application.Auth.Interfaces;

namespace AuthService.Infrastructure.Customers;

public sealed class CustomerServiceClient : ICustomerServiceClient
{
    private readonly HttpClient _client;

    public CustomerServiceClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<IReadOnlyCollection<string>> GetRolesByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.GetAsync($"/api/customers/by-email/{Uri.EscapeDataString(email)}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Array.Empty<string>();
            }

            var customer = await response.Content.ReadFromJsonAsync<CustomerResponse>(cancellationToken: cancellationToken);
            return customer?.Roles ?? Array.Empty<string>();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    private sealed class CustomerResponse
    {
        public Guid Id { get; init; }
        public string Email { get; init; } = null!;
        public string UserName { get; init; } = null!;
        public IReadOnlyCollection<string> Roles { get; init; } = Array.Empty<string>();
    }
}

