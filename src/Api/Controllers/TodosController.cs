using SharedKernel;
using Todos.Application.DTOs;
using Todos.Application.UseCases.Commands;
using Todos.Application.UseCases.Queries;
using Todos.Domain.TodoItems;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class TodosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;

    public TodosController(IMediator mediator, ICurrentUser currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetTodos(CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Unauthorized();

        var query = new GetUserTodosQuery(_currentUser.UserId);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });
        }

        return Ok(result.Value);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTodo(Guid id, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Unauthorized();

        var query = new GetUserTodoByIdQuery(_currentUser.UserId, TodoId.From(id));
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code == "General.NotFound" ? NotFound() : BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });
        }

        return Ok(result.Value);
    }

    [HttpGet("overdue")]
    public async Task<IActionResult> GetOverdueTodos(CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Unauthorized();

        var query = new GetOverdueTodosQuery(_currentUser.UserId);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });
        }

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTodo(CreateTodoDto dto, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Unauthorized();

        var command = new CreateTodoCommand(_currentUser.UserId, dto);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });
        }

        return CreatedAtAction(nameof(GetTodo), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTodo(Guid id, UpdateTodoDto dto, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Unauthorized();

        var command = new UpdateTodoCommand(_currentUser.UserId, TodoId.From(id), dto);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code == "Todo.NotFound" ? NotFound() : BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });
        }

        return Ok(result.Value);
    }

    [HttpPost("{id}/complete")]
    public async Task<IActionResult> CompleteTodo(Guid id, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Unauthorized();

        var command = new CompleteTodoCommand(_currentUser.UserId, TodoId.From(id));
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code == "Todo.NotFound" ? NotFound() : BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });
        }

        return Ok(new { Message = "Todo completed successfully." });
    }

    [HttpPost("{id}/start")]
    public async Task<IActionResult> StartTodo(Guid id, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Unauthorized();

        var command = new StartTodoCommand(_currentUser.UserId, TodoId.From(id));
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code == "Todo.NotFound" ? NotFound() : BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });
        }

        return Ok(new { Message = "Todo started successfully." });
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelTodo(Guid id, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Unauthorized();

        var command = new CancelTodoCommand(_currentUser.UserId, TodoId.From(id));
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code == "Todo.NotFound" ? NotFound() : BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });
        }

        return Ok(new { Message = "Todo cancelled successfully." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTodo(Guid id, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Unauthorized();

        var command = new DeleteTodoCommand(_currentUser.UserId, TodoId.From(id));
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code == "Todo.NotFound" ? NotFound() : BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });
        }

        return Ok(new { Message = "Todo deleted successfully." });
    }
}
