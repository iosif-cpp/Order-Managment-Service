using MediatR;

namespace OrderService.Application.Orders.Commands.ShipOrder;

public sealed record ShipOrderCommand(Guid OrderId, Guid CustomerId) : IRequest<OrderResponse>;

