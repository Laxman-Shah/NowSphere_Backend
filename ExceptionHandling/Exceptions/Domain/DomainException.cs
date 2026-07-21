
using smartApi.ExceptionHandling.Exceptions.Base;

namespace smartApi.ExceptionHandling.Exceptions.Domain;

public abstract class DomainException : AppException
{
    protected DomainException(
        string message,
        int statusCode,
        string errorCode,
        Exception? innerException = null)
        : base(message, statusCode, errorCode, innerException)
    {
    }
}