namespace OrderService.Application.Orders;

public sealed record OrderResponse(
    Guid Id,
    Guid CustomerId,
    decimal TotalPrice,
    string Status,
    DateTime CreatedAt);

