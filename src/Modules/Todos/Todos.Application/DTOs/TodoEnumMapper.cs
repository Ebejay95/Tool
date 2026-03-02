using DomTodos = Todos.Domain.Todos;

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
    public static TodoStatus MapToDto(DomTodos.TodoStatus status) => status switch
    {
        DomTodos.TodoStatus.Pending    => TodoStatus.Pending,
        DomTodos.TodoStatus.InProgress => TodoStatus.InProgress,
        DomTodos.TodoStatus.Completed  => TodoStatus.Completed,
        DomTodos.TodoStatus.Cancelled  => TodoStatus.Cancelled,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status,
                 $"Unbekannter TodoStatus-Wert: {status}. DTO-Enum muss aktualisiert werden.")
    };

    public static TodoPriority MapToDto(DomTodos.TodoPriority priority) => priority switch
    {
        DomTodos.TodoPriority.Low      => TodoPriority.Low,
        DomTodos.TodoPriority.Medium   => TodoPriority.Medium,
        DomTodos.TodoPriority.High     => TodoPriority.High,
        DomTodos.TodoPriority.Critical => TodoPriority.Critical,
        _ => throw new ArgumentOutOfRangeException(nameof(priority), priority,
                 $"Unbekannter TodoPriority-Wert: {priority}. DTO-Enum muss aktualisiert werden.")
    };

    public static DomTodos.TodoPriority MapToDomain(TodoPriority priority) => priority switch
    {
        TodoPriority.Low      => DomTodos.TodoPriority.Low,
        TodoPriority.Medium   => DomTodos.TodoPriority.Medium,
        TodoPriority.High     => DomTodos.TodoPriority.High,
        TodoPriority.Critical => DomTodos.TodoPriority.Critical,
        _ => throw new ArgumentOutOfRangeException(nameof(priority), priority,
                 $"Unbekannter TodoPriority-Wert: {priority}. Domain-Enum muss aktualisiert werden.")
    };
}
