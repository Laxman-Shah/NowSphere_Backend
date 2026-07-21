namespace smartApi.Authentication.DTOs.Profile.Responses;

public sealed class ProfileCurrentSessionResponseDto
{
    public Guid SessionId { get; init; }
    public string DeviceName { get; init; } = "Unknown Device";
    public string DeviceType { get; init; } = "UNKNOWN";
    public string? OperatingSystem { get; init; }
    public string? BrowserName { get; init; }
    public string? IpAddress { get; init; }
    public DateTime LoginAt { get; init; }
    public DateTime LastActivityAt { get; init; }
    public DateTime ExpiresAt { get; init; }
}
