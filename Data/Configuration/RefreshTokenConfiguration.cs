using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using smartApi.Entity;

namespace smartApi.Data.Configuration
{
    public class RefreshTokenConfiguration
        : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(
            EntityTypeBuilder<RefreshToken> entity)
        {
            entity.ToTable("refresh_tokens");


            // ========================================================
            // PRIMARY KEY
            // ========================================================

            entity.HasKey(e => e.RefreshTokenId);

            entity.Property(e => e.RefreshTokenId)
                .HasColumnName("refresh_token_id");


            // ========================================================
            // FOREIGN-KEY PROPERTIES
            // ========================================================

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(e => e.UserSessionId)
                .HasColumnName("user_session_id")
                .HasColumnType("uuid")
                .IsRequired(false);


            // ========================================================
            // TOKEN PROPERTIES
            // ========================================================

            entity.Property(e => e.TokenHash)
                .HasColumnName("token_hash")
                .HasMaxLength(512)
                .IsRequired();

            entity.Property(e => e.TokenFamilyId)
                .HasColumnName("token_family_id")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.IssuedAt)
                .HasColumnName("issued_at")
                .HasDefaultValueSql("now()")
                .IsRequired();

            entity.Property(e => e.ExpiresAt)
                .HasColumnName("expires_at")
                .IsRequired();

            entity.Property(e => e.RevokedAt)
                .HasColumnName("revoked_at");

            entity.Property(e => e.RevokedReason)
                .HasColumnName("revoked_reason")
                .HasMaxLength(150);

            entity.Property(e => e.ReplacedByTokenId)
                .HasColumnName("replaced_by_token_id");

            entity.Property(e => e.CreatedByIp)
                .HasColumnName("created_by_ip")
                .HasMaxLength(45);

            entity.Property(e => e.RevokedByIp)
                .HasColumnName("revoked_by_ip")
                .HasMaxLength(45);

            entity.Property(e => e.UserAgent)
                .HasColumnName("user_agent")
                .HasColumnType("text");

            entity.Property(e => e.IsRevoked)
                .HasColumnName("is_revoked")
                .HasDefaultValue(false)
                .IsRequired();


            // ========================================================
            // IGNORED CALCULATED PROPERTIES
            // ========================================================

            entity.Ignore(e => e.IsExpired);

            entity.Ignore(e => e.IsActive);


            // ========================================================
            // INDEXES
            // ========================================================

            entity.HasIndex(e => e.TokenHash)
                .IsUnique()
                .HasDatabaseName(
                    "uq_refresh_tokens_token_hash");

            entity.HasIndex(e => new
            {
                e.TokenHash,
                e.ExpiresAt
            })
                .HasDatabaseName(
                    "idx_refresh_tokens_token_lookup");

            entity.HasIndex(e => new
            {
                e.UserId,
                e.IsRevoked
            })
                .HasDatabaseName(
                    "idx_refresh_tokens_user_active");

            entity.HasIndex(e => new
            {
                e.UserId,
                e.ExpiresAt
            })
                .HasDatabaseName(
                    "idx_refresh_tokens_user_expiry");

            entity.HasIndex(e => e.ExpiresAt)
                .HasDatabaseName(
                    "idx_refresh_tokens_expires_at");

            entity.HasIndex(e => e.TokenFamilyId)
                .HasDatabaseName(
                    "idx_refresh_tokens_family");

            entity.HasIndex(e => e.RevokedAt)
                .HasDatabaseName(
                    "idx_refresh_tokens_revoked_at");

            entity.HasIndex(e => e.UserSessionId)
                .HasDatabaseName(
                    "idx_refresh_tokens_user_session");


            // ========================================================
            // USER RELATIONSHIP
            // ========================================================

            entity.HasOne(e => e.User)
                .WithMany(e => e.RefreshTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            // ========================================================
            // USER SESSION RELATIONSHIP
            // ========================================================

            entity.HasOne(e => e.UserSession)
                .WithMany(e => e.RefreshTokens)
                .HasForeignKey(e => e.UserSessionId)
                .OnDelete(DeleteBehavior.Restrict);


            // ========================================================
            // SELF-REFERENCING TOKEN ROTATION RELATIONSHIP
            // ========================================================

            entity.HasOne(e => e.ReplacedByToken)
                .WithMany(e => e.ReplacedTokens)
                .HasForeignKey(e => e.ReplacedByTokenId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}