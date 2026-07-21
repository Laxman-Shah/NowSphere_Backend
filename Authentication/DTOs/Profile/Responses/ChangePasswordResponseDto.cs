namespace smartApi.Authentication.DTOs.Profile.Responses;

public sealed class ChangePasswordResponseDto
{
    public string Message { get; init; } = "Password changed successfully. All other sessions have been revoked.";
}
