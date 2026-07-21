namespace smartApi.Entity
{
    public class Role
    {
        public long RoleId { get; set; }

        public string RoleName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsSystemRole { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }


        // One role can belong to many users through user_roles
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
