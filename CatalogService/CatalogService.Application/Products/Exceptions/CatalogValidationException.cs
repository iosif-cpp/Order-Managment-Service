namespace CatalogService.Application.Products.Exceptions;

public sealed class CatalogValidationException : Exception
{
    public CatalogValidationException(string message)
        : base(message)
    {
    }
}

