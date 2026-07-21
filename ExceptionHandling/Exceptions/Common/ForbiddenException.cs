using smartApi.ExceptionHandling.Exceptions.Base;

namespace smartApi.ExceptionHandling.Exceptions.Common;

public class ForbiddenException : AppException
{
    public ForbiddenException(
        string message,
        string errorCode = "FORBIDDEN")
        : base(
            message,
            StatusCodes.Status403Forbidden,
            errorCode)
    {
    }
}