namespace OrderService.Application.Orders.Exceptions;

public sealed class OrderValidationException : Exception
{
    public OrderValidationException(string message)
        : base(message)
    {
    }
}

