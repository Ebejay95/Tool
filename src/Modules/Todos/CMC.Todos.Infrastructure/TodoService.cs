using CMC.Persistence;
using CMC.Todos.Application;
using CMC.Todos.Domain;
using Microsoft.EntityFrameworkCore;

namespace CMC.Todos.Infrastructure;

public sealed class TodoService(ApplicationDbContext dbContext) : ITodoService
{
    public async Task<IReadOnlyList<TodoDto>> GetForUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        return await dbContext.Todos
            .AsNoTracking()
            .Where(t => t.OwnerUserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TodoDto(t.Id, t.Title, t.IsCompleted, t.CreatedAt, t.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<TodoDto> CreateAsync(string userId, string title, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        var now = DateTimeOffset.UtcNow;

        var entity = new TodoItem
        {
            Id = Guid.NewGuid(),
            OwnerUserId = userId,
            Title = title.Trim(),
            IsCompleted = false,
            CreatedAt = now,
            UpdatedAt = now,
        };

        dbContext.Todos.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new TodoDto(entity.Id, entity.Title, entity.IsCompleted, entity.CreatedAt, entity.UpdatedAt);
    }

    public async Task<TodoDto> UpdateTitleAsync(string userId, Guid todoId, string title, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        var entity = await LoadOwnedTodoAsync(userId, todoId, cancellationToken);
        entity.Title = title.Trim();
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return new TodoDto(entity.Id, entity.Title, entity.IsCompleted, entity.CreatedAt, entity.UpdatedAt);
    }

    public async Task<TodoDto> SetCompletedAsync(string userId, Guid todoId, bool isCompleted, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var entity = await LoadOwnedTodoAsync(userId, todoId, cancellationToken);
        entity.IsCompleted = isCompleted;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return new TodoDto(entity.Id, entity.Title, entity.IsCompleted, entity.CreatedAt, entity.UpdatedAt);
    }

    public async Task DeleteAsync(string userId, Guid todoId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var entity = await LoadOwnedTodoAsync(userId, todoId, cancellationToken);
        dbContext.Todos.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<TodoItem> LoadOwnedTodoAsync(string userId, Guid todoId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Todos.FirstOrDefaultAsync(t => t.Id == todoId && t.OwnerUserId == userId, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException("Todo not found.");
        }

        return entity;
    }
}
