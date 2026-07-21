using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using smartApi.Entity;
using smartApi.Enums;

namespace smartApi.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.ToTable("users");

        entity.HasKey(u => u.UserId);

        entity.Property(u => u.UserId)
            .HasColumnName("user_id");

        entity.Property(u => u.Username)
            .HasColumnName("username")
            .HasMaxLength(50)
            .IsRequired();

        entity.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(u => u.PhoneNumber)
            .HasColumnName("phone_number")
            .HasMaxLength(20);

        entity.Property(u => u.FullName)
            .HasColumnName("full_name")
            .HasMaxLength(100);

        entity.Property(u => u.AccountStatus)
            .HasColumnName("account_status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(AccountStatus.Active)
            // CLR default is 0 (undefined). Treat that as the sentinel so
            // EF uses the database default only when the value was unset.
            .HasSentinel((AccountStatus)0)
            .IsRequired();

        entity.Property(u => u.EmailVerified)
            .HasColumnName("email_verified")
            .HasDefaultValue(false);

        entity.Property(u => u.LastLoginAt)
            .HasColumnName("last_login_at");

        entity.Property(u => u.FailedLoginCount)
            .HasColumnName("failed_login_count")
            .HasDefaultValue(0);

        entity.Property(u => u.LockedUntil)
            .HasColumnName("locked_until");

        entity.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()");

        entity.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at");

        entity.Property(u => u.DeletedAt)
            .HasColumnName("deleted_at");

        entity.HasIndex(u => u.Username).IsUnique();

        entity.HasIndex(u => u.Email).IsUnique();
    }
}