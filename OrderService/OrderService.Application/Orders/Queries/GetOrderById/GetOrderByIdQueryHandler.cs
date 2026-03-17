using AutoMapper;
using MediatR;
using OrderService.Application.Orders.Interfaces;
using OrderService.Domain.Entities;

namespace OrderService.Application.Orders.Queries.GetOrderById;

public sealed class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderResponse?>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<OrderResponse?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return null;

        return _mapper.Map<OrderResponse>(order);
    }
}

