using Microsoft.AspNetCore.Diagnostics;
using smartApi.ExceptionHandling.Constants;
using smartApi.ExceptionHandling.Exceptions.Infrastructure;
using smartApi.ExceptionHandling.Models;

namespace smartApi.ExceptionHandling.Handlers;

public class InfrastructureExceptionHandler : IExceptionHandler
{
    private readonly ILogger<InfrastructureExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public InfrastructureExceptionHandler(
        ILogger<InfrastructureExceptionHandler> logger,
        IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not InfrastructureException infrastructureException)
        {
            return false;
        }

        _logger.LogError(
            exception,
            "Infrastructure error occurred: {Message}",
            exception.Message);

        var problemDetails = new ErrorProblemDetails
        {
            Status = infrastructureException.StatusCode,
            Type = $"https://api.smartapi.com/errors/{infrastructureException.ErrorCode.ToLowerInvariant()}",
            Title = GetTitle(infrastructureException.StatusCode),
            Detail = _environment.IsDevelopment()
                ? infrastructureException.Message
                : ErrorMessages.ExternalServiceError,
            Instance = httpContext.Request.Path,
            ErrorCode = infrastructureException.ErrorCode,
            TraceId = httpContext.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        };

        httpContext.Response.ContentType = "application/problem+json";
        httpContext.Response.StatusCode = infrastructureException.StatusCode;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }

    private static string GetTitle(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status500InternalServerError => "Internal Server Error",
            StatusCodes.Status503ServiceUnavailable => "Service Unavailable",
            _ => "Infrastructure Error"
        };
    }
}