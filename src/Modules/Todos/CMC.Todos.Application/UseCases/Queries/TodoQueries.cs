using CMC.SharedKernel;
using CMC.Todos.Application.DTOs;
using CMC.Todos.Application.Ports;
using MediatR;

namespace CMC.Todos.Application.UseCases.Queries;

public sealed record GetUserTodosQuery(UserId UserId) : Query<IReadOnlyList<TodoDto>>;

public sealed class GetUserTodosHandler : IRequestHandler<GetUserTodosQuery, Result<IReadOnlyList<TodoDto>>>
{
    private readonly ITodoQueryService _queryService;

    public GetUserTodosHandler(ITodoQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<Result<IReadOnlyList<TodoDto>>> Handle(GetUserTodosQuery request, CancellationToken cancellationToken)
    {
        var todos = await _queryService.GetUserTodosAsync(request.UserId, cancellationToken);
        return Result.Success(todos);
    }
}

public sealed record GetUserTodoByIdQuery(UserId UserId, TodoId TodoId) : Query<TodoDto>;

public sealed class GetUserTodoByIdHandler : IRequestHandler<GetUserTodoByIdQuery, Result<TodoDto>>
{
    private readonly ITodoQueryService _queryService;

    public GetUserTodoByIdHandler(ITodoQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<Result<TodoDto>> Handle(GetUserTodoByIdQuery request, CancellationToken cancellationToken)
    {
        var todo = await _queryService.GetUserTodoByIdAsync(request.TodoId, request.UserId, cancellationToken);

        if (todo == null)
            return Result.Failure<TodoDto>(Errors.General.NotFound);

        return Result.Success(todo);
    }
}

public sealed record GetOverdueTodosQuery(UserId UserId) : Query<IReadOnlyList<TodoDto>>;

public sealed class GetOverdueTodosHandler : IRequestHandler<GetOverdueTodosQuery, Result<IReadOnlyList<TodoDto>>>
{
    private readonly ITodoQueryService _queryService;

    public GetOverdueTodosHandler(ITodoQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<Result<IReadOnlyList<TodoDto>>> Handle(GetOverdueTodosQuery request, CancellationToken cancellationToken)
    {
        var todos = await _queryService.GetOverdueUserTodosAsync(request.UserId, cancellationToken);
        return Result.Success(todos);
    }
}
