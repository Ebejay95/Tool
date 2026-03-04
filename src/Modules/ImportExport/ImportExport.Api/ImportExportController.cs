using SharedKernel;
using ImportExport.Application.UseCases;
using ImportExport.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ImportExport.Api;

[ApiController]
[Route("api/v1/import-export")]
[Authorize]
public sealed class ImportExportController : ControllerBase
{
    private readonly IMediator    _mediator;
    private readonly ICurrentUser _currentUser;

    public ImportExportController(IMediator mediator, ICurrentUser currentUser)
    {
        _mediator    = mediator;
        _currentUser = currentUser;
    }

    // ── Registry ─────────────────────────────────────────────────────────────

    [HttpGet("entities")]
    public async Task<IActionResult> GetExportableEntities(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetExportableEntitiesQuery(), ct);
        return result.IsFailure ? BadRequest(Problem(result)) : Ok(result.Value);
    }

    [HttpGet("channels")]
    public async Task<IActionResult> GetChannels(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetChannelsQuery(), ct);
        return result.IsFailure ? BadRequest(Problem(result)) : Ok(result.Value);
    }

    // ── Mapping Profiles ─────────────────────────────────────────────────────

    [HttpGet("mappings")]
    public async Task<IActionResult> GetMappingProfiles(
        [FromQuery] string? entityTypeName,
        CancellationToken ct)
    {
        if (_currentUser.UserId is null) return Unauthorized();

        var query  = new GetMappingProfilesQuery(_currentUser.UserId, entityTypeName);
        var result = await _mediator.Send(query, ct);
        return result.IsFailure ? BadRequest(Problem(result)) : Ok(result.Value);
    }

    [HttpPost("mappings")]
    public async Task<IActionResult> CreateMappingProfile(
        CreateMappingProfileRequest dto,
        CancellationToken ct)
    {
        if (_currentUser.UserId is null) return Unauthorized();

        var cmd    = new CreateMappingProfileCommand(_currentUser.UserId, dto);
        var result = await _mediator.Send(cmd, ct);
        return result.IsFailure
            ? BadRequest(Problem(result))
            : CreatedAtAction(nameof(GetMappingProfiles), result.Value);
    }

    [HttpPut("mappings/{id:guid}")]
    public async Task<IActionResult> UpdateMappingProfile(
        Guid id,
        UpdateMappingProfileRequest dto,
        CancellationToken ct)
    {
        if (_currentUser.UserId is null) return Unauthorized();

        var cmd    = new UpdateMappingProfileCommand(_currentUser.UserId, id, dto);
        var result = await _mediator.Send(cmd, ct);
        return result.IsFailure ? BadRequest(Problem(result)) : Ok(result.Value);
    }

    [HttpDelete("mappings/{id:guid}")]
    public async Task<IActionResult> DeleteMappingProfile(Guid id, CancellationToken ct)
    {
        if (_currentUser.UserId is null) return Unauthorized();

        var cmd    = new DeleteMappingProfileCommand(_currentUser.UserId, id);
        var result = await _mediator.Send(cmd, ct);
        return result.IsFailure ? BadRequest(Problem(result)) : NoContent();
    }

    // ── Export ────────────────────────────────────────────────────────────────

    [HttpPost("export")]
    public async Task<IActionResult> Export(ExportRequest request, CancellationToken ct)
    {
        if (_currentUser.UserId is null) return Unauthorized();

        var cmd    = new ExportCommand(_currentUser.UserId, request);
        var result = await _mediator.Send(cmd, ct);

        if (result.IsFailure)
            return BadRequest(Problem(result));

        return File(result.Value.Data, result.Value.ContentType, result.Value.FileName);
    }

    // ── Import ────────────────────────────────────────────────────────────────

    [HttpPost("import")]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50 MB
    public async Task<IActionResult> Import(
        [FromForm] string    entityTypeName,
        [FromForm] string    channel,
        [FromForm] Guid?     mappingProfileId,
        IFormFile            file,
        CancellationToken    ct)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        if (file.Length == 0) return BadRequest(new { Error = "Import.EmptyFile", Message = "Die Datei ist leer." });

        await using var stream = file.OpenReadStream();
        var cmd    = new ImportCommand(_currentUser.UserId, entityTypeName, channel, stream, mappingProfileId);
        var result = await _mediator.Send(cmd, ct);

        return result.IsFailure ? BadRequest(Problem(result)) : Ok(result.Value);
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private static object Problem<T>(Result<T> result)
        => new { Error = result.Error.Code, Message = result.Error.Description };

    private static object Problem(Result result)
        => new { Error = result.Error.Code, Message = result.Error.Description };
}
