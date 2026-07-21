
namespace smartApi.ExceptionHandling.Exceptions.Infrastructure;

public class ExternalServiceException : InfrastructureException
{
    public ExternalServiceException(
        string message = "An external service error occurred.",
        string errorCode = "EXTERNAL_SERVICE_ERROR",
        Exception? innerException = null)
        : base(
            message,
            StatusCodes.Status503ServiceUnavailable,
            errorCode,
            innerException)
    {
    }
}
