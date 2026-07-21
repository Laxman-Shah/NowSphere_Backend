namespace smartApi.ExceptionHandling.Constants;

public static class ErrorMessages
{
    // General errors
    public const string InternalServerError =
        "An unexpected error occurred. Please contact support.";

    public const string BadRequest =
        "The request is invalid.";

    public const string NotFound =
        "The requested resource was not found.";

    public const string Conflict =
        "The request conflicts with the current system state.";

    public const string ValidationFailed =
        "Validation failed.";

    // User/Auth domain messages
    public const string UserEmailAlreadyExists =
        "Email is already registered.";

    public const string UsernameAlreadyExists =
        "Username is already taken.";

    public const string UserNotFound =
        "User not found.";

    public const string UserAccountLocked =
        "User account is locked.";

    public const string UserAccountInactive =
        "User account is inactive.";

    // Authentication messages
    public const string AuthInvalidCredentials =
        "Invalid email or password.";

    public const string AuthTokenMissing =
        "Authentication token is missing.";

    public const string AuthTokenInvalid =
        "Authentication token is invalid.";

    public const string AuthTokenExpired =
        "Authentication token has expired.";

    public const string AuthenticationFailed =
        "Authentication failed.";

    // Authorization messages
    public const string AccessDenied =
        "You are not allowed to access this resource.";

    public const string AdminAccessRequired =
        "Only ADMIN can access this resource.";

    public const string PermissionDenied =
        "You do not have permission to perform this action.";

    // Role messages
    public const string RoleNotFound =
        "Role not found.";

    public const string RoleAlreadyAssigned =
        "Role is already assigned to this user.";

    public const string RoleAssignmentFailed =
        "Failed to assign role to user.";

    // Password messages
    public const string PasswordRequired =
        "Password is required.";

    public const string PasswordTooWeak =
        "Password does not meet security requirements.";

    public const string PasswordHashFailed =
        "Failed to hash password.";

    public const string PasswordVerificationFailed =
        "Failed to verify password.";

    // Database / infrastructure messages
    public const string DatabaseError =
        "A database error occurred.";

    public const string UserSaveFailed =
        "Unable to save user.";

    public const string CredentialSaveFailed =
        "Unable to save user credential.";

    public const string RoleSaveFailed =
        "Unable to save role information.";

    // External service messages
    public const string ExternalServiceError =
        "An external service error occurred.";

    public const string EmailServiceUnavailable =
        "Email service is currently unavailable.";

    public const string SmsServiceUnavailable =
        "SMS service is currently unavailable.";
}