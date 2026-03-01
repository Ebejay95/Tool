using CMC.SharedKernel;
using CMC.Todos.Application.Ports;
using CMC.Todos.Domain.TodoItems;
using MediatR;

namespace CMC.Todos.Application.UseCases.Commands;

public sealed record CompleteTodoCommand(UserId UserId, TodoId TodoId) : Command;

public sealed class CompleteTodoHandler : IRequestHandler<CompleteTodoCommand, Result>
{
    private readonly ITodoRepository _todoRepository;
    private readonly ITodosUnitOfWork _unitOfWork;

    public CompleteTodoHandler(ITodoRepository todoRepository, ITodosUnitOfWork unitOfWork)
    {
        _todoRepository = todoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CompleteTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await _todoRepository.GetByIdAndUserIdAsync(request.TodoId, request.UserId, cancellationToken);
        if (todo == null)
            return Result.Failure(TodoErrors.NotFound);

        var result = todo.MarkAsCompleted();
        if (result.IsFailure)
            return result;

        _todoRepository.Update(todo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public sealed record StartTodoCommand(UserId UserId, TodoId TodoId) : Command;

public sealed class StartTodoHandler : IRequestHandler<StartTodoCommand, Result>
{
    private readonly ITodoRepository _todoRepository;
    private readonly ITodosUnitOfWork _unitOfWork;

    public StartTodoHandler(ITodoRepository todoRepository, ITodosUnitOfWork unitOfWork)
    {
        _todoRepository = todoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(StartTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await _todoRepository.GetByIdAndUserIdAsync(request.TodoId, request.UserId, cancellationToken);
        if (todo == null)
            return Result.Failure(TodoErrors.NotFound);

        var result = todo.MarkAsInProgress();
        if (result.IsFailure)
            return result;

        _todoRepository.Update(todo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public sealed record CancelTodoCommand(UserId UserId, TodoId TodoId) : Command;

public sealed class CancelTodoHandler : IRequestHandler<CancelTodoCommand, Result>
{
    private readonly ITodoRepository _todoRepository;
    private readonly ITodosUnitOfWork _unitOfWork;

    public CancelTodoHandler(ITodoRepository todoRepository, ITodosUnitOfWork unitOfWork)
    {
        _todoRepository = todoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CancelTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await _todoRepository.GetByIdAndUserIdAsync(request.TodoId, request.UserId, cancellationToken);
        if (todo == null)
            return Result.Failure(TodoErrors.NotFound);

        var result = todo.Cancel();
        if (result.IsFailure)
            return result;

        _todoRepository.Update(todo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public sealed record DeleteTodoCommand(UserId UserId, TodoId TodoId) : Command;

public sealed class DeleteTodoHandler : IRequestHandler<DeleteTodoCommand, Result>
{
    private readonly ITodoRepository _todoRepository;
    private readonly ITodosUnitOfWork _unitOfWork;

    public DeleteTodoHandler(ITodoRepository todoRepository, ITodosUnitOfWork unitOfWork)
    {
        _todoRepository = todoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await _todoRepository.GetByIdAndUserIdAsync(request.TodoId, request.UserId, cancellationToken);
        if (todo == null)
            return Result.Failure(TodoErrors.NotFound);

        // Mark for deletion to emit domain event
        todo.MarkForDeletion();

        _todoRepository.Remove(todo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
