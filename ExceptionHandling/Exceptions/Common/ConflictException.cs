using smartApi.ExceptionHandling.Exceptions.Base;

namespace smartApi.ExceptionHandling.Exceptions.Common;

public class ConflictException : AppException
{
    public ConflictException(
        string message,
        string errorCode = "CONFLICT")
        : base(
            message,
            StatusCodes.Status409Conflict,
            errorCode)
    {
    }
}