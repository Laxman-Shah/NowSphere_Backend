namespace smartApi.Entity
{
    public class email_otp_tokens
    {
        public long EmailOtpTokenId { get; set; }

        public long UserId { get; set; }

        public string SentToEmail { get; set; } = null!;

        public string TokenHash { get; set; } = null!;

        public string Purpose { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }

        public DateTime? UsedAt { get; set; }

        public DateTime? RevokedAt { get; set; }

        public string? RevokedReason { get; set; }

        public int AttemptCount { get; set; } = 0;

        public int MaxAttempts { get; set; } = 5;

        public int ResendCount { get; set; } = 0;

        public DateTime? LastSentAt { get; set; }

        public string? CreatedByIp { get; set; }

        public string? UserAgent { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        public bool IsUsed => UsedAt != null;

        public bool IsRevoked => RevokedAt != null;


        public Guid? LoginChallengeId { get; set; }

        public LoginChallenge? LoginChallenge { get; set; }

        public bool IsActive => !IsExpired && !IsUsed && !IsRevoked;
    }
}