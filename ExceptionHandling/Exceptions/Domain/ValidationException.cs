namespace smartApi.ExceptionHandling.Exceptions.Domain;

public class ValidationException : DomainException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(
        IReadOnlyDictionary<string, string[]> errors,
        string message = "Validation failed.",
        string errorCode = "VALIDATION_FAILED")
        : base(
            message,
            StatusCodes.Status400BadRequest,
            errorCode)
    {
        Errors = errors;
    }
}