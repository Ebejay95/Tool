using CMC.SharedKernel;
using CMC.Todos.Application.DTOs;
using CMC.Todos.Application.Ports;
using CMC.Todos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CMC.Todos.Infrastructure.Services;

public sealed class TodoQueryService : ITodoQueryService
{
    private readonly TodosDbContext _context;

    public TodoQueryService(TodosDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<TodoDto>> GetUserTodosAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var todos = await _context.Todos
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TodoDto(
                t.Id.Value.ToString(),
                t.UserId.Value.ToString(),
                t.Title,
                t.Description,
                t.Status,
                t.Priority,
                t.DueDate,
                t.CreatedAt,
                t.UpdatedAt,
                t.CompletedAt,
                t.DueDate.HasValue && t.DueDate < DateTime.UtcNow && t.Status != Domain.TodoItems.TodoStatus.Completed))
            .ToListAsync(cancellationToken);

        return todos;
    }

    public async Task<IReadOnlyList<TodoSummaryDto>> GetUserTodoSummariesAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var summaries = await _context.Todos
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TodoSummaryDto(
                t.Id.Value.ToString(),
                t.Title,
                t.Status,
                t.Priority,
                t.DueDate,
                t.DueDate.HasValue && t.DueDate < DateTime.UtcNow && t.Status != Domain.TodoItems.TodoStatus.Completed))
            .ToListAsync(cancellationToken);

        return summaries;
    }

    public async Task<TodoDto?> GetUserTodoByIdAsync(TodoId todoId, UserId userId, CancellationToken cancellationToken = default)
    {
        var todo = await _context.Todos
            .Where(t => t.Id == todoId && t.UserId == userId)
            .Select(t => new TodoDto(
                t.Id.Value.ToString(),
                t.UserId.Value.ToString(),
                t.Title,
                t.Description,
                t.Status,
                t.Priority,
                t.DueDate,
                t.CreatedAt,
                t.UpdatedAt,
                t.CompletedAt,
                t.DueDate.HasValue && t.DueDate < DateTime.UtcNow && t.Status != Domain.TodoItems.TodoStatus.Completed))
            .FirstOrDefaultAsync(cancellationToken);

        return todo;
    }

    public async Task<IReadOnlyList<TodoDto>> GetOverdueUserTodosAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var overdueTodos = await _context.Todos
            .Where(t => t.UserId == userId &&
                       t.DueDate.HasValue &&
                       t.DueDate < now &&
                       t.Status != Domain.TodoItems.TodoStatus.Completed)
            .OrderBy(t => t.DueDate)
            .Select(t => new TodoDto(
                t.Id.Value.ToString(),
                t.UserId.Value.ToString(),
                t.Title,
                t.Description,
                t.Status,
                t.Priority,
                t.DueDate,
                t.CreatedAt,
                t.UpdatedAt,
                t.CompletedAt,
                true)) // IsOverdue is always true for this query
            .ToListAsync(cancellationToken);

        return overdueTodos;
    }
}
