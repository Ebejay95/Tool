using Todos.Application.DTOs;
using Riok.Mapperly.Abstractions;

namespace App.ViewModels;

[Mapper]
public static partial class TodoMappings
{
    // string → Guid Konverter: wird von Mapperly für das Id-Feld verwendet
    private static Guid ParseGuid(string id) => Guid.Parse(id);

    // ── DTO → ViewModel ─────────────────────────────────────────────────────
    // Extra DTO-Felder (UserId, UpdatedAt, CompletedAt) werden von Mapperly ignoriert.

    public static partial TodoListItemViewModel ToViewModel(this TodoDto dto);

    public static List<TodoListItemViewModel> ToViewModels(this IEnumerable<TodoDto> dtos)
        => dtos.Select(ToViewModel).ToList();

    // ── ViewModel → DTO ─────────────────────────────────────────────────────
    // SelectedCategoryIds/SelectedTagIds im ViewModel → CategoryIds/TagIds im DTO

    [MapProperty(nameof(CreateTodoViewModel.SelectedCategoryIds), nameof(CreateTodoDto.CategoryIds))]
    [MapProperty(nameof(CreateTodoViewModel.SelectedTagIds), nameof(CreateTodoDto.TagIds))]
    public static partial CreateTodoDto ToDto(this CreateTodoViewModel vm);
}
