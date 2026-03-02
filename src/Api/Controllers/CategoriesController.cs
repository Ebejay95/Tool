using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Taxonomy.Application.DTOs;
using Taxonomy.Application.UseCases.Commands;
using Taxonomy.Application.UseCases.Queries;
using Taxonomy.Domain.Categories;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class CategoriesController(IMediator mediator, ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null) return Unauthorized();

        var query  = new GetAccessibleCategoriesQuery(currentUser.UserId);
        var result = await mediator.Send(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCategory(Guid id, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null) return Unauthorized();

        var query  = new GetCategoryByIdQuery(currentUser.UserId, CategoryId.From(id));
        var result = await mediator.Send(query, cancellationToken);

        if (result.IsFailure)
            return result.Error.Code == "General.NotFound" ? NotFound() : BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null) return Unauthorized();

        var command = new CreateCategoryCommand(currentUser.UserId, dto);
        var result  = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });

        return CreatedAtAction(nameof(GetCategory), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryDto dto, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null) return Unauthorized();

        var command = new UpdateCategoryCommand(currentUser.UserId, CategoryId.From(id), dto);
        var result  = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return result.Error.Code is "Category.NotFound" or "General.NotFound"
                ? NotFound()
                : BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null) return Unauthorized();

        var command = new DeleteCategoryCommand(currentUser.UserId, CategoryId.From(id));
        var result  = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return result.Error.Code is "Category.NotFound" or "General.NotFound"
                ? NotFound()
                : BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });

        return Ok(new { Message = "Kategorie erfolgreich gelöscht." });
    }
}
