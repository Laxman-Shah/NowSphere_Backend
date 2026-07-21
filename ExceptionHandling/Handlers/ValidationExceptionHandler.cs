using Microsoft.AspNetCore.Diagnostics;
using smartApi.ExceptionHandling.Exceptions.Domain;
using smartApi.ExceptionHandling.Models;

namespace smartApi.ExceptionHandling.Handlers;

public class ValidationExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ValidationExceptionHandler> _logger;

    public ValidationExceptionHandler(
        ILogger<ValidationExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
        {
            return false;
        }

        _logger.LogWarning(
            exception,
            "Validation error occurred: {Message}",
            exception.Message);

        var problemDetails = new ErrorProblemDetails
        {
            Status = validationException.StatusCode,
            Type = "https://api.smartapi.com/errors/validation-failed",
            Title = "Validation Failed",
            Detail = validationException.Message,
            Instance = httpContext.Request.Path,
            ErrorCode = validationException.ErrorCode,
            TraceId = httpContext.TraceIdentifier,
            Timestamp = DateTime.UtcNow,
            Errors = validationException.Errors
        };

        httpContext.Response.ContentType = "application/problem+json";
        httpContext.Response.StatusCode = validationException.StatusCode;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }
}
