using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Taxonomy.Application.DTOs;
using Taxonomy.Application.UseCases.Commands;
using Taxonomy.Application.UseCases.Queries;
using Taxonomy.Domain.Categories;

namespace Taxonomy.Api;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class CategoriesController(IMediator mediator, ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null) return Unauthorized();

        var result = await mediator.Send(new GetAllCategoriesQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCategory(Guid id, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null) return Unauthorized();

        var result = await mediator.Send(new GetCategoryByIdQuery(CategoryId.From(id)), cancellationToken);

        if (result.IsFailure)
            return result.Error.Code == "General.NotFound" ? NotFound() : BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null) return Unauthorized();

        var result = await mediator.Send(new CreateCategoryCommand(dto), cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });

        return CreatedAtAction(nameof(GetCategory), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryDto dto, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null) return Unauthorized();

        var result = await mediator.Send(new UpdateCategoryCommand(CategoryId.From(id), dto), cancellationToken);

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

        var result = await mediator.Send(new DeleteCategoryCommand(CategoryId.From(id)), cancellationToken);

        if (result.IsFailure)
            return result.Error.Code is "Category.NotFound" or "General.NotFound"
                ? NotFound()
                : BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });

        return Ok(new { Message = "Kategorie erfolgreich geloescht." });
    }
}
