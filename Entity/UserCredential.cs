namespace smartApi.Entity
{
    public class UserCredential
    {
        public long CredentialId { get; set; }

        public long UserId { get; set; }

        public string PasswordHash { get; set; } = string.Empty;

        public string PasswordAlgorithm { get; set; } = "BCrypt";

        public string? PasswordSalt { get; set; }

        public DateTime PasswordCreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? PasswordUpdatedAt { get; set; }

        public bool MustChangePassword { get; set; } = false;


        // Navigation property
        public User User { get; set; } = null!;
    }
}
