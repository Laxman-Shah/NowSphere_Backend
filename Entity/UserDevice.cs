namespace smartApi.Entity
{
    
    public class UserDevice
    {
        public long UserDeviceId { get; set; }

        public long UserId { get; set; }

        public string DeviceFingerprintHash { get; set; } = null!;

        public string? DeviceName { get; set; }

        public string DeviceType { get; set; } = "UNKNOWN";

        public string? OperatingSystem { get; set; }

        public string? OperatingSystemVersion { get; set; }

        public string? BrowserName { get; set; }

        public string? BrowserVersion { get; set; }

        public string? LastIpAddress { get; set; }

        public string? LastUserAgent { get; set; }

        public DateTime FirstSeenAt { get; set; }

        public DateTime LastSeenAt { get; set; }

        public bool IsTrusted { get; set; }

        public DateTime? TrustedAt { get; set; }

        public DateTime? TrustExpiresAt { get; set; }

        public DateTime? TrustRevokedAt { get; set; }

        public string? TrustRevokedReason { get; set; }

        public bool IsActive { get; set; }

        public DateTime? DeactivatedAt { get; set; }

        public string? DeactivationReason { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public User User { get; set; } = null!;

        public ICollection<UserSession> UserSessions { get; set; }
            = new List<UserSession>();

        public ICollection<LoginActivity> LoginActivities { get; set; }
            = new List<LoginActivity>();
    }
}
