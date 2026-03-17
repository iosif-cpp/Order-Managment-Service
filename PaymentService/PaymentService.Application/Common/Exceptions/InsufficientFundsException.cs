namespace PaymentService.Application.Common.Exceptions;

public sealed class InsufficientFundsException : Exception
{
    public InsufficientFundsException(Guid customerId, decimal requested, decimal available)
        : base($"Insufficient funds for customer '{customerId}'. Requested {requested}, available {available}.")
    {
    }
}

