using Identity.Application.Ports;
using Identity.Domain.Users;
using Identity.Infrastructure.Persistence.Configurations;
using SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence;

public sealed class IdentityDbContext : DbContext, IIdentityUnitOfWork
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options) { }

    public DbSet<User>         Users         => Set<User>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("identity");
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new IdentityOutboxMessageConfiguration());

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Domain Events atomar zusammen mit den Daten persistieren (Outbox Pattern)
        // Ein BackgroundService (IdentityOutboxProcessor) dispatcht sie anschließend via MediatR.
        var entities = ChangeTracker.Entries<Entity>().Select(e => e.Entity).ToList();
        var outboxMessages = MediatorExtensions.CollectOutboxMessages(entities);

        if (outboxMessages.Count > 0)
            OutboxMessages.AddRange(outboxMessages);

        return await base.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class IdentityOutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.EventType).HasMaxLength(512).IsRequired();
        builder.Property(m => m.Payload).IsRequired();
        builder.Property(m => m.OccurredOn).IsRequired();
        builder.HasIndex(m => m.ProcessedOn)
               .HasDatabaseName("IX_identity_OutboxMessages_ProcessedOn")
               .HasFilter("\"ProcessedOn\" IS NULL");
    }
}
