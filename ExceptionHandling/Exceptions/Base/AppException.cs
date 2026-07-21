using System;

namespace smartApi.ExceptionHandling.Exceptions.Base;

public abstract class AppException : Exception
{
    public int StatusCode { get; }

    public string ErrorCode { get; }

    protected AppException(
        string message,
        int statusCode,
        string errorCode,
        Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}
