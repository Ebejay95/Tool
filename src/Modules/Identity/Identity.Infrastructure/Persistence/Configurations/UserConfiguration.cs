using Identity.Domain.Users;
using SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasConversion(
                id => id.Value,
                value => UserId.From(value))
            .ValueGeneratedNever();

        builder.Property(u => u.Email)
            .HasConversion(
                email => email.Value,
                value => Email.Create(value).Value)
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .HasConversion(
                hash => hash.Value,
                value => HashedPassword.FromHash(value))
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(u => u.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.LastLoginAt);

        builder.Property(u => u.IsActive)
            .IsRequired();

        builder.Property(u => u.PasswordResetToken)
            .HasMaxLength(50);

        builder.Property(u => u.PasswordResetTokenExpiry);

        // ── E-Mail-Verifizierung
        builder.Property(u => u.IsEmailVerified)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.EmailVerificationToken)
            .HasMaxLength(50);

        builder.Property(u => u.EmailVerificationTokenExpiry);

        builder.Property(u => u.EmailVerificationLastSentAt);

        // ── Zwei-Faktor-Authentifizierung (TOTP)
        builder.Property(u => u.TwoFactorSecret)
            .HasMaxLength(200);

        builder.Property(u => u.TwoFactorPendingSecret)
            .HasMaxLength(200);

        builder.Property(u => u.IsTwoFactorEnabled)
            .IsRequired()
            .HasDefaultValue(false);

        // ── Rolle
        builder.Property(u => u.Role)
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(Identity.Domain.Users.UserRoles.User);

        // Ignore domain events (not persisted)
        builder.Ignore(u => u.DomainEvents);
    }
}
