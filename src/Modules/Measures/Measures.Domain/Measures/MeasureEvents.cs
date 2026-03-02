using SharedKernel;

namespace Measures.Domain.Measures;

public sealed record MeasureCreatedEvent(MeasureId MeasureId, UserId UserId, string Name) : DomainEvent;
public sealed record MeasureUpdatedEvent(MeasureId MeasureId, UserId UserId, string Name) : DomainEvent;
public sealed record MeasureDeletedEvent(MeasureId MeasureId, UserId UserId, string Name) : DomainEvent;
