
namespace smartApi.ExceptionHandling.Exceptions.Infrastructure;

public class DatabaseException : InfrastructureException
{
    public DatabaseException(
        string message = "A database error occurred.",
        string errorCode = "DATABASE_ERROR",
        Exception? innerException = null)
        : base(
            message,
            StatusCodes.Status500InternalServerError,
            errorCode,
            innerException)
    {
    }
}