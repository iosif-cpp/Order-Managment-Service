using CustomerService.Application.Users.Interfaces;
using MediatR;

namespace CustomerService.Application.Users.Queries.GetUserWithRoles;

public sealed class GetUserWithRolesQueryHandler
    : IRequestHandler<GetUserWithRolesQuery, UserWithRolesResponse>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IRoleRepository _roleRepository;

    public GetUserWithRolesQueryHandler(
        ICustomerRepository customerRepository,
        IRoleRepository roleRepository)
    {
        _customerRepository = customerRepository;
        _roleRepository = roleRepository;
    }

    public async Task<UserWithRolesResponse> Handle(GetUserWithRolesQuery request, CancellationToken cancellationToken)
    {
        var user = await _customerRepository.GetByIdAsync(request.UserId, cancellationToken)
                   ?? throw new KeyNotFoundException($"User '{request.UserId}' not found.");

        var roles = await _roleRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        return new UserWithRolesResponse(
            user.Id,
            user.Email,
            user.UserName,
            roles.Select(r => r.Name).ToArray());
    }
}

