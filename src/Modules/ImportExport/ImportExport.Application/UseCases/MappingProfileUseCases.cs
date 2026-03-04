using SharedKernel;
using ImportExport.Application.Channels;
using ImportExport.Application.Ports;
using ImportExport.Contracts;
using ImportExport.Domain.Profiles;
using MediatR;

namespace ImportExport.Application.UseCases;

// ─────────────────────────────────────────────────────────────────────────────
// QUERIES
// ─────────────────────────────────────────────────────────────────────────────

public sealed record GetMappingProfilesQuery(UserId UserId, string? EntityTypeName = null)
    : Query<IReadOnlyList<MappingProfileDto>>;

public sealed class GetMappingProfilesHandler
    : IRequestHandler<GetMappingProfilesQuery, Result<IReadOnlyList<MappingProfileDto>>>
{
    private readonly IMappingProfileRepository _repo;

    public GetMappingProfilesHandler(IMappingProfileRepository repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<MappingProfileDto>>> Handle(
        GetMappingProfilesQuery request, CancellationToken ct)
    {
        var profiles = request.EntityTypeName is not null
            ? await _repo.GetByUserAndTypeAsync(request.UserId, request.EntityTypeName, ct)
            : await _repo.GetByUserAsync(request.UserId, ct);

        return Result.Success(profiles.Select(MapToDto).ToList() as IReadOnlyList<MappingProfileDto>);
    }

    internal static MappingProfileDto MapToDto(MappingProfile p) => new(
        p.Id.Value,
        p.Name,
        p.EntityTypeName,
        p.Channel,
        p.FieldRules.Select(r => new FieldRuleDto(r.SourceColumn, r.TargetField, r.TransformHint)).ToList(),
        p.CreatedAt,
        p.UpdatedAt);
}

// ─────────────────────────────────────────────────────────────────────────────
// CREATE
// ─────────────────────────────────────────────────────────────────────────────

public sealed record CreateMappingProfileCommand(
    UserId                       UserId,
    CreateMappingProfileRequest  Data) : Command<MappingProfileDto>;

public sealed class CreateMappingProfileHandler
    : IRequestHandler<CreateMappingProfileCommand, Result<MappingProfileDto>>
{
    private readonly IMappingProfileRepository _repo;
    private readonly IImportExportUnitOfWork   _uow;

    public CreateMappingProfileHandler(IMappingProfileRepository repo, IImportExportUnitOfWork uow)
    {
        _repo = repo;
        _uow  = uow;
    }

    public async Task<Result<MappingProfileDto>> Handle(CreateMappingProfileCommand request, CancellationToken ct)
    {
        var rules = request.Data.FieldRules
            .Select(r => new MappingFieldRule(r.SourceColumn, r.TargetField, r.TransformHint))
            .ToList();

        var result = MappingProfile.Create(
            request.UserId,
            request.Data.Name,
            request.Data.EntityTypeName,
            request.Data.Channel,
            rules);

        if (result.IsFailure)
            return Result.Failure<MappingProfileDto>(result.Error);

        _repo.Add(result.Value);
        await _uow.SaveChangesAsync(ct);

        return Result.Success(GetMappingProfilesHandler.MapToDto(result.Value));
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// UPDATE
// ─────────────────────────────────────────────────────────────────────────────

public sealed record UpdateMappingProfileCommand(
    UserId                       UserId,
    Guid                         ProfileId,
    UpdateMappingProfileRequest  Data) : Command<MappingProfileDto>;

public sealed class UpdateMappingProfileHandler
    : IRequestHandler<UpdateMappingProfileCommand, Result<MappingProfileDto>>
{
    private readonly IMappingProfileRepository _repo;
    private readonly IImportExportUnitOfWork   _uow;

    public UpdateMappingProfileHandler(IMappingProfileRepository repo, IImportExportUnitOfWork uow)
    {
        _repo = repo;
        _uow  = uow;
    }

    public async Task<Result<MappingProfileDto>> Handle(UpdateMappingProfileCommand request, CancellationToken ct)
    {
        var profile = await _repo.GetByIdAsync(request.ProfileId, ct);
        if (profile is null)
            return Result.Failure<MappingProfileDto>(MappingProfileErrors.NotFound);

        if (profile.UserId != request.UserId)
            return Result.Failure<MappingProfileDto>(MappingProfileErrors.Forbidden);

        var rules = request.Data.FieldRules
            .Select(r => new MappingFieldRule(r.SourceColumn, r.TargetField, r.TransformHint))
            .ToList();

        var updateResult = profile.Update(request.Data.Name, rules);
        if (updateResult.IsFailure)
            return Result.Failure<MappingProfileDto>(updateResult.Error);

        await _uow.SaveChangesAsync(ct);
        return Result.Success(GetMappingProfilesHandler.MapToDto(profile));
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// DELETE
// ─────────────────────────────────────────────────────────────────────────────

public sealed record DeleteMappingProfileCommand(UserId UserId, Guid ProfileId) : Command;

public sealed class DeleteMappingProfileHandler
    : IRequestHandler<DeleteMappingProfileCommand, Result>
{
    private readonly IMappingProfileRepository _repo;
    private readonly IImportExportUnitOfWork   _uow;

    public DeleteMappingProfileHandler(IMappingProfileRepository repo, IImportExportUnitOfWork uow)
    {
        _repo = repo;
        _uow  = uow;
    }

    public async Task<Result> Handle(DeleteMappingProfileCommand request, CancellationToken ct)
    {
        var profile = await _repo.GetByIdAsync(request.ProfileId, ct);
        if (profile is null)
            return Result.Failure(MappingProfileErrors.NotFound);

        if (profile.UserId != request.UserId)
            return Result.Failure(MappingProfileErrors.Forbidden);

        _repo.Remove(profile);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// REGISTRY QUERY: Alle bekannten exportierbaren Typen + Channels abfragen
// ─────────────────────────────────────────────────────────────────────────────

public sealed record GetExportableEntitiesQuery : Query<IReadOnlyList<ExportableEntityDto>>;

public sealed class GetExportableEntitiesHandler
    : IRequestHandler<GetExportableEntitiesQuery, Result<IReadOnlyList<ExportableEntityDto>>>
{
    private readonly ImportExport.Application.Registry.ExportableEntityRegistry _registry;

    public GetExportableEntitiesHandler(
        ImportExport.Application.Registry.ExportableEntityRegistry registry)
        => _registry = registry;

    public Task<Result<IReadOnlyList<ExportableEntityDto>>> Handle(
        GetExportableEntitiesQuery request, CancellationToken ct)
    {
        var result = _registry.GetAll()
            .Select(d => new ExportableEntityDto(
                d.TypeName,
                d.TypeName, // DisplayName = TypeName; kann in Zukunft mit Attribut überschrieben werden
                d.Fields.Select(f => new FieldInfoDto(
                    f.PropertyName, f.DisplayName, f.PropertyType.Name,
                    f.CanImport, f.CanExport, f.Order)).ToList()))
            .ToList() as IReadOnlyList<ExportableEntityDto>;

        return Task.FromResult(Result.Success(result));
    }
}

public sealed record GetChannelsQuery : Query<IReadOnlyList<ChannelInfoDto>>;

public sealed class GetChannelsHandler
    : IRequestHandler<GetChannelsQuery, Result<IReadOnlyList<ChannelInfoDto>>>
{
    private readonly IEnumerable<IExportChannel> _exportChannels;
    private readonly IEnumerable<IImportChannel> _importChannels;

    public GetChannelsHandler(
        IEnumerable<IExportChannel> exportChannels,
        IEnumerable<IImportChannel> importChannels)
    {
        _exportChannels = exportChannels;
        _importChannels = importChannels;
    }

    public Task<Result<IReadOnlyList<ChannelInfoDto>>> Handle(
        GetChannelsQuery request, CancellationToken ct)
    {
        var channels = _exportChannels
            .Select(c => new ChannelInfoDto(c.Key, c.DisplayName))
            .ToList() as IReadOnlyList<ChannelInfoDto>;

        return Task.FromResult(Result.Success(channels));
    }
}
