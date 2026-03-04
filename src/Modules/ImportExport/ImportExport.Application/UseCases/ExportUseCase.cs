using System.Reflection;
using SharedKernel;
using ImportExport.Application.Channels;
using ImportExport.Application.Ports;
using ImportExport.Application.Registry;
using ImportExport.Contracts;
using ImportExport.Domain.Profiles;
using MediatR;

namespace ImportExport.Application.UseCases;

// ─────────────────────────────────────────────────────────────────────────────
// EXPORT
// ─────────────────────────────────────────────────────────────────────────────

public sealed record ExportCommand(
    UserId        UserId,
    ExportRequest Request) : Command<ExportResult>;

public sealed class ExportHandler : IRequestHandler<ExportCommand, Result<ExportResult>>
{
    private readonly ExportableEntityRegistry        _registry;
    private readonly IEnumerable<IExportChannel>     _channels;
    private readonly IMappingProfileRepository        _profiles;
    private readonly IEnumerable<IExportSource>      _sources;

    public ExportHandler(
        ExportableEntityRegistry     registry,
        IEnumerable<IExportChannel>  channels,
        IMappingProfileRepository     profiles,
        IEnumerable<IExportSource>   sources)
    {
        _registry = registry;
        _channels = channels;
        _profiles = profiles;
        _sources  = sources;
    }

    public async Task<Result<ExportResult>> Handle(ExportCommand request, CancellationToken ct)
    {
        var desc = _registry.TryGet(request.Request.EntityTypeName);
        if (desc is null)
            return Result.Failure<ExportResult>(ImportExportErrors.UnknownEntityType);

        var channel = _channels.FirstOrDefault(c =>
            c.Key.Equals(request.Request.Channel, StringComparison.OrdinalIgnoreCase));
        if (channel is null)
            return Result.Failure<ExportResult>(ImportExportErrors.UnknownChannel);

        var source = _sources.FirstOrDefault(s =>
            s.EntityTypeName.Equals(request.Request.EntityTypeName, StringComparison.OrdinalIgnoreCase));
        if (source is null)
            return Result.Failure<ExportResult>(ImportExportErrors.NoExportSourceRegistered);

        // Daten laden
        var items = await source.GetAllForUserAsync(request.UserId, ct);

        // Optionales Mapping-Profil laden
        MappingProfile? profile = request.Request.MappingProfileId is Guid pid
            ? await _profiles.GetByIdAsync(pid, ct)
            : null;

        var fieldMap = BuildFieldMap(desc, profile);

        var exportableFields = desc.Fields.Where(f => f.CanExport)
            .OrderBy(f => f.Order)
            .ToList();

        var headers = exportableFields
            .Where(f => fieldMap.ContainsKey(f.PropertyName))
            .Select(f => fieldMap[f.PropertyName])
            .ToList();

        var rows = new List<IReadOnlyDictionary<string, string?>>();
        foreach (var obj in items)
        {
            var row = new Dictionary<string, string?>();
            foreach (var field in exportableFields)
            {
                if (!fieldMap.TryGetValue(field.PropertyName, out var col)) continue;
                var prop = desc.ClrType.GetProperty(field.PropertyName,
                    BindingFlags.Public | BindingFlags.Instance);
                var rawValue = prop?.GetValue(obj);
                row[col] = FormatValue(rawValue);
            }
            rows.Add(row);
        }

        var bytes    = await channel.WriteAsync(headers, rows, ct);
        var fileName = $"{request.Request.EntityTypeName}-Export-{DateTime.UtcNow:yyyyMMddHHmmss}{channel.FileExtension}";

        return Result.Success(new ExportResult(fileName, channel.ContentType, bytes));
    }

    /// Liefert Dictionary: PropertyName → Spaltenheader (ggf. durch Profil überschrieben)
    private static Dictionary<string, string> BuildFieldMap(
        ExportableEntityDescriptor desc,
        MappingProfile?            profile)
    {
        var map = desc.Fields
            .Where(f => f.CanExport)
            .ToDictionary(f => f.PropertyName, f => f.DisplayName);

        if (profile is null) return map;

        foreach (var rule in profile.FieldRules)
        {
            if (map.ContainsKey(rule.TargetField))
                map[rule.TargetField] = rule.SourceColumn;
        }
        return map;
    }

    private static string? FormatValue(object? value) => value switch
    {
        null              => null,
        DateTime dt       => dt.ToString("O"),
        DateTimeOffset dto => dto.ToString("O"),
        bool b            => b ? "true" : "false",
        _                 => value.ToString()
    };
}
