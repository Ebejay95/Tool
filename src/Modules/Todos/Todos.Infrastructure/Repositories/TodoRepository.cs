using SharedKernel;
using Todos.Application.Ports;
using Todos.Domain.Todos;
using Todos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Todos.Infrastructure.Repositories;

public sealed class TodoRepository : ITodoRepository
{
    private readonly TodosDbContext _context;

    public TodoRepository(TodosDbContext context)
    {
        _context = context;
    }

    public async Task<Todo?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Todos
            .FirstOrDefaultAsync(t => t.Id == TodoId.From(id), cancellationToken);
    }

    public async Task<Todo?> GetByIdAndUserIdAsync(TodoId todoId, UserId userId, CancellationToken cancellationToken = default)
    {
        return await _context.Todos
            .FirstOrDefaultAsync(t => t.Id == todoId && t.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<Todo>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        return await _context.Todos
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Todo>> GetOverdueByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Todos
            .Where(t => t.UserId == userId &&
                       t.DueDate.HasValue &&
                       t.DueDate < now &&
                       t.Status != TodoStatus.Completed)
            .OrderBy(t => t.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Todo>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Todos
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public void Add(Todo entity)
    {
        _context.Todos.Add(entity);
    }

    public void Update(Todo entity)
    {
        _context.Todos.Update(entity);
    }

    public void Remove(Todo entity)
    {
        _context.Todos.Remove(entity);
    }
}
