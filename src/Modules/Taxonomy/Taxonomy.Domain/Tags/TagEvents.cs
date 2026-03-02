using SharedKernel;

namespace Taxonomy.Domain.Tags;

public sealed record TagCreatedEvent(TagId TagId, UserId? UserId, string Label) : DomainEvent;
public sealed record TagUpdatedEvent(TagId TagId, string Label) : DomainEvent;
public sealed record TagDeletedEvent(TagId TagId) : DomainEvent;
