
using smartApi.ExceptionHandling.Exceptions.Base;

namespace smartApi.ExceptionHandling.Exceptions.Security;

public abstract class SecurityException : AppException
{
    protected SecurityException(
        string message,
        int statusCode,
        string errorCode,
        Exception? innerException = null)
        : base(message, statusCode, errorCode, innerException)
    {
    }
}