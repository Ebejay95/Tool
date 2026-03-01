using Notifications.Abstractions;
using Notifications.Domain;
using SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Notifications.Infrastructure.Persistence;

public sealed class NotificationsDbContext : DbContext
{
    public NotificationsDbContext(DbContextOptions<NotificationsDbContext> options)
        : base(options) { }

    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("notifications");
        modelBuilder.ApplyConfiguration(new NotificationConfiguration());
    }
}

internal sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications", "notifications");

        // Primärschlüssel via Value Object Converter
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id)
            .HasConversion(
                id  => id.Value,
                raw => NotificationId.From(raw))
            .HasColumnName("Id");

        // RecipientId (UserId Value Object)
        builder.Property(n => n.RecipientId)
            .HasConversion(
                uid => uid.Value,
                raw => UserId.From(raw))
            .HasColumnName("RecipientId")
            .IsRequired();

        builder.Property(n => n.Title)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(n => n.Body)
            .IsRequired();

        builder.Property(n => n.Severity)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(n => n.IsRead)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(n => n.ExpiresAt)
            .IsRequired(false);

        builder.Property(n => n.CreatedAt)
            .IsRequired();

        // Interactions als JSON-Spalte (EF Core 8 owned-entity JSON)
        // Kein expliziter Key – EF Core generiert auto den Ordinal-Shadow-Key für ToJson()
        builder.OwnsMany(n => n.Interactions, interactionBuilder =>
        {
            interactionBuilder.ToJson();

            interactionBuilder.Property(i => i.Label).HasMaxLength(128).IsRequired();
            interactionBuilder.Property(i => i.Type).HasConversion<int>().IsRequired();
            interactionBuilder.Property(i => i.Href).HasMaxLength(2048).IsRequired(false);
            interactionBuilder.Property(i => i.ActionName).HasMaxLength(256).IsRequired(false);
        });

        // Index für schnelle Abfragen nach Empfänger + Lesestatus
        builder.HasIndex(n => new { n.RecipientId, n.IsRead })
            .HasDatabaseName("IX_Notifications_RecipientId_IsRead");

        builder.HasIndex(n => new { n.RecipientId, n.CreatedAt })
            .HasDatabaseName("IX_Notifications_RecipientId_CreatedAt");
    }
}
