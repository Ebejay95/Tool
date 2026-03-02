using Measures.Application.DTOs;
using Measures.Application.UseCases.Commands;
using Measures.Application.UseCases.Queries;
using Measures.Domain.Measures;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class MeasuresController(IMediator mediator, ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMeasures(CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null) return Unauthorized();

        var query = new GetUserMeasuresQuery(currentUser.UserId);
        var result = await mediator.Send(query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetMeasure(Guid id, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null) return Unauthorized();

        var query = new GetUserMeasureByIdQuery(currentUser.UserId, MeasureId.From(id));
        var result = await mediator.Send(query, cancellationToken);

        if (result.IsFailure)
            return result.Error.Code == "General.NotFound"
                ? NotFound()
                : BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMeasure([FromBody] CreateMeasureDto dto, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null) return Unauthorized();

        var command = new CreateMeasureCommand(currentUser.UserId, dto);
        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });

        return CreatedAtAction(nameof(GetMeasure), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateMeasure(Guid id, [FromBody] UpdateMeasureDto dto, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null) return Unauthorized();

        var command = new UpdateMeasureCommand(currentUser.UserId, MeasureId.From(id), dto);
        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return result.Error.Code == "Measure.NotFound"
                ? NotFound()
                : BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteMeasure(Guid id, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null) return Unauthorized();

        var command = new DeleteMeasureCommand(currentUser.UserId, MeasureId.From(id));
        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return result.Error.Code == "Measure.NotFound"
                ? NotFound()
                : BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });

        return Ok(new { Message = "Measure deleted successfully." });
    }
}
