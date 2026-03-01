using Todos.Application.DTOs;

namespace Todos.Api.ViewModels;

/// <summary>
/// Schreibgeschütztes ViewModel für eine einzelne Aufgabe in der Listenansicht.
/// Enthält bereits alle UI-seitigen Darstellungsinfos (Labels, Farben etc.).
/// </summary>
public sealed record TodoListItemViewModel(
    Guid Id,
    string Title,
    string Description,
    TodoStatus Status,
    TodoPriority Priority,
    DateTime CreatedAt,
    DateTime? DueDate,
    bool IsOverdue)
{
    // ── UI-Helpers (kein Razor-Code nötig, testbar) ──────────────────────────

    public string StatusLabel => Status switch
    {
        TodoStatus.Pending    => "Offen",
        TodoStatus.InProgress => "In Bearbeitung",
        TodoStatus.Completed  => "Erledigt",
        TodoStatus.Cancelled  => "Abgebrochen",
        _                     => Status.ToString()
    };

    public string PriorityLabel => Priority switch
    {
        TodoPriority.Low      => "Niedrig",
        TodoPriority.Medium   => "Mittel",
        TodoPriority.High     => "Hoch",
        TodoPriority.Critical => "Kritisch",
        _                     => Priority.ToString()
    };

    public bool IsCompleted => Status == TodoStatus.Completed;
}
