using Todos.Application.DTOs;

namespace App.ViewModels;

/// <summary>
/// Mapping-Methoden zwischen DTOs (Application-Layer) und ViewModels (Web-Layer).
/// Hält den Blazor-Code sauber – keine DTO-Imports in .razor-Dateien nötig.
/// </summary>
public static class TodoMappings
{
    // ── DTO → ViewModel ─────────────────────────────────────────────────────

    public static TodoListItemViewModel ToViewModel(this TodoDto dto) =>
        new(
            Id:          Guid.Parse(dto.Id),
            Title:       dto.Title,
            Description: dto.Description,
            Status:      dto.Status,
            Priority:    dto.Priority,
            CreatedAt:   dto.CreatedAt,
            DueDate:     dto.DueDate,
            IsOverdue:   dto.IsOverdue
        );

    public static List<TodoListItemViewModel> ToViewModels(this IEnumerable<TodoDto> dtos) =>
        dtos.Select(ToViewModel).ToList();

    // ── ViewModel → DTO ─────────────────────────────────────────────────────

    public static CreateTodoDto ToDto(this CreateTodoViewModel vm) =>
        new()
        {
            Title       = vm.Title,
            Description = vm.Description,
            Priority    = vm.Priority,
            DueDate     = vm.DueDate
        };
}
