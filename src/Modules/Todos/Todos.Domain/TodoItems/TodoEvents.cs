using SharedKernel;

namespace Todos.Domain.TodoItems;

public sealed record TodoCreatedEvent(TodoId TodoId, UserId UserId, string Title) : DomainEvent;

public sealed record TodoUpdatedEvent(TodoId TodoId, UserId UserId, string Title) : DomainEvent;

public sealed record TodoCompletedEvent(TodoId TodoId, UserId UserId, string Title) : DomainEvent;

public sealed record TodoCancelledEvent(TodoId TodoId, UserId UserId, string Title) : DomainEvent;

public sealed record TodoStatusChangedEvent(TodoId TodoId, UserId UserId, TodoStatus NewStatus) : DomainEvent;

public sealed record TodoDeletedEvent(TodoId TodoId, UserId UserId, string Title) : DomainEvent;
