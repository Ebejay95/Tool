using CMC.SharedKernel;

namespace CMC.Todos.Domain.TodoItems;

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

public sealed class TodoItem : AggregateRoot, IResourceOwner
{
    // IResourceOwner: erlaubt generischen OwnershipHandler ohne Todo-spezifischen Code in CMC.Web
    string IResourceOwner.OwnerId => UserId.Value.ToString();

    private TodoItem() { } // For EF

    private TodoItem(
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
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public TodoStatus Status { get; private set; }
    public TodoPriority Priority { get; private set; }
    public DateTime? DueDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public bool IsOverdue => DueDate.HasValue && DueDate < DateTime.UtcNow && Status != TodoStatus.Completed;

    public static Result<TodoItem> Create(
        UserId userId,
        string title,
        string description,
        TodoPriority priority = TodoPriority.Medium,
        DateTime? dueDate = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<TodoItem>(TodoErrors.TitleRequired);

        if (title.Length > 200)
            return Result.Failure<TodoItem>(TodoErrors.TitleTooLong);

        if (description.Length > 2000)
            return Result.Failure<TodoItem>(TodoErrors.DescriptionTooLong);

        if (dueDate.HasValue && dueDate < DateTime.UtcNow.Date)
            return Result.Failure<TodoItem>(TodoErrors.DueDateInPast);

        var todoId = TodoId.New();
        var todo = new TodoItem(todoId, userId, title.Trim(), description.Trim(), priority, dueDate);

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
