using smartApi.ExceptionHandling.Exceptions.Base;

namespace smartApi.ExceptionHandling.Exceptions.Common;

public class NotFoundException : AppException
{
    public NotFoundException(
        string message,
        string errorCode = "NOT_FOUND")
        : base(
            message,
            StatusCodes.Status404NotFound,
            errorCode)
    {
    }
}