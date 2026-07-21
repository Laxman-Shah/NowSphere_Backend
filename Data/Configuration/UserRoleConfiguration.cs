using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using smartApi.Entity;

namespace smartApi.Data.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> entity)
    {
        entity.ToTable("user_roles");

        entity.HasKey(ur => ur.UserRoleId);

        entity.Property(ur => ur.UserRoleId)
            .HasColumnName("user_role_id");

        entity.Property(ur => ur.UserId)
            .HasColumnName("user_id");

        entity.Property(ur => ur.RoleId)
            .HasColumnName("role_id");

        entity.Property(ur => ur.AssignedAt)
            .HasColumnName("assigned_at")
            .HasDefaultValueSql("now()");

        entity.Property(ur => ur.AssignedByUserId)
            .HasColumnName("assigned_by_user_id");

        entity.Property(ur => ur.RevokedAt)
            .HasColumnName("revoked_at");

        entity.Property(ur => ur.RevokedByUserId)
            .HasColumnName("revoked_by_user_id");

        entity.Property(ur => ur.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        entity.HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(ur => ur.AssignedByUser)
            .WithMany(u => u.AssignedUserRoles)
            .HasForeignKey(ur => ur.AssignedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(ur => ur.RevokedByUser)
            .WithMany(u => u.RevokedUserRoles)
            .HasForeignKey(ur => ur.RevokedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(ur => new { ur.UserId, ur.RoleId })
            .IsUnique();
    }
}