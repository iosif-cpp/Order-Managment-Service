using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CatalogService.Application.Products.Exceptions;

namespace CatalogService.API.Middleware;

public sealed class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            var details = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            await WriteProblemDetails(
                context,
                HttpStatusCode.BadRequest,
                "Validation failed.",
                details);
        }
        catch (CatalogValidationException ex)
        {
            await WriteProblemDetails(
                context,
                HttpStatusCode.BadRequest,
                ex.Message,
                null);
        }
        catch (DbUpdateException)
        {
            await WriteProblemDetails(
                context,
                HttpStatusCode.Conflict,
                "Database constraint violation.",
                null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while processing request.");
            await WriteProblemDetails(
                context,
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred.",
                null);
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

        context.Response.Clear();
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var payload = new
        {
            status = (int)statusCode,
            title = message,
            details
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}

