using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using smartApi.Entity;

namespace smartApi.Data.Configuration
{


    public sealed class UserDeviceConfiguration
        : IEntityTypeConfiguration<UserDevice>
    {
        public void Configure(EntityTypeBuilder<UserDevice> builder)
        {
            builder.ToTable("user_devices");

            builder.HasKey(x => x.UserDeviceId);

            builder.Property(x => x.UserDeviceId)
                .HasColumnName("user_device_id");

            builder.Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder.Property(x => x.DeviceFingerprintHash)
                .HasColumnName("device_fingerprint_hash")
                .HasMaxLength(512)
                .IsRequired();

            builder.Property(x => x.DeviceName)
                .HasColumnName("device_name")
                .HasMaxLength(150);

            builder.Property(x => x.DeviceType)
                .HasColumnName("device_type")
                .HasMaxLength(30)
                .HasDefaultValue("UNKNOWN")
                .IsRequired();

            builder.Property(x => x.OperatingSystem)
                .HasColumnName("operating_system")
                .HasMaxLength(100);

            builder.Property(x => x.OperatingSystemVersion)
                .HasColumnName("operating_system_version")
                .HasMaxLength(50);

            builder.Property(x => x.BrowserName)
                .HasColumnName("browser_name")
                .HasMaxLength(100);

            builder.Property(x => x.BrowserVersion)
                .HasColumnName("browser_version")
                .HasMaxLength(50);

            builder.Property(x => x.LastIpAddress)
                .HasColumnName("last_ip_address")
                .HasMaxLength(45);

            builder.Property(x => x.LastUserAgent)
                .HasColumnName("last_user_agent");

            builder.Property(x => x.FirstSeenAt)
                .HasColumnName("first_seen_at")
                .HasDefaultValueSql("now()")
                .IsRequired();

            builder.Property(x => x.LastSeenAt)
                .HasColumnName("last_seen_at")
                .HasDefaultValueSql("now()")
                .IsRequired();

            builder.Property(x => x.IsTrusted)
                .HasColumnName("is_trusted")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(x => x.TrustedAt)
                .HasColumnName("trusted_at");

            builder.Property(x => x.TrustExpiresAt)
                .HasColumnName("trust_expires_at");

            builder.Property(x => x.TrustRevokedAt)
                .HasColumnName("trust_revoked_at");

            builder.Property(x => x.TrustRevokedReason)
                .HasColumnName("trust_revoked_reason")
                .HasMaxLength(150);

            builder.Property(x => x.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(x => x.DeactivatedAt)
                .HasColumnName("deactivated_at");

            builder.Property(x => x.DeactivationReason)
                .HasColumnName("deactivation_reason")
                .HasMaxLength(150);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()")
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

            builder.HasIndex(x => new
            {
                x.UserId,
                x.DeviceFingerprintHash
            })
                .IsUnique()
                .HasDatabaseName("uq_user_devices_user_fingerprint");

            builder.HasIndex(x => new { x.UserId, x.IsActive })
                .HasDatabaseName("idx_user_devices_user_active");

            builder.HasIndex(x => new { x.UserId, x.IsTrusted })
                .HasDatabaseName("idx_user_devices_user_trusted");

            builder.HasIndex(x => x.LastSeenAt)
                .HasDatabaseName("idx_user_devices_last_seen");

            builder.HasIndex(x => x.TrustExpiresAt)
                .HasDatabaseName("idx_user_devices_trust_expiry");

            builder.HasOne(x => x.User)
                .WithMany(x => x.UserDevices)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}