using SharedKernel;

namespace Taxonomy.Domain.Categories;

/// <summary>
/// Kategorie für die Taxonomie.
/// UserId == null → globale Kategorie (für alle User sichtbar, nur Admins änderbar).
/// UserId != null → benutzerspezifische Kategorie.
/// </summary>
public sealed class Category : AggregateRoot
{
    private Category() { } // For EF

    private Category(
        CategoryId id,
        UserId? userId,
        string label,
        string color)
    {
        Id     = id;
        UserId = userId;
        Label  = label;
        Color  = color;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CategoryCreatedEvent(id, userId, label));
    }

    public new CategoryId Id { get; private set; } = null!;

    /// <summary>null = global für alle User.</summary>
    public UserId? UserId { get; private set; }

    public string Label { get; private set; } = string.Empty;

    /// <summary>Hexadezimale Farbe, z. B. "#FF5733".</summary>
    public string Color { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // ── Factory ───────────────────────────────────────────────────────────────

    public static Result<Category> Create(UserId? userId, string label, string color)
    {
        if (string.IsNullOrWhiteSpace(label))
            return Result.Failure<Category>(CategoryErrors.LabelRequired);

        if (label.Length > 100)
            return Result.Failure<Category>(CategoryErrors.LabelTooLong);

        if (string.IsNullOrWhiteSpace(color))
            return Result.Failure<Category>(CategoryErrors.ColorRequired);

        return Result.Success(new Category(CategoryId.New(), userId, label.Trim(), color.Trim()));
    }

    // ── Mutations ─────────────────────────────────────────────────────────────

    public Result Update(string label, string color)
    {
        if (string.IsNullOrWhiteSpace(label))
            return Result.Failure(CategoryErrors.LabelRequired);

        if (label.Length > 100)
            return Result.Failure(CategoryErrors.LabelTooLong);

        if (string.IsNullOrWhiteSpace(color))
            return Result.Failure(CategoryErrors.ColorRequired);

        Label     = label.Trim();
        Color     = color.Trim();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CategoryUpdatedEvent(Id, label));

        return Result.Success();
    }

    public void MarkForDeletion()
        => AddDomainEvent(new CategoryDeletedEvent(Id));

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Darf der angegebene User diese Kategorie bearbeiten?</summary>
    public bool IsOwnedBy(UserId userId) => UserId is not null && UserId.Value == userId.Value;

    public bool IsGlobal => UserId is null;
}
