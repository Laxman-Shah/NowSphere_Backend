using smartApi.Enums;

namespace smartApi.Entity
{
    public class User
    {
        public long UserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string? FullName { get; set; }

        public AccountStatus AccountStatus { get; set; } = AccountStatus.Active;

        public bool EmailVerified { get; set; } = false;

        public DateTime? LastLoginAt { get; set; }

        public int FailedLoginCount { get; set; } = 0;

        public DateTime? LockedUntil { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }


        // One-to-one relation: One user has one credential
        public UserCredential? Credential { get; set; }


        public int SecurityVersion { get; set; } = 1;


        // One user can have many user_roles records
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();


        // For role assignment tracking
        public ICollection<UserRole> AssignedUserRoles { get; set; } = new List<UserRole>();

        public ICollection<UserRole> RevokedUserRoles { get; set; } = new List<UserRole>();


        public ICollection<LoginChallenge> LoginChallenges { get; set; } = new List<LoginChallenge>();

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();


        public ICollection<email_otp_tokens> EmailOtpTokens { get; set; } = new List<email_otp_tokens>();




        public ICollection<UserDevice> UserDevices { get; set; }
    = new List<UserDevice>();

        public ICollection<UserSession> UserSessions { get; set; }
            = new List<UserSession>();

        public ICollection<LoginActivity> LoginActivities { get; set; }
            = new List<LoginActivity>();
    }
}
