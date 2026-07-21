using smartApi.ExceptionHandling.Exceptions.Base;

namespace smartApi.ExceptionHandling.Exceptions.Infrastructure;

public abstract class InfrastructureException : AppException
{
    protected InfrastructureException(
        string message,
        int statusCode,
        string errorCode,
        Exception? innerException = null)
        : base(message, statusCode, errorCode, innerException)
    {
    }
}

