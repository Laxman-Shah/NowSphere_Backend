namespace smartApi.ExceptionHandling.Exceptions.Security;

public class AuthorizationException : SecurityException
{
    public AuthorizationException(
        string message = "You are not allowed to access this resource.",
        string errorCode = "ACCESS_DENIED",
        Exception? innerException = null)
        : base(
            message,
            StatusCodes.Status403Forbidden,
            errorCode,
            innerException)
    {
    }
}
