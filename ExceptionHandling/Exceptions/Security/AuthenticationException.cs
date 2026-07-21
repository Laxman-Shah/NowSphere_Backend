
namespace smartApi.ExceptionHandling.Exceptions.Security;

public class AuthenticationException : SecurityException
{
    public AuthenticationException(
        string message = "Authentication failed.",
        string errorCode = "AUTHENTICATION_FAILED",
        Exception? innerException = null)
        : base(
            message,
            StatusCodes.Status401Unauthorized,
            errorCode,
            innerException)
    {
    }
}
