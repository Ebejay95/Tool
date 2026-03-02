using Measures.Application.Ports;
using Measures.Domain.Measures;
using Measures.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel;

namespace Measures.Infrastructure.Persistence;

public sealed class MeasuresDbContext : DbContext, IMeasuresUnitOfWork
{
    public MeasuresDbContext(DbContextOptions<MeasuresDbContext> options)
        : base(options) { }

    public DbSet<Measure> Measures => Set<Measure>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("measures");
        modelBuilder.ApplyConfiguration(new MeasureConfiguration());
        modelBuilder.ApplyConfiguration(new MeasuresOutboxMessageConfiguration());

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entities = ChangeTracker.Entries<Entity>().Select(e => e.Entity).ToList();
        var outboxMessages = MediatorExtensions.CollectOutboxMessages(entities);

        if (outboxMessages.Count > 0)
            OutboxMessages.AddRange(outboxMessages);

        return await base.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class MeasuresOutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.EventType).HasMaxLength(512).IsRequired();
        builder.Property(m => m.Payload).IsRequired();
        builder.Property(m => m.OccurredOn).IsRequired();
        builder.HasIndex(m => m.ProcessedOn)
               .HasDatabaseName("IX_measures_OutboxMessages_ProcessedOn")
               .HasFilter("\"ProcessedOn\" IS NULL");
    }
}
