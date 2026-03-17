namespace OrderService.Application.Orders.Exceptions;

public sealed class OrderNotFoundException : Exception
{
    public OrderNotFoundException(Guid orderId)
        : base($"Order with id '{orderId}' was not found.")
    {
    }
}

