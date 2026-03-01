using CMC.Identity.Application.Ports;
using CMC.Identity.Domain.Users;
using CMC.Identity.Infrastructure.Persistence.Configurations;
using CMC.SharedKernel;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CMC.Identity.Infrastructure.Persistence;

public sealed class IdentityDbContext : DbContext, IIdentityUnitOfWork
{
    private readonly IMediator _mediator;

    public IdentityDbContext(DbContextOptions<IdentityDbContext> options, IMediator mediator)
        : base(options)
    {
        _mediator = mediator;
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());

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
