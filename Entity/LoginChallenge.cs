using smartApi.Enums;

namespace smartApi.Entity
{
    public class LoginChallenge
    {
        public Guid LoginChallengeId { get; set; }

        public long UserId { get; set; }

        public LoginChallengeStatus Status { get; set; }

        public DateTime ExpiresAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime? RevokedAt { get; set; }

        public string? RevokedReason { get; set; }

        public string? CreatedByIp { get; set; }

        public string? UserAgent { get; set; }

        public DateTime CreatedAt { get; set; }

        public int ResendCount { get; set; } = 0;

        public DateTime? LastOtpSentAt { get; set; }

        public Guid ConcurrencyToken { get; set; }

        public User User { get; set; } = null!;

        public ICollection<email_otp_tokens> OtpTokens
        { get; set; } =
            new List<email_otp_tokens>();


        public UserSession? UserSession { get; set; }

        public ICollection<LoginActivity> LoginActivities { get; set; }
            = new List<LoginActivity>();
    }
}