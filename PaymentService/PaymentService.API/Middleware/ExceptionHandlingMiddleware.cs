using System.Net;
using System.Text.Json;
using PaymentService.Application.Common.Exceptions;
using FluentValidation;

namespace PaymentService.API.Middleware;

public sealed class ExceptionHandlingMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            await WriteProblemDetails(
                context,
                HttpStatusCode.BadRequest,
                "Validation failed.",
                errors);
        }
        catch (EntityNotFoundException ex)
        {
            await WriteProblemDetails(context, HttpStatusCode.NotFound, ex.Message, null);
        }
        catch (InsufficientFundsException ex)
        {
            await WriteProblemDetails(context, HttpStatusCode.Conflict, ex.Message, null);
        }
        catch (Exception)
        {
            await WriteProblemDetails(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.", null);
        }
    }

    private static async Task WriteProblemDetails(
        HttpContext context,
        HttpStatusCode statusCode,
        string message,
        object? details)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var payload = new
        {
            status = (int)statusCode,
            error = message,
            details
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}

