namespace OrderService.Application.Orders.Events;

public sealed class OrderCreatedEvent
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public DateTime CreatedAt { get; init; }
    public IReadOnlyCollection<OrderCreatedItem> Items { get; init; } = Array.Empty<OrderCreatedItem>();
}

public sealed class OrderCreatedItem
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
}

