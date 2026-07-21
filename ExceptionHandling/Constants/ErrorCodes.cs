
namespace smartApi.ExceptionHandling.Constants;

public static class ErrorCodes
{
    // General errors
    public const string InternalServerError = "INTERNAL_SERVER_ERROR";
    public const string BadRequest = "BAD_REQUEST";
    public const string NotFound = "NOT_FOUND";
    public const string Conflict = "CONFLICT";
    public const string ValidationFailed = "VALIDATION_FAILED";

    // User/Auth domain errors
    public const string UserEmailAlreadyExists = "USER_EMAIL_ALREADY_EXISTS";
    public const string UsernameAlreadyExists = "USERNAME_ALREADY_EXISTS";
    public const string UserNotFound = "USER_NOT_FOUND";
    public const string UserAccountLocked = "USER_ACCOUNT_LOCKED";
    public const string UserAccountInactive = "USER_ACCOUNT_INACTIVE";

    // Authentication errors
    public const string AuthInvalidCredentials = "AUTH_INVALID_CREDENTIALS";
    public const string AuthTokenMissing = "AUTH_TOKEN_MISSING";
    public const string AuthTokenInvalid = "AUTH_TOKEN_INVALID";
    public const string AuthTokenExpired = "AUTH_TOKEN_EXPIRED";
    public const string AuthenticationFailed = "AUTHENTICATION_FAILED";

    // Authorization errors
    public const string AccessDenied = "ACCESS_DENIED";
    public const string AdminAccessRequired = "ADMIN_ACCESS_REQUIRED";
    public const string PermissionDenied = "PERMISSION_DENIED";

    // Role errors
    public const string RoleNotFound = "ROLE_NOT_FOUND";
    public const string RoleAlreadyAssigned = "ROLE_ALREADY_ASSIGNED";
    public const string RoleAssignmentFailed = "ROLE_ASSIGNMENT_FAILED";

    // Password errors
    public const string PasswordRequired = "PASSWORD_REQUIRED";
    public const string PasswordTooWeak = "PASSWORD_TOO_WEAK";
    public const string PasswordHashFailed = "PASSWORD_HASH_FAILED";
    public const string PasswordVerificationFailed = "PASSWORD_VERIFICATION_FAILED";

    // Database / infrastructure errors
    public const string DatabaseError = "DATABASE_ERROR";
    public const string UserSaveFailed = "USER_SAVE_FAILED";
    public const string CredentialSaveFailed = "CREDENTIAL_SAVE_FAILED";
    public const string RoleSaveFailed = "ROLE_SAVE_FAILED";

    // External service errors
    public const string ExternalServiceError = "EXTERNAL_SERVICE_ERROR";
    public const string EmailServiceUnavailable = "EMAIL_SERVICE_UNAVAILABLE";
    public const string SmsServiceUnavailable = "SMS_SERVICE_UNAVAILABLE";
}
