using Microsoft.AspNetCore.Diagnostics;
using smartApi.ExceptionHandling.Exceptions.Security;
using smartApi.ExceptionHandling.Models;

namespace smartApi.ExceptionHandling.Handlers;

public class SecurityExceptionHandler : IExceptionHandler
{
    private readonly ILogger<SecurityExceptionHandler> _logger;

    public SecurityExceptionHandler(
        ILogger<SecurityExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not SecurityException securityException)
        {
            return false;
        }

        _logger.LogWarning(
            exception,
            "Security error occurred: {Message}",
            exception.Message);

        var problemDetails = new ErrorProblemDetails
        {
            Status = securityException.StatusCode,
            Type = $"https://api.smartapi.com/errors/{securityException.ErrorCode.ToLowerInvariant()}",
            Title = GetTitle(securityException.StatusCode),
            Detail = securityException.Message,
            Instance = httpContext.Request.Path,
            ErrorCode = securityException.ErrorCode,
            TraceId = httpContext.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        };

        httpContext.Response.ContentType = "application/problem+json";
        httpContext.Response.StatusCode = securityException.StatusCode;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }

    private static string GetTitle(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status401Unauthorized => "Unauthorized",
            StatusCodes.Status403Forbidden => "Forbidden",
            _ => "Security Error"
        };
    }
}