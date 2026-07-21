using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using smartApi.Entity;

namespace smartApi.Data.Configurations
{
    public class EmailOtpTokenConfiguration : IEntityTypeConfiguration<email_otp_tokens>
    {
        public void Configure(EntityTypeBuilder<email_otp_tokens> entity)
        {
            entity.ToTable("email_otp_tokens");

            entity.HasKey(e => e.EmailOtpTokenId);

            entity.Property(e => e.EmailOtpTokenId)
                .HasColumnName("email_otp_token_id");

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(e => e.LoginChallengeId)
                .HasColumnName("login_challenge_id")
                .IsRequired(false);

            entity.Property(e => e.SentToEmail)
                .HasColumnName("sent_to_email")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.TokenHash)
                .HasColumnName("token_hash")
                .HasMaxLength(512)
                .IsRequired();

            entity.Property(e => e.Purpose)
                .HasColumnName("purpose")
                .HasMaxLength(60)
                .IsRequired();

            entity.Property(e => e.ExpiresAt)
                .HasColumnName("expires_at")
                .IsRequired();

            entity.Property(e => e.UsedAt)
                .HasColumnName("used_at");

            entity.Property(e => e.RevokedAt)
                .HasColumnName("revoked_at");

            entity.Property(e => e.RevokedReason)
                .HasColumnName("revoked_reason")
                .HasMaxLength(150);

            entity.Property(e => e.AttemptCount)
                .HasColumnName("attempt_count")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(e => e.MaxAttempts)
                .HasColumnName("max_attempts")
                .HasDefaultValue(5)
                .IsRequired();

            entity.Property(e => e.ResendCount)
                .HasColumnName("resend_count")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(e => e.LastSentAt)
                .HasColumnName("last_sent_at");

            entity.Property(e => e.CreatedByIp)
                .HasColumnName("created_by_ip")
                .HasMaxLength(45);

            entity.Property(e => e.UserAgent)
                .HasColumnName("user_agent")
                .HasColumnType("text");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()")
                .IsRequired();

            // ============================
            // Indexes
            // ============================

            entity.HasIndex(e => new { e.UserId, e.Purpose, e.UsedAt })
                .HasDatabaseName("idx_email_otp_tokens_lookup");

            entity.HasIndex(e => new { e.UserId, e.Purpose, e.ExpiresAt })
                .HasDatabaseName("idx_email_otp_tokens_user_purpose_expiry");

            entity.HasIndex(e => new { e.UserId, e.Purpose, e.RevokedAt })
                .HasDatabaseName("idx_email_otp_tokens_user_purpose_revoked");

            entity.HasIndex(e => e.SentToEmail)
                .HasDatabaseName("idx_email_otp_tokens_sent_to_email");

            entity.HasIndex(e => e.ExpiresAt)
                .HasDatabaseName("idx_email_otp_tokens_expiry");

            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("idx_email_otp_tokens_created_at");

            entity.HasIndex(e => e.RevokedAt)
                .HasDatabaseName("idx_email_otp_tokens_revoked_at");

            entity.HasIndex(e => new
            {
                e.LoginChallengeId,
                e.Purpose
            })
            .HasDatabaseName("ix_email_otp_tokens_challenge_id_purpose");

            // ============================
            // Relationships
            // ============================

            entity.HasOne(e => e.User)
                .WithMany(u => u.EmailOtpTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.LoginChallenge)
                .WithMany(c => c.OtpTokens)
                .HasForeignKey(e => e.LoginChallengeId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        }
    }
}