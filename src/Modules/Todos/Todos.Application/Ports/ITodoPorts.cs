using SharedKernel;
using Todos.Application.DTOs;
using Todos.Domain.Todos;

namespace Todos.Application.Ports;

public interface ITodoRepository : IRepository<Todo>
{
    Task<IReadOnlyList<Todo>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<Todo?> GetByIdAndUserIdAsync(TodoId todoId, UserId userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Todo>> GetOverdueByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
}

public interface ITodoQueryService
{
    Task<IReadOnlyList<TodoDto>> GetUserTodosAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TodoSummaryDto>> GetUserTodoSummariesAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<TodoDto?> GetUserTodoByIdAsync(TodoId todoId, UserId userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TodoDto>> GetOverdueUserTodosAsync(UserId userId, CancellationToken cancellationToken = default);
}
