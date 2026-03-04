using ImportExport.Application.UseCases;
using SharedKernel;
using Todos.Application.Ports;
using Todos.Domain.Todos;

namespace Todos.Infrastructure.ImportExport;

/// <summary>
/// Erstellt neue Todo-Entitäten aus einer Import-Zeile.
/// Owner = aktueller User (Default-CRUD gemäß Anforderung).
/// </summary>
public sealed class TodoImportAdapter : IImportAdapter
{
    private readonly ITodoRepository       _repository;
    private readonly ITodosUnitOfWork      _unitOfWork;

    public TodoImportAdapter(ITodoRepository repository, ITodosUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public string EntityTypeName => Todo.ExportableTypeName;

    public async Task<string?> ImportRowAsync(
        UserId                              ownerId,
        IReadOnlyDictionary<string, string?> row,
        CancellationToken                   ct = default)
    {
        var title       = row.GetValueOrDefault("Title")?.Trim();
        var description = row.GetValueOrDefault("Description")?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(title))
            return "Pflichtfeld 'Title' (Titel) fehlt oder ist leer.";

        var priority = TodoPriority.Medium;
        if (row.TryGetValue("Priority", out var priorityStr) && !string.IsNullOrWhiteSpace(priorityStr))
        {
            if (!Enum.TryParse<TodoPriority>(priorityStr, ignoreCase: true, out var parsedPriority))
                return $"Ungültiger Prioritätswert: '{priorityStr}'. Erlaubt: Low, Medium, High, Critical.";
            priority = parsedPriority;
        }

        DateTime? dueDate = null;
        if (row.TryGetValue("DueDate", out var dueDateStr) && !string.IsNullOrWhiteSpace(dueDateStr))
        {
            if (!DateTime.TryParse(dueDateStr, out var parsedDate))
                return $"Ungültiges Datumsformat für DueDate: '{dueDateStr}'.";
            dueDate = parsedDate.ToUniversalTime();
        }

        var result = Todo.Create(ownerId, title, description, priority, dueDate);
        if (result.IsFailure)
            return result.Error.Description;

        _repository.Add(result.Value);
        await _unitOfWork.SaveChangesAsync(ct);
        return null;
    }
}
