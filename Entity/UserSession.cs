namespace smartApi.Entity
{
    public sealed class UserSession
    {
        public Guid UserSessionId { get; set; }

        public long UserId { get; set; }

        public long UserDeviceId { get; set; }

        public Guid? LoginChallengeId { get; set; }

        public string Status { get; set; } = "ACTIVE";

        public string AuthenticationLevel { get; set; } = "PASSWORD_OTP";

        public string? AuthenticationMethods { get; set; }

        public bool OtpVerified { get; set; }

        public DateTime? OtpVerifiedAt { get; set; }

        public string? LoginIpAddress { get; set; }

        public string? LoginUserAgent { get; set; }

        public string? LastIpAddress { get; set; }

        public string? LastUserAgent { get; set; }

        public DateTime LoginAt { get; set; }

        public DateTime LastActivityAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public DateTime? LoggedOutAt { get; set; }

        public string? LogoutReason { get; set; }

        public DateTime? RevokedAt { get; set; }

        public string? RevokedReason { get; set; }

        public string? RevokedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public Guid ConcurrencyToken { get; set; }

        public User User { get; set; } = null!;

        public UserDevice UserDevice { get; set; } = null!;

        public LoginChallenge? LoginChallenge { get; set; }

        public ICollection<RefreshToken> RefreshTokens { get; set; }
            = new List<RefreshToken>();

        public ICollection<LoginActivity> LoginActivities { get; set; }
            = new List<LoginActivity>();
    }
}
