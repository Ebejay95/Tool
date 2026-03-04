using SharedKernel;

namespace Todos.Domain.Todos;

public enum TodoStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3
}

public enum TodoPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

public sealed class Todo : AggregateRoot, IResourceOwner, IExportable
{
    // IResourceOwner: erlaubt generischen OwnershipHandler ohne Todo-spezifischen Code in Api
    string IResourceOwner.OwnerId => UserId.Value.ToString();

    // IExportable: macht Todo in der Import/Export-Registry verfügbar
    public static string ExportableTypeName => "Todo";

    private Todo() { } // For EF

    private Todo(
        TodoId todoId,
        UserId userId,
        string title,
        string description,
        TodoPriority priority,
        DateTime? dueDate)
    {
        Id = todoId;
        UserId = userId;
        Title = title;
        Description = description;
        Priority = priority;
        DueDate = dueDate;
        Status = TodoStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TodoCreatedEvent(todoId, userId, title));
    }

    public new TodoId Id { get; private set; } = null!;
    public UserId UserId { get; private set; } = null!;

    [ExportField("Titel", order: 1)]
    public string Title { get; private set; } = string.Empty;

    [ExportField("Beschreibung", order: 2)]
    public string Description { get; private set; } = string.Empty;

    [ExportField("Status", order: 3)]
    public TodoStatus Status { get; private set; }

    [ExportField("Priorität", order: 4)]
    public TodoPriority Priority { get; private set; }

    [ExportField("Fälligkeitsdatum", order: 5)]
    public DateTime? DueDate { get; private set; }

    [ExportField("Erstellt am", order: 6, canImport: false)]
    public DateTime CreatedAt { get; private set; }

    [ExportField("Aktualisiert am", order: 7, canImport: false)]
    public DateTime UpdatedAt { get; private set; }

    [ExportField("Abgeschlossen am", order: 8, canImport: false)]
    public DateTime? CompletedAt { get; private set; }

    public List<Guid> CategoryIds { get; private set; } = [];
    public List<Guid> TagIds { get; private set; } = [];

    [ExportField("Überfällig", order: 9, canImport: false)]
    public bool IsOverdue => DueDate.HasValue && DueDate < DateTime.UtcNow && Status != TodoStatus.Completed;

    public void SetCategories(IEnumerable<Guid> ids) { CategoryIds = ids.ToList(); UpdatedAt = DateTime.UtcNow; }
    public void SetTags(IEnumerable<Guid> ids)       { TagIds       = ids.ToList(); UpdatedAt = DateTime.UtcNow; }

    public static Result<Todo> Create(
        UserId userId,
        string title,
        string description,
        TodoPriority priority = TodoPriority.Medium,
        DateTime? dueDate = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<Todo>(TodoErrors.TitleRequired);

        if (title.Length > 200)
            return Result.Failure<Todo>(TodoErrors.TitleTooLong);

        if (description.Length > 2000)
            return Result.Failure<Todo>(TodoErrors.DescriptionTooLong);

        if (dueDate.HasValue && dueDate < DateTime.UtcNow.Date)
            return Result.Failure<Todo>(TodoErrors.DueDateInPast);

        var todoId = TodoId.New();
        var todo = new Todo(todoId, userId, title.Trim(), description.Trim(), priority, dueDate);

        return Result.Success(todo);
    }

    public Result Update(string title, string description, TodoPriority priority, DateTime? dueDate)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure(TodoErrors.TitleRequired);

        if (title.Length > 200)
            return Result.Failure(TodoErrors.TitleTooLong);

        if (description.Length > 2000)
            return Result.Failure(TodoErrors.DescriptionTooLong);

        if (dueDate.HasValue && dueDate < DateTime.UtcNow.Date &&  Status != TodoStatus.Completed)
            return Result.Failure(TodoErrors.DueDateInPast);

        Title = title.Trim();
        Description = description.Trim();
        Priority = priority;
        DueDate = dueDate;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TodoUpdatedEvent(Id, UserId, Title));

        return Result.Success();
    }

    public Result MarkAsInProgress()
    {
        if (Status == TodoStatus.Completed)
            return Result.Failure(TodoErrors.CannotChangeCompletedTodo);

        if (Status == TodoStatus.Cancelled)
            return Result.Failure(TodoErrors.CannotChangeCancelledTodo);

        Status = TodoStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TodoStatusChangedEvent(Id, UserId, Status));

        return Result.Success();
    }

    public Result MarkAsCompleted()
    {
        if (Status == TodoStatus.Completed)
            return Result.Success(); // Already completed

        if (Status == TodoStatus.Cancelled)
            return Result.Failure(TodoErrors.CannotChangeCancelledTodo);

        Status = TodoStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TodoCompletedEvent(Id, UserId, Title));

        return Result.Success();
    }

    public Result MarkAsPending()
    {
        if (Status == TodoStatus.Cancelled)
            return Result.Failure(TodoErrors.CannotChangeCancelledTodo);

        Status = TodoStatus.Pending;
        CompletedAt = null;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TodoStatusChangedEvent(Id, UserId, Status));

        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status == TodoStatus.Cancelled)
            return Result.Success(); // Already cancelled

        Status = TodoStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TodoCancelledEvent(Id, UserId, Title));

        return Result.Success();
    }

    public void MarkForDeletion()
    {
        AddDomainEvent(new TodoDeletedEvent(Id, UserId, Title));
    }
}
