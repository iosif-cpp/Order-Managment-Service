using AutoMapper;
using FluentAssertions;
using Moq;
using OrderService.Application.Catalog.Interfaces;
using OrderService.Application.Catalog.Models;
using OrderService.Application.Orders.Commands.CreateOrder;
using OrderService.Application.Orders.Exceptions;
using OrderService.Application.Orders.Interfaces;

namespace OrderService.Tests;

public sealed class CreateOrderCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenNoItems_ThrowsOrderValidationException()
    {
        var orders = new Mock<IOrderRepository>(MockBehavior.Strict);
        var mapper = new Mock<IMapper>(MockBehavior.Strict);
        var catalog = new Mock<ICatalogProductsReadRepository>(MockBehavior.Strict);
        var eventsPublisher = new Mock<IOrderEventsPublisher>(MockBehavior.Strict);

        var handler = new CreateOrderCommandHandler(orders.Object, mapper.Object, catalog.Object, eventsPublisher.Object);

        var act = async () =>
            await handler.Handle(new CreateOrderCommand(Guid.NewGuid(), Array.Empty<CreateOrderItemRequest>()), CancellationToken.None);

        await act.Should().ThrowAsync<OrderValidationException>();
    }

    [Fact]
    public async Task Handle_WhenProductNotFoundInSnapshot_ThrowsOrderValidationException()
    {
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var orders = new Mock<IOrderRepository>(MockBehavior.Strict);
        var mapper = new Mock<IMapper>(MockBehavior.Strict);
        var eventsPublisher = new Mock<IOrderEventsPublisher>(MockBehavior.Strict);

        var catalog = new Mock<ICatalogProductsReadRepository>(MockBehavior.Strict);
        catalog.Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, CatalogProductResponse>());

        var handler = new CreateOrderCommandHandler(orders.Object, mapper.Object, catalog.Object, eventsPublisher.Object);

        var act = async () =>
            await handler.Handle(
                new CreateOrderCommand(customerId, new[] { new CreateOrderItemRequest(productId, 1) }),
                CancellationToken.None);

        await act.Should().ThrowAsync<OrderValidationException>();
    }

    [Fact]
    public async Task Handle_WhenProductInactive_ThrowsOrderValidationException()
    {
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var snapshot = new CatalogProductResponse(productId, "p", 10m, false, 10, DateTime.UtcNow);

        var orders = new Mock<IOrderRepository>(MockBehavior.Strict);
        var mapper = new Mock<IMapper>(MockBehavior.Strict);
        var eventsPublisher = new Mock<IOrderEventsPublisher>(MockBehavior.Strict);

        var catalog = new Mock<ICatalogProductsReadRepository>(MockBehavior.Strict);
        catalog.Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, CatalogProductResponse> { [productId] = snapshot });

        var handler = new CreateOrderCommandHandler(orders.Object, mapper.Object, catalog.Object, eventsPublisher.Object);

        var act = async () =>
            await handler.Handle(
                new CreateOrderCommand(customerId, new[] { new CreateOrderItemRequest(productId, 1) }),
                CancellationToken.None);

        await act.Should().ThrowAsync<OrderValidationException>()
            .WithMessage("*not active*");
    }

    [Fact]
    public async Task Handle_WhenNotEnoughStock_ThrowsOrderValidationException()
    {
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var snapshot = new CatalogProductResponse(productId, "p", 10m, true, 1, DateTime.UtcNow);

        var orders = new Mock<IOrderRepository>(MockBehavior.Strict);
        var mapper = new Mock<IMapper>(MockBehavior.Strict);
        var eventsPublisher = new Mock<IOrderEventsPublisher>(MockBehavior.Strict);

        var catalog = new Mock<ICatalogProductsReadRepository>(MockBehavior.Strict);
        catalog.Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, CatalogProductResponse> { [productId] = snapshot });

        var handler = new CreateOrderCommandHandler(orders.Object, mapper.Object, catalog.Object, eventsPublisher.Object);

        var act = async () =>
            await handler.Handle(
                new CreateOrderCommand(customerId, new[] { new CreateOrderItemRequest(productId, 2) }),
                CancellationToken.None);

        await act.Should().ThrowAsync<OrderValidationException>()
            .WithMessage("*Not enough stock*");
    }

    [Fact]
    public async Task Handle_WhenValid_AddsOrderAndPublishesPaymentRequested()
    {
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var snapshot = new CatalogProductResponse(productId, "p", 10m, true, 10, DateTime.UtcNow);

        var orders = new Mock<IOrderRepository>(MockBehavior.Strict);
        orders.Setup(x => x.AddAsync(It.IsAny<OrderService.Domain.Entities.Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var mapper = new Mock<IMapper>(MockBehavior.Strict);
        mapper.Setup(x => x.Map<OrderService.Application.Orders.OrderResponse>(It.IsAny<object>()))
            .Returns(new OrderService.Application.Orders.OrderResponse(Guid.NewGuid(), customerId, 10m, "PendingPayment", DateTime.UtcNow));

        var eventsPublisher = new Mock<IOrderEventsPublisher>(MockBehavior.Strict);
        eventsPublisher.Setup(x => x.PublishOrderPaymentRequestedAsync(It.IsAny<OrderService.Application.Orders.Events.OrderPaymentRequestedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var catalog = new Mock<ICatalogProductsReadRepository>(MockBehavior.Strict);
        catalog.Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, CatalogProductResponse> { [productId] = snapshot });

        var handler = new CreateOrderCommandHandler(orders.Object, mapper.Object, catalog.Object, eventsPublisher.Object);

        _ = await handler.Handle(
            new CreateOrderCommand(customerId, new[] { new CreateOrderItemRequest(productId, 1) }),
            CancellationToken.None);

        orders.Verify(x => x.AddAsync(It.IsAny<OrderService.Domain.Entities.Order>(), It.IsAny<CancellationToken>()), Times.Once);
        eventsPublisher.Verify(x => x.PublishOrderPaymentRequestedAsync(It.IsAny<OrderService.Application.Orders.Events.OrderPaymentRequestedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}

