using CustomerService.Application.Users.Interfaces;
using CustomerService.Domain.Entities;
using MediatR;

namespace CustomerService.Application.Users.Commands.AssignRole;

public sealed class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IRoleRepository _roleRepository;

    public AssignRoleCommandHandler(
        ICustomerRepository customerRepository,
        IRoleRepository roleRepository)
    {
        _customerRepository = customerRepository;
        _roleRepository = roleRepository;
    }

    public async Task<Unit> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _customerRepository.GetByIdAsync(request.UserId, cancellationToken)
                   ?? throw new KeyNotFoundException($"User '{request.UserId}' not found.");

        var role = await _roleRepository.GetByNameAsync(request.RoleName, cancellationToken);
        if (role is null)
        {
            role = new Role(request.RoleName);
            await _roleRepository.AddAsync(role, cancellationToken);
        }

        user.AddRole(role);
        await _customerRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

