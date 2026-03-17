using AutoMapper;
using MediatR;
using OrderService.Application.Orders.Exceptions;
using OrderService.Application.Orders.Interfaces;
using OrderService.Domain.Entities;

namespace OrderService.Application.Orders.Commands.CancelOrder;

public sealed class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, OrderResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public CancelOrderCommandHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<OrderResponse> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

        if (order is null || order.CustomerId != request.CustomerId)
            throw new OrderNotFoundException(request.OrderId);

        if (order.Status == OrderStatus.Shipped)
            throw new InvalidOrderStateException("Shipped orders cannot be cancelled.");

        order.Cancel();

        await _orderRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<OrderResponse>(order);
    }
}

