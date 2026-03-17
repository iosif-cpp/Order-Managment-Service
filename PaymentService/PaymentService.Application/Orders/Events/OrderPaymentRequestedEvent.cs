namespace PaymentService.Application.Orders.Events;

public sealed class OrderPaymentRequestedEvent
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public decimal TotalAmount { get; init; }
    public DateTime CreatedAt { get; init; }
    public IReadOnlyCollection<OrderPaymentRequestedItem> Items { get; init; } = Array.Empty<OrderPaymentRequestedItem>();
}

public sealed class OrderPaymentRequestedItem
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}

