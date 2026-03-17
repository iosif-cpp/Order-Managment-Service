namespace CatalogService.Application.Orders.Events;

public sealed class OrderPaidEvent
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public decimal Amount { get; init; }
    public DateTime PaidAt { get; init; }
    public IReadOnlyCollection<OrderPaidItem> Items { get; init; } = Array.Empty<OrderPaidItem>();
}

public sealed class OrderPaidItem
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
}

