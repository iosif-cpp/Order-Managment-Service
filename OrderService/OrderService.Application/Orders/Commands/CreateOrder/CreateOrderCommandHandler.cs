using AutoMapper;
using MediatR;
using OrderService.Application.Catalog.Interfaces;
using OrderService.Application.Orders.Events;
using OrderService.Application.Orders.Exceptions;
using OrderService.Application.Orders.Interfaces;
using OrderService.Domain.Entities;

namespace OrderService.Application.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;
    private readonly ICatalogProductsReadRepository _catalogProducts;
    private readonly IOrderEventsPublisher _eventsPublisher;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IMapper mapper,
        ICatalogProductsReadRepository catalogProducts,
        IOrderEventsPublisher eventsPublisher)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
        _catalogProducts = catalogProducts;
        _eventsPublisher = eventsPublisher;
    }

    public async Task<OrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.Items.Count == 0)
            throw new OrderValidationException("Order must contain at least one item.");

        var order = new Order(request.CustomerId);

        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToArray();
        var products = await _catalogProducts.GetByIdsAsync(productIds, cancellationToken);

        if (products.Count != productIds.Length)
            throw new OrderValidationException("One or more products were not found in catalog snapshot.");

        var paymentItems = new List<OrderPaymentRequestedItem>(request.Items.Count);

        foreach (var i in request.Items)
        {
            var p = products[i.ProductId];
            if (!p.IsActive)
                throw new OrderValidationException($"Product '{p.ProductId}' is not active.");
            if (p.Stock < i.Quantity)
                throw new OrderValidationException($"Not enough stock for product '{p.ProductId}'.");

            order.AddItem(p.ProductId, p.Name, p.Price, i.Quantity);
            paymentItems.Add(new OrderPaymentRequestedItem
            {
                ProductId = p.ProductId,
                Quantity = i.Quantity,
                UnitPrice = p.Price
            });
        }

        await _orderRepository.AddAsync(order, cancellationToken);

        await _eventsPublisher.PublishOrderPaymentRequestedAsync(new OrderPaymentRequestedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            CreatedAt = order.CreatedAt,
            TotalAmount = order.GetTotalPrice(),
            Items = paymentItems
        }, cancellationToken);

        return _mapper.Map<OrderResponse>(order);
    }
}

