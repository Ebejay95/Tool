using SharedKernel;
using Todos.Application.DTOs;
using Todos.Application.Ports;
using Todos.Domain.TodoItems;
using MediatR;

namespace Todos.Application.UseCases.Commands;

public sealed record UpdateTodoCommand(UserId UserId, TodoId TodoId, UpdateTodoDto Data) : Command<TodoDto>;

public sealed class UpdateTodoHandler : IRequestHandler<UpdateTodoCommand, Result<TodoDto>>
{
    private readonly ITodoRepository _todoRepository;
    private readonly ITodosUnitOfWork _unitOfWork;

    public UpdateTodoHandler(ITodoRepository todoRepository, ITodosUnitOfWork unitOfWork)
    {
        _todoRepository = todoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TodoDto>> Handle(UpdateTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await _todoRepository.GetByIdAndUserIdAsync(request.TodoId, request.UserId, cancellationToken);
        if (todo == null)
            return Result.Failure<TodoDto>(TodoErrors.NotFound);

        var updateResult = todo.Update(
            request.Data.Title,
            request.Data.Description,
            TodoEnumMapper.MapToDomain(request.Data.Priority),
            request.Data.DueDate);

        if (updateResult.IsFailure)
            return Result.Failure<TodoDto>(updateResult.Error);

        _todoRepository.Update(todo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var todoDto = new TodoDto(
            todo.Id.Value.ToString(),
            todo.UserId.Value.ToString(),
            todo.Title,
            todo.Description,
            TodoEnumMapper.MapToDto(todo.Status),
            TodoEnumMapper.MapToDto(todo.Priority),
            todo.DueDate,
            todo.CreatedAt,
            todo.UpdatedAt,
            todo.CompletedAt,
            todo.IsOverdue);

        return Result.Success(todoDto);
    }
}
