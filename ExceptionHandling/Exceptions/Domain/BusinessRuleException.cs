namespace smartApi.ExceptionHandling.Exceptions.Domain;

public class BusinessRuleException : DomainException
{
    public BusinessRuleException(
        string message,
        string errorCode = "BUSINESS_RULE_VIOLATION",
        int statusCode = StatusCodes.Status400BadRequest,
        Exception? innerException = null)
        : base(message, statusCode, errorCode, innerException)
    {
    }
}
