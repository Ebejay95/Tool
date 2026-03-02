namespace Todos.Application.DTOs;

public sealed class CreateTodoDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TodoPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public List<Guid> CategoryIds { get; set; } = [];
    public List<Guid> TagIds { get; set; } = [];
}

public sealed class UpdateTodoDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TodoPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public List<Guid> CategoryIds { get; set; } = [];
    public List<Guid> TagIds { get; set; } = [];
}

public sealed class UpdateTodoStatusDto
{
    public TodoStatus Status { get; set; }
}

public sealed record TodoDto(
    string Id,
    string UserId,
    string Title,
    string Description,
    TodoStatus Status,
    TodoPriority Priority,
    DateTime? DueDate,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? CompletedAt,
    bool IsOverdue,
    List<Guid> CategoryIds,
    List<Guid> TagIds);

public sealed record TodoSummaryDto(
    string Id,
    string Title,
    TodoStatus Status,
    TodoPriority Priority,
    DateTime? DueDate,
    bool IsOverdue);

public sealed class TodoStatsDto
{
    public int TotalTodos { get; set; }
    public int PendingTodos { get; set; }
    public int InProgressTodos { get; set; }
    public int CompletedTodos { get; set; }
    public int OverdueTodos { get; set; }
}
