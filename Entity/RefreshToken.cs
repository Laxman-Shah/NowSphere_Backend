namespace smartApi.Entity
{
    public class RefreshToken
    {
        public long RefreshTokenId { get; set; }

        public long UserId { get; set; }

        // Foreign key to UserSession.
        //
        // It is nullable temporarily because refresh_tokens may already
        // contain old records created before session management existed.
        public Guid? UserSessionId { get; set; }

        public string TokenHash { get; set; } = null!;

        public string TokenFamilyId { get; set; } = null!;

        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAt { get; set; }

        public DateTime? RevokedAt { get; set; }

        public string? RevokedReason { get; set; }

        public long? ReplacedByTokenId { get; set; }

        public string? CreatedByIp { get; set; }

        public string? RevokedByIp { get; set; }

        public string? UserAgent { get; set; }

        public bool IsRevoked { get; set; } = false;


        // ============================================================
        // NAVIGATION PROPERTIES
        // ============================================================

        // Many refresh tokens belong to one user.
        public User User { get; set; } = null!;

        // Many refresh tokens can belong to one user session.
        public UserSession? UserSession { get; set; }

        // Self-reference navigation.
        public RefreshToken? ReplacedByToken { get; set; }

        public ICollection<RefreshToken> ReplacedTokens { get; set; }
            = new List<RefreshToken>();


        // ============================================================
        // HELPER PROPERTIES
        //
        // These are calculated properties and are not database columns.
        // ============================================================

        public bool IsExpired =>
            DateTime.UtcNow >= ExpiresAt;

        public bool IsActive =>
            !IsRevoked && !IsExpired;
    }
}