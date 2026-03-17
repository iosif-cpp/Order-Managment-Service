using AutoMapper;
using MediatR;
using OrderService.Application.Orders.Exceptions;
using OrderService.Application.Orders.Interfaces;
using OrderService.Domain.Entities;

namespace OrderService.Application.Orders.Commands.MarkOrderAsPaid;

public sealed class MarkOrderAsPaidCommandHandler : IRequestHandler<MarkOrderAsPaidCommand, OrderResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public MarkOrderAsPaidCommandHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<OrderResponse> Handle(MarkOrderAsPaidCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

        if (order is null || order.CustomerId != request.CustomerId)
            throw new OrderNotFoundException(request.OrderId);

        if (order.Status != OrderStatus.PendingPayment)
            throw new InvalidOrderStateException("Only orders pending payment can be paid.");

        order.MarkAsPaid();

        await _orderRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<OrderResponse>(order);
    }
}

