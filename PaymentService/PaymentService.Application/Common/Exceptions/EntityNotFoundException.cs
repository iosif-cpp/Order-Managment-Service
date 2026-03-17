namespace PaymentService.Application.Common.Exceptions;

public sealed class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string message) : base(message)
    {
    }
}

