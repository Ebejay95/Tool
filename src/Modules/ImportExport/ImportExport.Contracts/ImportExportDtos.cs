namespace ImportExport.Contracts;

// ── Entity Registry ───────────────────────────────────────────────────────────

/// <summary>Metadaten zu einem registrierten, exportierbaren Entitätstyp.</summary>
public sealed record ExportableEntityDto(
    string                    TypeName,
    string                    DisplayName,
    IReadOnlyList<FieldInfoDto> Fields);

/// <summary>Beschreibung eines einzelnen export-fähigen Felds.</summary>
public sealed record FieldInfoDto(
    string PropertyName,
    string DisplayName,
    string TypeName,
    bool   CanImport,
    bool   CanExport,
    int    Order);

// ── Mapping Profiles ─────────────────────────────────────────────────────────

public sealed record MappingProfileDto(
    Guid                      Id,
    string                    Name,
    string                    EntityTypeName,
    string                    Channel,
    IReadOnlyList<FieldRuleDto> FieldRules,
    DateTime                  CreatedAt,
    DateTime                  UpdatedAt);

public sealed record FieldRuleDto(
    string  SourceColumn,
    string  TargetField,
    string? TransformHint);

public sealed record CreateMappingProfileRequest(
    string                  Name,
    string                  EntityTypeName,
    string                  Channel,
    List<FieldRuleDto>      FieldRules);

public sealed record UpdateMappingProfileRequest(
    string             Name,
    List<FieldRuleDto> FieldRules);

// ── Export ────────────────────────────────────────────────────────────────────

public sealed record ExportRequest(
    string  EntityTypeName,
    string  Channel,
    Guid?   MappingProfileId = null);

public sealed record ExportResult(
    string FileName,
    string ContentType,
    byte[] Data);

// ── Import ────────────────────────────────────────────────────────────────────

public sealed record ImportResult(
    int  ImportedCount,
    int  SkippedCount,
    int  ErrorCount,
    IReadOnlyList<string> Errors);

// ── Channels ──────────────────────────────────────────────────────────────────

public sealed record ChannelInfoDto(string Key, string DisplayName);
