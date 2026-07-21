using smartApi.ExceptionHandling.Exceptions.Base;

namespace smartApi.ExceptionHandling.Exceptions.Common;

public class BadRequestException : AppException
{
    public BadRequestException(
        string message,
        string errorCode = "BAD_REQUEST")
        : base(
            message,
            StatusCodes.Status400BadRequest,
            errorCode)
    {
    }
}
