using SharedKernel;
using Todos.Application.DTOs;
using Todos.Application.Ports;
using Todos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using DomStatus = Todos.Domain.TodoItems.TodoStatus;

namespace Todos.Infrastructure.Services;

public sealed class TodoQueryService : ITodoQueryService
{
    private readonly TodosDbContext _context;

    public TodoQueryService(TodosDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<TodoDto>> GetUserTodosAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var entities = await _context.Todos
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities
            .Select(t => new TodoDto(
                t.Id.Value.ToString(),
                t.UserId.Value.ToString(),
                t.Title,
                t.Description,
                TodoEnumMapper.MapToDto(t.Status),
                TodoEnumMapper.MapToDto(t.Priority),
                t.DueDate,
                t.CreatedAt,
                t.UpdatedAt,
                t.CompletedAt,
                t.DueDate.HasValue && t.DueDate < DateTimeOffset.UtcNow && t.Status != DomStatus.Completed))
            .ToList();
    }

    public async Task<IReadOnlyList<TodoSummaryDto>> GetUserTodoSummariesAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var summaries = await _context.Todos
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        return summaries
            .Select(t => new TodoSummaryDto(
                t.Id.Value.ToString(),
                t.Title,
                TodoEnumMapper.MapToDto(t.Status),
                TodoEnumMapper.MapToDto(t.Priority),
                t.DueDate,
                t.DueDate.HasValue && t.DueDate < DateTimeOffset.UtcNow && t.Status != DomStatus.Completed))
            .ToList();
    }

    public async Task<TodoDto?> GetUserTodoByIdAsync(TodoId todoId, UserId userId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Todos
            .Where(t => t.Id == todoId && t.UserId == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null) return null;

        return new TodoDto(
            entity.Id.Value.ToString(),
            entity.UserId.Value.ToString(),
            entity.Title,
            entity.Description,
            TodoEnumMapper.MapToDto(entity.Status),
            TodoEnumMapper.MapToDto(entity.Priority),
            entity.DueDate,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.CompletedAt,
            entity.DueDate.HasValue && entity.DueDate < DateTimeOffset.UtcNow && entity.Status != DomStatus.Completed);
    }

    public async Task<IReadOnlyList<TodoDto>> GetOverdueUserTodosAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var entities = await _context.Todos
            .Where(t => t.UserId == userId &&
                       t.DueDate.HasValue &&
                       t.DueDate < now &&
                       t.Status != DomStatus.Completed)
            .OrderBy(t => t.DueDate)
            .ToListAsync(cancellationToken);

        return entities
            .Select(t => new TodoDto(
                t.Id.Value.ToString(),
                t.UserId.Value.ToString(),
                t.Title,
                t.Description,
                TodoEnumMapper.MapToDto(t.Status),
                TodoEnumMapper.MapToDto(t.Priority),
                t.DueDate,
                t.CreatedAt,
                t.UpdatedAt,
                t.CompletedAt,
                true)) // IsOverdue is always true for this query
            .ToList();
    }
}
