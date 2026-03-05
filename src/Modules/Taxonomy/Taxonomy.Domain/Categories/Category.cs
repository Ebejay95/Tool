using SharedKernel;

namespace Taxonomy.Domain.Categories;

/// <summary>
/// Globale Kategorie für die Taxonomie – sichtbar und verwaltbar von allen authentifizierten Benutzern.
/// </summary>
public sealed class Category : AggregateRoot
{
    private Category() { } // For EF

    private Category(
        CategoryId id,
        string label,
        string color)
    {
        Id        = id;
        Label     = label;
        Color     = color;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CategoryCreatedEvent(id, label));
    }

    public new CategoryId Id { get; private set; } = null!;

    public string Label { get; private set; } = string.Empty;

    /// <summary>Hexadezimale Farbe, z. B. "#FF5733".</summary>
    public string Color { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // ── Factory ───────────────────────────────────────────────────────────────

    public static Result<Category> Create(string label, string color)
    {
        if (string.IsNullOrWhiteSpace(label))
            return Result.Failure<Category>(CategoryErrors.LabelRequired);

        if (label.Length > 100)
            return Result.Failure<Category>(CategoryErrors.LabelTooLong);

        if (string.IsNullOrWhiteSpace(color))
            return Result.Failure<Category>(CategoryErrors.ColorRequired);

        return Result.Success(new Category(CategoryId.New(), label.Trim(), color.Trim()));
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

}
