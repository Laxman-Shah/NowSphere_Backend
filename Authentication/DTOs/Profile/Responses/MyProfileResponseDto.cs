using smartApi.Enums;

namespace smartApi.Authentication.DTOs.Profile.Responses;

public sealed class MyProfileResponseDto
{
    public long UserId { get; init; }
    public string Username { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string? PhoneNumber { get; init; }
    public string? FullName { get; init; }
    public AccountStatus AccountStatus { get; init; }
    public bool EmailVerified { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public List<string> Roles { get; init; } = new();
    public int ActiveSessionCount { get; init; }
    public ProfileCurrentSessionResponseDto? CurrentSession { get; init; }
}
