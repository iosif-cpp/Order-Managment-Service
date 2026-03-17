using OrderService.Application.Orders.Events;

namespace OrderService.Application.Orders.Interfaces;

public interface IOrderEventsPublisher
{
    Task PublishOrderPaymentRequestedAsync(OrderPaymentRequestedEvent evt, CancellationToken cancellationToken = default);
}

