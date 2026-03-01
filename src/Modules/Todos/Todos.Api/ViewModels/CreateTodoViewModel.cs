using System.ComponentModel.DataAnnotations;
using Todos.Application.DTOs;

namespace Todos.Api.ViewModels;

/// <summary>
/// ViewModel für das "Neue Aufgabe erstellen"-Formular.
/// Enthält Validierungsattribute und ist unabhängig von DTOs/Domain-Typen.
/// </summary>
public sealed class CreateTodoViewModel
{
    [Required(ErrorMessage = "Titel ist erforderlich.")]
    [MaxLength(200, ErrorMessage = "Titel darf max. 200 Zeichen lang sein.")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "Beschreibung darf max. 2000 Zeichen lang sein.")]
    public string Description { get; set; } = string.Empty;

    public TodoPriority Priority { get; set; } = TodoPriority.Medium;

    public DateTime? DueDate { get; set; }
}
