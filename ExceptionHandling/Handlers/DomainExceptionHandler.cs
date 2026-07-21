
using Microsoft.AspNetCore.Diagnostics;
using smartApi.ExceptionHandling.Exceptions.Domain;
using smartApi.ExceptionHandling.Models;

namespace smartApi.ExceptionHandling.Handlers;

public class DomainExceptionHandler : IExceptionHandler
{
    private readonly ILogger<DomainExceptionHandler> _logger;

    public DomainExceptionHandler(
        ILogger<DomainExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not DomainException domainException)
        {
            return false;
        }

        if (exception is ValidationException)
        {
            return false;
        }

        _logger.LogWarning(
            exception,
            "Domain error occurred: {Message}",
            exception.Message);

        var problemDetails = new ErrorProblemDetails
        {
            Status = domainException.StatusCode,
            Type = $"https://api.smartapi.com/errors/{domainException.ErrorCode.ToLowerInvariant()}",
            Title = GetTitle(domainException.StatusCode),
            Detail = domainException.Message,
            Instance = httpContext.Request.Path,
            ErrorCode = domainException.ErrorCode,
            TraceId = httpContext.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        };

        httpContext.Response.ContentType = "application/problem+json";
        httpContext.Response.StatusCode = domainException.StatusCode;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }

    private static string GetTitle(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad Request",
            StatusCodes.Status404NotFound => "Not Found",
            StatusCodes.Status409Conflict => "Conflict",
            _ => "Domain Error"
        };
    }
}
