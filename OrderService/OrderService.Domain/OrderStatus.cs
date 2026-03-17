namespace OrderService.Domain.Entities;

public enum OrderStatus
{
    PendingPayment,
    Paid,
    Cancelled,
    Shipped
}

