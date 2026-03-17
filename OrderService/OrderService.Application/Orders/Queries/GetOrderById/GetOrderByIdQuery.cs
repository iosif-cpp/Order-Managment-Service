using MediatR;

namespace OrderService.Application.Orders.Queries.GetOrderById;

public sealed record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderResponse?>;

