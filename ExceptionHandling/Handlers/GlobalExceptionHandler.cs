
using Microsoft.AspNetCore.Diagnostics;
using smartApi.ExceptionHandling.Constants;
using smartApi.ExceptionHandling.Models;

namespace smartApi.ExceptionHandling.Handlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
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
        _logger.LogError(
            exception,
            "Unhandled unexpected exception occurred: {Message}",
            exception.Message);

        var problemDetails = new ErrorProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Type = "https://api.smartapi.com/errors/internal-server-error",
            Title = "Internal Server Error",
            Detail = _environment.IsDevelopment()
                ? exception.Message
                : ErrorMessages.InternalServerError,
            Instance = httpContext.Request.Path,
            ErrorCode = ErrorCodes.InternalServerError,
            TraceId = httpContext.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        };

        httpContext.Response.ContentType = "application/problem+json";
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }
}