using SharedKernel;

namespace Taxonomy.Domain.Categories;

public sealed record CategoryCreatedEvent(CategoryId CategoryId, string Label) : DomainEvent;
public sealed record CategoryUpdatedEvent(CategoryId CategoryId, string Label) : DomainEvent;
public sealed record CategoryDeletedEvent(CategoryId CategoryId) : DomainEvent;
