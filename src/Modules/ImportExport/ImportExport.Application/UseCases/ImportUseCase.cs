using SharedKernel;
using ImportExport.Application.Channels;
using ImportExport.Application.Ports;
using ImportExport.Application.Registry;
using ImportExport.Contracts;
using MediatR;

namespace ImportExport.Application.UseCases;

// ─────────────────────────────────────────────────────────────────────────────
// IMPORT
// ─────────────────────────────────────────────────────────────────────────────

public sealed record ImportCommand(
    UserId   UserId,
    string   EntityTypeName,
    string   Channel,
    Stream   FileStream,
    Guid?    MappingProfileId = null) : Command<ImportResult>;

public sealed class ImportHandler : IRequestHandler<ImportCommand, Result<ImportResult>>
{
    private readonly ExportableEntityRegistry       _registry;
    private readonly IEnumerable<IImportChannel>    _channels;
    private readonly IEnumerable<IImportAdapter>    _adapters;
    private readonly IMappingProfileRepository       _profiles;

    public ImportHandler(
        ExportableEntityRegistry     registry,
        IEnumerable<IImportChannel>  channels,
        IEnumerable<IImportAdapter>  adapters,
        IMappingProfileRepository     profiles)
    {
        _registry = registry;
        _channels = channels;
        _adapters = adapters;
        _profiles = profiles;
    }

    public async Task<Result<ImportResult>> Handle(ImportCommand request, CancellationToken ct)
    {
        var desc = _registry.TryGet(request.EntityTypeName);
        if (desc is null)
            return Result.Failure<ImportResult>(ImportExportErrors.UnknownEntityType);

        var channel = _channels.FirstOrDefault(c =>
            c.Key.Equals(request.Channel, StringComparison.OrdinalIgnoreCase));
        if (channel is null)
            return Result.Failure<ImportResult>(ImportExportErrors.UnknownChannel);

        var adapter = _adapters.FirstOrDefault(a =>
            a.EntityTypeName.Equals(request.EntityTypeName, StringComparison.OrdinalIgnoreCase));
        if (adapter is null)
            return Result.Failure<ImportResult>(ImportExportErrors.NoImportAdapterRegistered);

        // Optionales Mapping-Profil laden: SourceColumn → TargetField (Property)
        var columnToProperty = await BuildColumnMapAsync(request.MappingProfileId, desc, ct);

        // Datei parsen
        var rows = await channel.ParseAsync(request.FileStream, ct);

        int imported = 0, skipped = 0, errorCount = 0;
        var errors = new List<string>();

        foreach (var (row, index) in rows.Select((r, i) => (r, i)))
        {
            // Spaltenname → PropertyName übersetzen
            var mapped = new Dictionary<string, string?>();
            foreach (var (col, val) in row.Values)
            {
                var key = columnToProperty.TryGetValue(col, out var prop) ? prop : col;
                mapped[key] = val;
            }

            var error = await adapter.ImportRowAsync(request.UserId, mapped, ct);
            if (error is null)
                imported++;
            else
            {
                errorCount++;
                errors.Add($"Zeile {index + 2}: {error}");
                skipped++;
            }
        }

        return Result.Success(new ImportResult(imported, skipped, errorCount, errors));
    }

    private async Task<Dictionary<string, string>> BuildColumnMapAsync(
        Guid? profileId,
        ExportableEntityDescriptor desc,
        CancellationToken ct)
    {
        // Default: DisplayName → PropertyName
        var map = desc.Fields.ToDictionary(f => f.DisplayName, f => f.PropertyName);

        if (profileId is null) return map;

        var profile = await _profiles.GetByIdAsync(profileId.Value, ct);
        if (profile is null) return map;

        foreach (var rule in profile.FieldRules)
            map[rule.SourceColumn] = rule.TargetField;

        return map;
    }
}
