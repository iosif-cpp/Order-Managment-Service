namespace CustomerService.Application.Common.Exceptions;

public sealed class EmailAlreadyExistsException : Exception
{
    public EmailAlreadyExistsException(string email)
        : base($"User with email '{email}' already exists.")
    {
    }
}

