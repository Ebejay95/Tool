using SharedKernel;
using Todos.Application.DTOs;
using Todos.Application.Ports;
using Todos.Domain.TodoItems;
using MediatR;

namespace Todos.Application.UseCases.Commands;

public sealed record CreateTodoCommand(UserId UserId, CreateTodoDto Data) : Command<TodoDto>;

public sealed class CreateTodoHandler : IRequestHandler<CreateTodoCommand, Result<TodoDto>>
{
    private readonly ITodoRepository _todoRepository;
    private readonly ITodosUnitOfWork _unitOfWork;

    public CreateTodoHandler(ITodoRepository todoRepository, ITodosUnitOfWork unitOfWork)
    {
        _todoRepository = todoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TodoDto>> Handle(CreateTodoCommand request, CancellationToken cancellationToken)
    {
        var todoResult = TodoItem.Create(
            request.UserId,
            request.Data.Title,
            request.Data.Description,
            TodoEnumMapper.MapToDomain(request.Data.Priority),
            request.Data.DueDate);

        if (todoResult.IsFailure)
            return Result.Failure<TodoDto>(todoResult.Error);

        var todo = todoResult.Value;
        _todoRepository.Add(todo);

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
