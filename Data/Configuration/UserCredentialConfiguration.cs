using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using smartApi.Entity;

namespace smartApi.Data.Configurations;

public class UserCredentialConfiguration : IEntityTypeConfiguration<UserCredential>
{
    public void Configure(EntityTypeBuilder<UserCredential> entity)
    {
        entity.ToTable("user_credentials");

        entity.HasKey(c => c.CredentialId);

        entity.Property(c => c.CredentialId)
            .HasColumnName("credential_id");

        entity.Property(c => c.UserId)
            .HasColumnName("user_id");

        entity.Property(c => c.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(255)
            .IsRequired();

        entity.Property(c => c.PasswordAlgorithm)
            .HasColumnName("password_algorithm")
            .HasMaxLength(50)
            .HasDefaultValue("BCrypt");

        entity.Property(c => c.PasswordSalt)
            .HasColumnName("password_salt")
            .HasMaxLength(255);

        entity.Property(c => c.PasswordCreatedAt)
            .HasColumnName("password_created_at")
            .HasDefaultValueSql("now()");

        entity.Property(c => c.PasswordUpdatedAt)
            .HasColumnName("password_updated_at");

        entity.Property(c => c.MustChangePassword)
            .HasColumnName("must_change_password")
            .HasDefaultValue(false);

        entity.HasOne(c => c.User)
            .WithOne(u => u.Credential)
            .HasForeignKey<UserCredential>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(c => c.UserId)
            .IsUnique();
    }
}