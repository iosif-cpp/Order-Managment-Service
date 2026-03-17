namespace OrderService.Application.Orders.Commands.CreateOrder;

public sealed record CreateOrderItemRequest(
    Guid ProductId,
    int Quantity);

