namespace SharedKernel;

/// <summary>
/// Domain-Objekte mit einem Owner implementieren dieses Interface,
/// damit der generische OwnershipHandler greift.
/// </summary>
public interface IResourceOwner
{
    string OwnerId { get; }
}

/// <summary>
/// Repräsentiert den aktuell eingeloggten Benutzer, framework-agnostisch.
/// Implementierung liegt in der jeweiligen Host-Schicht (z.B. Api).
/// </summary>
public interface ICurrentUser
{
    UserId? UserId         { get; }
    string? Email          { get; }
    bool    IsAuthenticated { get; }
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IRepository<T> where T : AggregateRoot
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    void Add(T entity);
    void Update(T entity);
    void Remove(T entity);
}

public interface IDomainEventDispatcher
{
    Task DispatchEventsAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}

public abstract record Query<TResponse> : MediatR.IRequest<Result<TResponse>>;

public abstract record Command : MediatR.IRequest<Result>;

public abstract record Command<TResponse> : MediatR.IRequest<Result<TResponse>>;
