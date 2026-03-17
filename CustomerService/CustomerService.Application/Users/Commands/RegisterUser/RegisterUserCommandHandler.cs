using CustomerService.Application.Common.Exceptions;
using CustomerService.Application.Users.Events;
using CustomerService.Application.Users.Interfaces;
using CustomerService.Domain.Entities;
using MediatR;

namespace CustomerService.Application.Users.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler
    : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ICustomerEventsPublisher _eventsPublisher;

    public RegisterUserCommandHandler(
        ICustomerRepository customerRepository,
        IRoleRepository roleRepository,
        ICustomerEventsPublisher eventsPublisher)
    {
        _customerRepository = customerRepository;
        _roleRepository = roleRepository;
        _eventsPublisher = eventsPublisher;
    }

    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existing = await _customerRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
        {
            throw new EmailAlreadyExistsException(request.Email);
        }

        var user = new User(request.Email, request.UserName);

        const string defaultRoleName = "Customer";
        var role = await _roleRepository.GetByNameAsync(defaultRoleName, cancellationToken);

        if (role is null)
        {
            role = new Role(defaultRoleName);
            await _roleRepository.AddAsync(role, cancellationToken);
        }

        user.AddRole(role);

        await _customerRepository.AddAsync(user, cancellationToken);
        await _customerRepository.SaveChangesAsync(cancellationToken);

        var evt = new CustomerRegisteredEvent
        {
            UserId = user.Id,
            Email = user.Email,
            UserName = user.UserName,
            Password = request.Password,
            Roles = new[] { defaultRoleName }
        };

        await _eventsPublisher.PublishCustomerRegisteredAsync(evt, cancellationToken);

        return user.Id;
    }
}

