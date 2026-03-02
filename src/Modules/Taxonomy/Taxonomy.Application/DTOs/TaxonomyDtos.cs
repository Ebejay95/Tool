namespace Taxonomy.Application.DTOs;

// ── Category ─────────────────────────────────────────────────────────────────

public sealed class CreateCategoryDto
{
    public string Label { get; set; } = string.Empty;
    public string Color { get; set; } = "#6366F1";
}

public sealed class UpdateCategoryDto
{
    public string Label { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}

public sealed record CategoryDto(
    string  Id,
    string? UserId,
    string  Label,
    string  Color,
    bool    IsGlobal,
    DateTime CreatedAt,
    DateTime UpdatedAt);

// ── Tag ───────────────────────────────────────────────────────────────────────

public sealed class CreateTagDto
{
    public string Label { get; set; } = string.Empty;
    public string Color { get; set; } = "#10B981";
}

public sealed class UpdateTagDto
{
    public string Label { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}

public sealed record TagDto(
    string  Id,
    string? UserId,
    string  Label,
    string  Color,
    bool    IsGlobal,
    DateTime CreatedAt,
    DateTime UpdatedAt);
