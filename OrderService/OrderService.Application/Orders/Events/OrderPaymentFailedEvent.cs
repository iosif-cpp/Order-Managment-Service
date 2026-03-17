namespace OrderService.Application.Orders.Events;

public sealed class OrderPaymentFailedEvent
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public decimal Amount { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTime FailedAt { get; init; }
}

