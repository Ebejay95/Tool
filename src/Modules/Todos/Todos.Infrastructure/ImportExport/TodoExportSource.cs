using ImportExport.Application.UseCases;
using SharedKernel;
using Todos.Application.Ports;
using Todos.Domain.Todos;

namespace Todos.Infrastructure.ImportExport;

/// <summary>Stellt alle Todo-Entitäten eines Users für den Export bereit.</summary>
public sealed class TodoExportSource : IExportSource
{
    private readonly ITodoRepository _repository;

    public TodoExportSource(ITodoRepository repository) => _repository = repository;

    public string EntityTypeName => Todo.ExportableTypeName;

    public async Task<IReadOnlyList<object>> GetAllForUserAsync(UserId userId, CancellationToken ct = default)
    {
        var todos = await _repository.GetByUserIdAsync(userId, ct);
        return todos.Cast<object>().ToList();
    }
}
