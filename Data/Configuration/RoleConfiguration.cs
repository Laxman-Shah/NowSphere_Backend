using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using smartApi.Entity;

namespace smartApi.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> entity)
    {
        entity.ToTable("roles");

        entity.HasKey(r => r.RoleId);

        entity.Property(r => r.RoleId)
            .HasColumnName("role_id");

        entity.Property(r => r.RoleName)
            .HasColumnName("role_name")
            .HasMaxLength(50)
            .IsRequired();

        entity.Property(r => r.Description)
            .HasColumnName("description")
            .HasMaxLength(255);

        entity.Property(r => r.IsSystemRole)
            .HasColumnName("is_system_role")
            .HasDefaultValue(true);

        entity.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()");

        entity.Property(r => r.UpdatedAt)
            .HasColumnName("updated_at");

        entity.HasIndex(r => r.RoleName)
            .IsUnique();

        entity.HasData(
            new Role
            {
                RoleId = 1,
                RoleName = "ADMIN",
                Description = "System administrator with full access",
                IsSystemRole = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Role
            {
                RoleId = 2,
                RoleName = "USER",
                Description = "Default normal user role",
                IsSystemRole = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Role
            {
                RoleId = 3,
                RoleName = "MANAGER",
                Description = "Manager role with limited administrative access",
                IsSystemRole = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Role
            {
                RoleId = 4,
                RoleName = "SUPPORT",
                Description = "Support role for customer or user assistance",
                IsSystemRole = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}