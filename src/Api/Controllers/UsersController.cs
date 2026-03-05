using Identity.Application.DTOs;
using Identity.Application.UseCases.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Nutzerverwaltung – ausschließlich für den Master-User.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "master")]
public sealed class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Gibt alle Nutzer zurück (inkl. inaktive).</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUsersQuery(), cancellationToken);
        if (result.IsFailure)
            return BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });
        return Ok(result.Value);
    }

    /// <summary>Setzt die Rolle eines Nutzers (user / admin / super-admin).</summary>
    [HttpPut("{id:guid}/role")]
    public async Task<IActionResult> UpdateRole(Guid id, UpdateUserRoleDto dto, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateUserRoleCommand(id, dto.Role), cancellationToken);
        if (result.IsFailure)
            return BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });
        return NoContent();
    }
}
