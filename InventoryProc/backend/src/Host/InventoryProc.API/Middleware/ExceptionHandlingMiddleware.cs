using InventoryProc.SharedKernel.Exceptions;
using InventoryProc.SharedKernel.Common;
using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace InventoryProc.API.Middleware;

/// <summary>
/// Global exception handling middleware
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, result) = exception switch
        {
            ValidationException validationEx => (
                (int)HttpStatusCode.BadRequest,
                Result.Fail(validationEx.Message, validationEx.Errors.Select(e => $"{e.Field}: {e.Message}").ToList())
            ),

            NotFoundException notFoundEx => (
                (int)HttpStatusCode.NotFound,
                Result.Fail(notFoundEx.Message, new List<string> { notFoundEx.Message })
            ),

            BusinessRuleException businessEx => (
                (int)HttpStatusCode.UnprocessableEntity,
                Result.Fail(businessEx.Message, new List<string> { businessEx.Message })
            ),

            UnauthorizedAccessException unauthorizedEx => (
                (int)HttpStatusCode.Unauthorized,
                Result.Fail("Unauthorized access", new List<string> { unauthorizedEx.Message })
            ),

            DbUpdateException dbEx when dbEx.InnerException?.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true ||
                                        dbEx.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true => (
                (int)HttpStatusCode.Conflict,
                Result.Fail("Duplicate entry. The record already exists.", new List<string> { "A record with this information already exists" })
            ),

            _ => (
                (int)HttpStatusCode.InternalServerError,
                Result.Fail("An unexpected error occurred", new List<string> { "Internal server error" })
            )
        };

        // Log the exception
        if (statusCode == (int)HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception occurred: {Message}", exception.Message);
        }

        context.Response.StatusCode = statusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(result, jsonOptions));
    }
}

/// <summary>
/// Extension method to register middleware
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
