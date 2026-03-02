using SharedKernel;
using Todos.Application.Ports;
using Todos.Domain.Todos;
using Todos.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Todos.Infrastructure.Persistence;

public sealed class TodosDbContext : DbContext, ITodosUnitOfWork
{
    public TodosDbContext(DbContextOptions<TodosDbContext> options)
        : base(options) { }

    public DbSet<Todo>     Todos         => Set<Todo>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("todos");
        modelBuilder.ApplyConfiguration(new TodoConfiguration());
        modelBuilder.ApplyConfiguration(new TodosOutboxMessageConfiguration());

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Domain Events atomar zusammen mit den Daten persistieren (Outbox Pattern)
        // Ein BackgroundService (TodosOutboxProcessor) dispatcht sie anschließend via MediatR.
        var entities = ChangeTracker.Entries<Entity>().Select(e => e.Entity).ToList();
        var outboxMessages = MediatorExtensions.CollectOutboxMessages(entities);

        if (outboxMessages.Count > 0)
            OutboxMessages.AddRange(outboxMessages);

        return await base.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class TodosOutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.EventType).HasMaxLength(512).IsRequired();
        builder.Property(m => m.Payload).IsRequired();
        builder.Property(m => m.OccurredOn).IsRequired();
        builder.HasIndex(m => m.ProcessedOn)
               .HasDatabaseName("IX_todos_OutboxMessages_ProcessedOn")
               .HasFilter("\"ProcessedOn\" IS NULL");
    }
}
