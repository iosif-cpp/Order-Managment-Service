using MediatR;

namespace OrderService.Application.Orders.Queries.GetOrdersForCustomer;

public sealed record GetOrdersForCustomerQuery(Guid CustomerId) : IRequest<IReadOnlyCollection<OrderResponse>>;

