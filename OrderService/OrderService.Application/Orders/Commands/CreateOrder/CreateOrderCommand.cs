using MediatR;

namespace OrderService.Application.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand(
    Guid CustomerId,
    IReadOnlyCollection<CreateOrderItemRequest> Items
) : IRequest<OrderResponse>;
