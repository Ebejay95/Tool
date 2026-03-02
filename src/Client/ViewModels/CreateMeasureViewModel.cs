using System.ComponentModel.DataAnnotations;

namespace App.ViewModels;

/// <summary>
/// ViewModel für das "Neue Maßnahme erstellen"-Formular.
/// Enthält Validierungsattribute ; unabhängig von DTOs/Domain-Typen.
/// </summary>
public sealed class CreateMeasureViewModel
{
    [Required(ErrorMessage = "ISO-ID ist erforderlich.")]
    [MaxLength(50, ErrorMessage = "ISO-ID darf max. 50 Zeichen lang sein.")]
    public string IsoId { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "Kategorie darf max. 100 Zeichen lang sein.")]
    public string Category { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name ist erforderlich.")]
    [MaxLength(200, ErrorMessage = "Name darf max. 200 Zeichen lang sein.")]
    public string Name { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "Kosten müssen ≥ 0 sein.")]
    public decimal CostEur { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Aufwand muss ≥ 0 sein.")]
    public double EffortHours { get; set; }

    [Range(1, 5, ErrorMessage = "Risikowirkung muss zwischen 1 und 5 liegen.")]
    public int ImpactRisk { get; set; } = 3;

    [Range(1, 3, ErrorMessage = "Konfidenz muss zwischen 1 und 3 liegen.")]
    public int Confidence { get; set; } = 2;

    [MaxLength(2000, ErrorMessage = "Begründung darf max. 2000 Zeichen lang sein.")]
    public string? Justification { get; set; }

    public HashSet<Guid> SelectedCategoryIds { get; set; } = [];
    public HashSet<Guid> SelectedTagIds       { get; set; } = [];
}
