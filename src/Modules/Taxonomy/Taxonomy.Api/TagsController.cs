using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Taxonomy.Application.DTOs;
using Taxonomy.Application.UseCases.Commands;
using Taxonomy.Application.UseCases.Queries;
using Taxonomy.Domain.Tags;

namespace Taxonomy.Api;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class TagsController(IMediator mediator, ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTags(CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null) return Unauthorized();

        var query  = new GetAccessibleTagsQuery(currentUser.UserId);
        var result = await mediator.Send(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTag(Guid id, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null) return Unauthorized();

        var query  = new GetTagByIdQuery(currentUser.UserId, TagId.From(id));
        var result = await mediator.Send(query, cancellationToken);

        if (result.IsFailure)
            return result.Error.Code == "General.NotFound" ? NotFound() : BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTag([FromBody] CreateTagDto dto, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null) return Unauthorized();

        var command = new CreateTagCommand(currentUser.UserId, dto);
        var result  = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });

        return CreatedAtAction(nameof(GetTag), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateTag(Guid id, [FromBody] UpdateTagDto dto, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null) return Unauthorized();

        var command = new UpdateTagCommand(currentUser.UserId, TagId.From(id), dto);
        var result  = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return result.Error.Code is "Tag.NotFound" or "General.NotFound"
                ? NotFound()
                : BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTag(Guid id, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null) return Unauthorized();

        var command = new DeleteTagCommand(currentUser.UserId, TagId.From(id));
        var result  = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return result.Error.Code is "Tag.NotFound" or "General.NotFound"
                ? NotFound()
                : BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });

        return Ok(new { Message = "Tag erfolgreich gelöscht." });
    }
}
