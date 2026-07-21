using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using smartApi.Entity;

namespace smartApi.Data.Configurations
{
    public sealed class LoginChallengeConfiguration
        : IEntityTypeConfiguration<LoginChallenge>
    {
        public void Configure(EntityTypeBuilder<LoginChallenge> builder)
        {
            // ========================================================
            // TABLE NAME
            // ========================================================

            builder.ToTable("login_challenges");

            // ========================================================
            // PRIMARY KEY
            // ========================================================

            builder.HasKey(challenge => challenge.LoginChallengeId);

            builder.Property(challenge => challenge.LoginChallengeId)
                .HasColumnName("login_challenge_id")
                .ValueGeneratedNever();

            /*
             * ValueGeneratedNever is used because the application
             * creates the ID with Guid.NewGuid().
             */

            // ========================================================
            // USER FOREIGN KEY
            // ========================================================

            builder.Property(challenge => challenge.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            // ========================================================
            // CHALLENGE STATUS
            // ========================================================

            builder.Property(challenge => challenge.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            /*
             * Stores enum values as:
             * Pending
             * Completed
             * Expired
             * Revoked
             */

            // ========================================================
            // CHALLENGE EXPIRATION
            // ========================================================

            builder.Property(challenge => challenge.ExpiresAt)
                .HasColumnName("expires_at")
                .IsRequired();

            // ========================================================
            // CHALLENGE COMPLETION
            // ========================================================

            builder.Property(challenge => challenge.CompletedAt)
                .HasColumnName("completed_at")
                .IsRequired(false);

            // ========================================================
            // CHALLENGE REVOCATION
            // ========================================================

            builder.Property(challenge => challenge.RevokedAt)
                .HasColumnName("revoked_at")
                .IsRequired(false);

            builder.Property(challenge => challenge.RevokedReason)
                .HasColumnName("revoked_reason")
                .HasMaxLength(150)
                .IsRequired(false);

            // ========================================================
            // REQUEST SECURITY INFORMATION
            // ========================================================

            builder.Property(challenge => challenge.CreatedByIp)
                .HasColumnName("created_by_ip")
                .HasMaxLength(45)
                .IsRequired(false);

            builder.Property(challenge => challenge.UserAgent)
                .HasColumnName("user_agent")
                .HasColumnType("text")
                .IsRequired(false);

            // ========================================================
            // CREATED DATE
            // ========================================================

            builder.Property(challenge => challenge.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            // ========================================================
            // OTP RESEND INFORMATION
            // ========================================================

            builder.Property(challenge => challenge.ResendCount)
                .HasColumnName("resend_count")
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(challenge => challenge.LastOtpSentAt)
                .HasColumnName("last_otp_sent_at")
                .IsRequired(false);

            // ========================================================
            // OPTIMISTIC CONCURRENCY
            // ========================================================

            builder.Property(challenge => challenge.ConcurrencyToken)
                .HasColumnName("concurrency_token")
                .IsConcurrencyToken()
                .IsRequired();

            /*
             * Generate a new Guid whenever the challenge changes.
             */

            // ========================================================
            // USER RELATIONSHIP
            // One User -> Many LoginChallenges
            // ========================================================

            builder.HasOne(challenge => challenge.User)
                .WithMany(user => user.LoginChallenges)
                .HasForeignKey(challenge => challenge.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========================================================
            // EMAIL OTP TOKENS RELATIONSHIP
            // One LoginChallenge -> Many EmailOtpTokens
            // ========================================================

            builder.HasMany(challenge => challenge.OtpTokens)
                .WithOne(token => token.LoginChallenge)
                .HasForeignKey(token => token.LoginChallengeId)
                .OnDelete(DeleteBehavior.Restrict);

            /*
             * A LoginChallenge can generate multiple OTP tokens
             * (for example after resend operations).
             *
             * We use Restrict so deleting a LoginChallenge will not
             * automatically delete OTP history.
             */

            // ========================================================
            // INDEX : USER ID + STATUS
            // ========================================================

            builder.HasIndex(challenge => new
            {
                challenge.UserId,
                challenge.Status
            })
            .HasDatabaseName("ix_login_challenges_user_id_status");

            // ========================================================
            // INDEX : EXPIRATION DATE
            // ========================================================

            builder.HasIndex(challenge => challenge.ExpiresAt)
                .HasDatabaseName("ix_login_challenges_expires_at");

            // ========================================================
            // INDEX : STATUS + EXPIRATION
            // ========================================================

            builder.HasIndex(challenge => new
            {
                challenge.Status,
                challenge.ExpiresAt
            })
            .HasDatabaseName("ix_login_challenges_status_expires_at");
        }
    }
}