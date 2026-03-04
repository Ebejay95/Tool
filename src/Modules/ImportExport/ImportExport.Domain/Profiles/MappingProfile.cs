using SharedKernel;

namespace ImportExport.Domain.Profiles;

public sealed record MappingProfileId : ValueObject
{
    private MappingProfileId(Guid value) => Value = value;
    public Guid Value { get; }
    public static MappingProfileId New()            => new(Guid.NewGuid());
    public static MappingProfileId From(Guid value) => new(value);
    public static implicit operator Guid(MappingProfileId id) => id.Value;
}

/// <summary>
/// Bildet eine Import/Export-Spalte auf eine Entity-Property ab.
/// SourceColumn  = Spaltenname in der Datei (z.B. "Aufgabe")
/// TargetField   = PropertyName der Entity   (z.B. "Title")
/// TransformHint = optionaler Hinweis für den Channel, z.B. Datumsformat "dd.MM.yyyy"
/// </summary>
public sealed record MappingFieldRule(
    string  SourceColumn,
    string  TargetField,
    string? TransformHint = null);

public sealed class MappingProfile : AggregateRoot, IResourceOwner
{
    string IResourceOwner.OwnerId => UserId.Value.ToString();

    private MappingProfile() { } // For EF

    private MappingProfile(
        MappingProfileId id,
        UserId           userId,
        string           name,
        string           entityTypeName,
        string           channel,
        List<MappingFieldRule> fieldRules)
    {
        Id             = id;
        UserId         = userId;
        Name           = name;
        EntityTypeName = entityTypeName;
        Channel        = channel;
        FieldRules     = fieldRules;
        CreatedAt      = DateTime.UtcNow;
        UpdatedAt      = DateTime.UtcNow;
    }

    public new MappingProfileId    Id             { get; private set; } = null!;
    public UserId                  UserId         { get; private set; } = null!;
    public string                  Name           { get; private set; } = string.Empty;
    /// <summary>Stabiler Typ-Schlüssel, z.B. "Todo" / "Measure"</summary>
    public string                  EntityTypeName { get; private set; } = string.Empty;
    /// <summary>Channel-Schlüssel, z.B. "Excel" / "Csv"</summary>
    public string                  Channel        { get; private set; } = string.Empty;
    public List<MappingFieldRule>  FieldRules     { get; private set; } = [];
    public DateTime                CreatedAt      { get; private set; }
    public DateTime                UpdatedAt      { get; private set; }

    public static Result<MappingProfile> Create(
        UserId           userId,
        string           name,
        string           entityTypeName,
        string           channel,
        List<MappingFieldRule> fieldRules)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<MappingProfile>(MappingProfileErrors.NameRequired);

        if (string.IsNullOrWhiteSpace(entityTypeName))
            return Result.Failure<MappingProfile>(MappingProfileErrors.EntityTypeNameRequired);

        if (string.IsNullOrWhiteSpace(channel))
            return Result.Failure<MappingProfile>(MappingProfileErrors.ChannelRequired);

        return Result.Success(new MappingProfile(
            MappingProfileId.New(), userId, name, entityTypeName, channel, fieldRules));
    }

    public Result Update(
        string                 name,
        List<MappingFieldRule> fieldRules)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(MappingProfileErrors.NameRequired);

        Name       = name;
        FieldRules = fieldRules;
        UpdatedAt  = DateTime.UtcNow;
        return Result.Success();
    }
}
