namespace smartApi.Entity
{
    public class UserRole
    {
        public long UserRoleId { get; set; }

        public long UserId { get; set; }

        public long RoleId { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        public long? AssignedByUserId { get; set; }

        public DateTime? RevokedAt { get; set; }

        public long? RevokedByUserId { get; set; }

        public bool IsActive { get; set; } = true;


        // Navigation properties
        public User User { get; set; } = null!;

        public Role Role { get; set; } = null!;

        public User? AssignedByUser { get; set; }

        public User? RevokedByUser { get; set; }
    }
}
