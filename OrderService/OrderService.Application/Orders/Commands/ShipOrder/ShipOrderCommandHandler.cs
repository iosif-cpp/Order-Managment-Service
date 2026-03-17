using AutoMapper;
using MediatR;
using OrderService.Application.Orders.Exceptions;
using OrderService.Application.Orders.Interfaces;
using OrderService.Domain.Entities;

namespace OrderService.Application.Orders.Commands.ShipOrder;

public sealed class ShipOrderCommandHandler : IRequestHandler<ShipOrderCommand, OrderResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public ShipOrderCommandHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<OrderResponse> Handle(ShipOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

        if (order is null || order.CustomerId != request.CustomerId)
            throw new OrderNotFoundException(request.OrderId);

        if (order.Status != OrderStatus.Paid)
            throw new InvalidOrderStateException("Only paid orders can be shipped.");

        order.Ship();

        await _orderRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<OrderResponse>(order);
    }
}

