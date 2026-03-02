using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel;
using Taxonomy.Application.Ports;
using Taxonomy.Domain.Categories;
using Taxonomy.Domain.Tags;
using Taxonomy.Infrastructure.Persistence.Configurations;

namespace Taxonomy.Infrastructure.Persistence;

public sealed class TaxonomyDbContext : DbContext, ITaxonomyUnitOfWork
{
    public TaxonomyDbContext(DbContextOptions<TaxonomyDbContext> options)
        : base(options) { }

    public DbSet<Category>      Categories     => Set<Category>();
    public DbSet<Tag>           Tags           => Set<Tag>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("taxonomy");
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new TagConfiguration());
        modelBuilder.ApplyConfiguration(new TaxonomyOutboxMessageConfiguration());

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

internal sealed class TaxonomyOutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.EventType).HasMaxLength(512).IsRequired();
        builder.Property(m => m.Payload).IsRequired();
        builder.Property(m => m.OccurredOn).IsRequired();
        builder.HasIndex(m => m.ProcessedOn)
               .HasDatabaseName("IX_taxonomy_OutboxMessages_ProcessedOn")
               .HasFilter("\"ProcessedOn\" IS NULL");
    }
}
