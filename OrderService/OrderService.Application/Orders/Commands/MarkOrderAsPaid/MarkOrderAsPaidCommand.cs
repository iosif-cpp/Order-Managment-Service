using MediatR;

namespace OrderService.Application.Orders.Commands.MarkOrderAsPaid;

public sealed record MarkOrderAsPaidCommand(Guid OrderId, Guid CustomerId) : IRequest<OrderResponse>;

