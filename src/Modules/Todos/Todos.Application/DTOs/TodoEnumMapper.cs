using DomTodoItems = Todos.Domain.TodoItems;

namespace Todos.Application.DTOs;

/// <summary>
/// Explizite Switch-Mappings zwischen Domain-Enums und DTO-Enums.
///
/// Sicherer als int-Cast: gibt zur Laufzeit eine ArgumentOutOfRangeException,
/// wenn ein neuer Enum-Wert in der Domain ohne entsprechende DTO-Aktualisierung
/// hinzugefügt wird. Der int-Cast würde in diesem Fall still einen falschen Wert liefern.
/// </summary>
public static class TodoEnumMapper
{
    public static TodoStatus MapToDto(DomTodoItems.TodoStatus status) => status switch
    {
        DomTodoItems.TodoStatus.Pending    => TodoStatus.Pending,
        DomTodoItems.TodoStatus.InProgress => TodoStatus.InProgress,
        DomTodoItems.TodoStatus.Completed  => TodoStatus.Completed,
        DomTodoItems.TodoStatus.Cancelled  => TodoStatus.Cancelled,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status,
                 $"Unbekannter TodoStatus-Wert: {status}. DTO-Enum muss aktualisiert werden.")
    };

    public static TodoPriority MapToDto(DomTodoItems.TodoPriority priority) => priority switch
    {
        DomTodoItems.TodoPriority.Low      => TodoPriority.Low,
        DomTodoItems.TodoPriority.Medium   => TodoPriority.Medium,
        DomTodoItems.TodoPriority.High     => TodoPriority.High,
        DomTodoItems.TodoPriority.Critical => TodoPriority.Critical,
        _ => throw new ArgumentOutOfRangeException(nameof(priority), priority,
                 $"Unbekannter TodoPriority-Wert: {priority}. DTO-Enum muss aktualisiert werden.")
    };

    public static DomTodoItems.TodoPriority MapToDomain(TodoPriority priority) => priority switch
    {
        TodoPriority.Low      => DomTodoItems.TodoPriority.Low,
        TodoPriority.Medium   => DomTodoItems.TodoPriority.Medium,
        TodoPriority.High     => DomTodoItems.TodoPriority.High,
        TodoPriority.Critical => DomTodoItems.TodoPriority.Critical,
        _ => throw new ArgumentOutOfRangeException(nameof(priority), priority,
                 $"Unbekannter TodoPriority-Wert: {priority}. Domain-Enum muss aktualisiert werden.")
    };
}
