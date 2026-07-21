using smartApi.ExceptionHandling.Exceptions.Base;

namespace smartApi.ExceptionHandling.Exceptions.Common;

public class UnauthorizedException : AppException
{
    public UnauthorizedException(
        string message,
        string errorCode = "UNAUTHORIZED")
        : base(
            message,
            StatusCodes.Status401Unauthorized,
            errorCode)
    {
    }
}