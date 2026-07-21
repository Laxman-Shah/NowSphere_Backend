using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using smartApi.Entity;

namespace smartApi.Data.Configuration
{


    public sealed class UserSessionConfiguration
        : IEntityTypeConfiguration<UserSession>
    {
        public void Configure(EntityTypeBuilder<UserSession> builder)
        {
            builder.ToTable("user_sessions");

            builder.HasKey(x => x.UserSessionId);

            builder.Property(x => x.UserSessionId)
                .HasColumnName("user_session_id")
                .ValueGeneratedNever();

            builder.Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder.Property(x => x.UserDeviceId)
                .HasColumnName("user_device_id")
                .IsRequired();

            builder.Property(x => x.LoginChallengeId)
                .HasColumnName("login_challenge_id");

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasMaxLength(30)
                .HasDefaultValue("ACTIVE")
                .IsRequired();

            builder.Property(x => x.AuthenticationLevel)
                .HasColumnName("authentication_level")
                .HasMaxLength(30)
                .HasDefaultValue("PASSWORD_OTP")
                .IsRequired();

            builder.Property(x => x.AuthenticationMethods)
                .HasColumnName("authentication_methods")
                .HasMaxLength(150);

            builder.Property(x => x.OtpVerified)
                .HasColumnName("otp_verified")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(x => x.OtpVerifiedAt)
                .HasColumnName("otp_verified_at");

            builder.Property(x => x.LoginIpAddress)
                .HasColumnName("login_ip_address")
                .HasMaxLength(45);

            builder.Property(x => x.LoginUserAgent)
                .HasColumnName("login_user_agent");

            builder.Property(x => x.LastIpAddress)
                .HasColumnName("last_ip_address")
                .HasMaxLength(45);

            builder.Property(x => x.LastUserAgent)
                .HasColumnName("last_user_agent");

            builder.Property(x => x.LoginAt)
                .HasColumnName("login_at")
                .HasDefaultValueSql("now()")
                .IsRequired();

            builder.Property(x => x.LastActivityAt)
                .HasColumnName("last_activity_at")
                .HasDefaultValueSql("now()")
                .IsRequired();

            builder.Property(x => x.ExpiresAt)
                .HasColumnName("expires_at")
                .IsRequired();

            builder.Property(x => x.LoggedOutAt)
                .HasColumnName("logged_out_at");

            builder.Property(x => x.LogoutReason)
                .HasColumnName("logout_reason")
                .HasMaxLength(150);

            builder.Property(x => x.RevokedAt)
                .HasColumnName("revoked_at");

            builder.Property(x => x.RevokedReason)
                .HasColumnName("revoked_reason")
                .HasMaxLength(150);

            builder.Property(x => x.RevokedBy)
                .HasColumnName("revoked_by")
                .HasMaxLength(30);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()")
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

            builder.Property(x => x.ConcurrencyToken)
                .HasColumnName("concurrency_token")
                .IsConcurrencyToken()
                .IsRequired();

            builder.HasIndex(x => new { x.UserId, x.Status })
                .HasDatabaseName("idx_user_sessions_user_status");

            builder.HasIndex(x => new { x.UserId, x.ExpiresAt })
                .HasDatabaseName("idx_user_sessions_user_expiry");

            builder.HasIndex(x => new { x.UserDeviceId, x.Status })
                .HasDatabaseName("idx_user_sessions_device_status");

            builder.HasIndex(x => x.LoginChallengeId)
                .IsUnique()
                .HasDatabaseName("uq_user_sessions_login_challenge");

            builder.HasIndex(x => x.ExpiresAt)
                .HasDatabaseName("idx_user_sessions_expiry");

            builder.HasIndex(x => x.LastActivityAt)
                .HasDatabaseName("idx_user_sessions_last_activity");

            builder.HasIndex(x => x.RevokedAt)
                .HasDatabaseName("idx_user_sessions_revoked");

            builder.HasOne(x => x.User)
                .WithMany(x => x.UserSessions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.UserDevice)
                .WithMany(x => x.UserSessions)
                .HasForeignKey(x => x.UserDeviceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.LoginChallenge)
                .WithOne(x => x.UserSession)
                .HasForeignKey<UserSession>(x => x.LoginChallengeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}