namespace smartApi.Authentication.DTOs.Profile.Responses;

public sealed class UpdateProfileResponseDto
{
    public long UserId { get; init; }
    public string Username { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string? PhoneNumber { get; init; }
    public string? FullName { get; init; }
    public DateTime UpdatedAt { get; init; }
    public string Message { get; init; } = "Profile updated successfully.";
}
