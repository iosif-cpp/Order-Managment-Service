using MediatR;

namespace OrderService.Application.Orders.Commands.CancelOrder;

public sealed record CancelOrderCommand(Guid OrderId, Guid CustomerId) : IRequest<OrderResponse>;

