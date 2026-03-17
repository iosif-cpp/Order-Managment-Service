using AutoMapper;
using MediatR;
using OrderService.Application.Orders.Interfaces;

namespace OrderService.Application.Orders.Queries.GetOrdersForCustomer;

public sealed class GetOrdersForCustomerQueryHandler
    : IRequestHandler<GetOrdersForCustomerQuery, IReadOnlyCollection<OrderResponse>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetOrdersForCustomerQueryHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyCollection<OrderResponse>> Handle(
        GetOrdersForCustomerQuery request,
        CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetByCustomerAsync(request.CustomerId, cancellationToken);

        return orders
            .Select(o => _mapper.Map<OrderResponse>(o))
            .ToArray();
    }
}

