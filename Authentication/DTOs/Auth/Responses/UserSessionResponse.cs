namespace smartApi.Authentication.DTOs.Auth.Responses
{


    public sealed class UserSessionResponse
    {
        public Guid SessionId { get; init; }
        public string DeviceName { get; init; } = "Unknown Device";
        public string DeviceType { get; init; } = "UNKNOWN";
        public string? OperatingSystem { get; init; }
        public string? BrowserName { get; init; }
        public string? IpAddress { get; init; }
        public string Status { get; init; } = null!;
        public DateTime LoginAt { get; init; }
        public DateTime LastActivityAt { get; init; }
        public DateTime ExpiresAt { get; init; }
        public bool IsCurrentSession { get; init; }
        public bool IsTrustedDevice { get; init; }
    }
}
