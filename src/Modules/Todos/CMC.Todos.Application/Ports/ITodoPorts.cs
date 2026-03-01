using CMC.SharedKernel;
using CMC.Todos.Application.DTOs;
using CMC.Todos.Domain.TodoItems;

namespace CMC.Todos.Application.Ports;

public interface ITodoRepository : IRepository<TodoItem>
{
    Task<IReadOnlyList<TodoItem>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<TodoItem?> GetByIdAndUserIdAsync(TodoId todoId, UserId userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TodoItem>> GetOverdueByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
}

public interface ITodoQueryService
{
    Task<IReadOnlyList<TodoDto>> GetUserTodosAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TodoSummaryDto>> GetUserTodoSummariesAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<TodoDto?> GetUserTodoByIdAsync(TodoId todoId, UserId userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TodoDto>> GetOverdueUserTodosAsync(UserId userId, CancellationToken cancellationToken = default);
}
