using System.ComponentModel.DataAnnotations;
using Taxonomy.Application.DTOs;

namespace App.ViewModels;

// ── Category ─────────────────────────────────────────────────────────────────

public sealed record CategoryViewModel(
    Guid     Id,
    string   Label,
    string   Color,
    bool     IsGlobal,
    DateTime CreatedAt)
{
    public string ScopeLabel => IsGlobal ? "Global" : "Persönlich";
}

public sealed class CreateCategoryViewModel
{
    [Required(ErrorMessage = "Name ist erforderlich.")]
    [MaxLength(100, ErrorMessage = "Name darf max. 100 Zeichen lang sein.")]
    public string Label { get; set; } = string.Empty;

    [Required(ErrorMessage = "Farbe ist erforderlich.")]
    public string Color { get; set; } = "#6366F1";
}

// ── Tag ───────────────────────────────────────────────────────────────────────

public sealed record TagViewModel(
    Guid     Id,
    string   Label,
    string   Color,
    bool     IsGlobal,
    DateTime CreatedAt)
{
    public string ScopeLabel => IsGlobal ? "Global" : "Persönlich";
}

public sealed class CreateTagViewModel
{
    [Required(ErrorMessage = "Name ist erforderlich.")]
    [MaxLength(100, ErrorMessage = "Name darf max. 100 Zeichen lang sein.")]
    public string Label { get; set; } = string.Empty;

    [Required(ErrorMessage = "Farbe ist erforderlich.")]
    public string Color { get; set; } = "#10B981";
}

// ── Mappings ──────────────────────────────────────────────────────────────────

public static class TaxonomyMappings
{
    public static CategoryViewModel ToViewModel(this CategoryDto dto) => new(
        Id:        Guid.Parse(dto.Id),
        Label:     dto.Label,
        Color:     dto.Color,
        IsGlobal:  dto.IsGlobal,
        CreatedAt: dto.CreatedAt);

    public static List<CategoryViewModel> ToViewModels(this IEnumerable<CategoryDto> dtos) =>
        dtos.Select(d => d.ToViewModel()).ToList();

    public static CreateCategoryDto ToDto(this CreateCategoryViewModel vm) => new()
    {
        Label = vm.Label,
        Color = vm.Color
    };

    public static TagViewModel ToViewModel(this TagDto dto) => new(
        Id:        Guid.Parse(dto.Id),
        Label:     dto.Label,
        Color:     dto.Color,
        IsGlobal:  dto.IsGlobal,
        CreatedAt: dto.CreatedAt);

    public static List<TagViewModel> ToViewModels(this IEnumerable<TagDto> dtos) =>
        dtos.Select(d => d.ToViewModel()).ToList();

    public static CreateTagDto ToDto(this CreateTagViewModel vm) => new()
    {
        Label = vm.Label,
        Color = vm.Color
    };
}
