namespace CMC.Todos.Application;

public interface ITodoService
{
    Task<IReadOnlyList<TodoDto>> GetForUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<TodoDto> CreateAsync(string userId, string title, CancellationToken cancellationToken = default);
    Task<TodoDto> UpdateTitleAsync(string userId, Guid todoId, string title, CancellationToken cancellationToken = default);
    Task<TodoDto> SetCompletedAsync(string userId, Guid todoId, bool isCompleted, CancellationToken cancellationToken = default);
    Task DeleteAsync(string userId, Guid todoId, CancellationToken cancellationToken = default);
}
