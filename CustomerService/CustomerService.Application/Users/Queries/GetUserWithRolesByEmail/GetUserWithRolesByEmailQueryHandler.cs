using CustomerService.Application.Users.Interfaces;
using MediatR;

namespace CustomerService.Application.Users.Queries.GetUserWithRolesByEmail;

public sealed class GetUserWithRolesByEmailQueryHandler
    : IRequestHandler<GetUserWithRolesByEmailQuery, UserWithRolesByEmailResponse>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IRoleRepository _roleRepository;

    public GetUserWithRolesByEmailQueryHandler(
        ICustomerRepository customerRepository,
        IRoleRepository roleRepository)
    {
        _customerRepository = customerRepository;
        _roleRepository = roleRepository;
    }

    public async Task<UserWithRolesByEmailResponse> Handle(GetUserWithRolesByEmailQuery request, CancellationToken cancellationToken)
    {
        var user = await _customerRepository.GetByEmailAsync(request.Email, cancellationToken)
                   ?? throw new KeyNotFoundException($"User '{request.Email}' not found.");

        var roles = await _roleRepository.GetByUserIdAsync(user.Id, cancellationToken);

        return new UserWithRolesByEmailResponse(
            user.Id,
            user.Email,
            user.UserName,
            roles.Select(r => r.Name).ToArray());
    }
}

