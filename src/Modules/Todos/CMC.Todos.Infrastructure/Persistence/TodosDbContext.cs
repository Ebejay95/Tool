using CMC.SharedKernel;
using CMC.Todos.Application.Ports;
using CMC.Todos.Domain.TodoItems;
using CMC.Todos.Infrastructure.Persistence.Configurations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CMC.Todos.Infrastructure.Persistence;

public sealed class TodosDbContext : DbContext, ITodosUnitOfWork
{
    private readonly IMediator _mediator;

    public TodosDbContext(DbContextOptions<TodosDbContext> options, IMediator mediator)
        : base(options)
    {
        _mediator = mediator;
    }

    public DbSet<TodoItem> Todos => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TodoItemConfiguration());

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Entitäten VOR dem Save sammeln (ChangeTracker-State ändert sich danach)
        var entities = ChangeTracker.Entries<Entity>().Select(e => e.Entity).ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        // Domain-Events dispatchen und leeren (SharedKernel-Extension)
        await _mediator.DispatchDomainEventsAsync(entities, cancellationToken);

        return result;
    }
}
