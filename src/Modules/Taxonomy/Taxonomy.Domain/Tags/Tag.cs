using SharedKernel;

namespace Taxonomy.Domain.Tags;

/// <summary>
/// Tag für die Taxonomie.
/// UserId == null → globaler Tag (für alle User sichtbar).
/// UserId != null → benutzerspezifischer Tag.
/// </summary>
public sealed class Tag : AggregateRoot
{
    private Tag() { } // For EF

    private Tag(
        TagId id,
        UserId? userId,
        string label,
        string color)
    {
        Id        = id;
        UserId    = userId;
        Label     = label;
        Color     = color;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TagCreatedEvent(id, userId, label));
    }

    public new TagId Id { get; private set; } = null!;

    /// <summary>null = global für alle User.</summary>
    public UserId? UserId { get; private set; }

    public string Label { get; private set; } = string.Empty;

    /// <summary>Hexadezimale Farbe, z. B. "#3498DB".</summary>
    public string Color { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // ── Factory ───────────────────────────────────────────────────────────────

    public static Result<Tag> Create(UserId? userId, string label, string color)
    {
        if (string.IsNullOrWhiteSpace(label))
            return Result.Failure<Tag>(TagErrors.LabelRequired);

        if (label.Length > 100)
            return Result.Failure<Tag>(TagErrors.LabelTooLong);

        if (string.IsNullOrWhiteSpace(color))
            return Result.Failure<Tag>(TagErrors.ColorRequired);

        return Result.Success(new Tag(TagId.New(), userId, label.Trim(), color.Trim()));
    }

    // ── Mutations ─────────────────────────────────────────────────────────────

    public Result Update(string label, string color)
    {
        if (string.IsNullOrWhiteSpace(label))
            return Result.Failure(TagErrors.LabelRequired);

        if (label.Length > 100)
            return Result.Failure(TagErrors.LabelTooLong);

        if (string.IsNullOrWhiteSpace(color))
            return Result.Failure(TagErrors.ColorRequired);

        Label     = label.Trim();
        Color     = color.Trim();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TagUpdatedEvent(Id, label));

        return Result.Success();
    }

    public void MarkForDeletion()
        => AddDomainEvent(new TagDeletedEvent(Id));

    // ── Helpers ───────────────────────────────────────────────────────────────

    public bool IsOwnedBy(UserId userId) => UserId is not null && UserId.Value == userId.Value;

    public bool IsGlobal => UserId is null;
}
