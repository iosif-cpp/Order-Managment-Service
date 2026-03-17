using OrderService.Application.Orders.Commands.CreateOrder;

namespace OrderService.API.Controllers;

public sealed class CreateOrderRequest
{
    public IReadOnlyCollection<CreateOrderItemRequest> Items { get; init; } = Array.Empty<CreateOrderItemRequest>();
}

