using CustomerService.Application.Users.Events;

namespace CustomerService.Application.Users.Interfaces;

public interface ICustomerEventsPublisher
{
    Task PublishCustomerRegisteredAsync(CustomerRegisteredEvent evt, CancellationToken ct = default);
}

