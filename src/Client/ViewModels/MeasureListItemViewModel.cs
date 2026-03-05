namespace App.ViewModels;

/// <summary>
/// Schreibgeschütztes ViewModel für eine einzelne Maßnahme in der Listenansicht.
/// </summary>
public sealed record MeasureListItemViewModel(
    Guid    Id,
    string  IsoId,
    string  Name,
    decimal CostEur,
    int     ImpactRisk,
    int     Confidence,
    List<Guid> CategoryIds,
    List<Guid> TagIds)
{
    // ── UI-Helpers ────────────────────────────────────────────────────────────

    /// <summary>ImpactRisk 1–5 als Sterne-Darstellung.</summary>
    public string ImpactRiskLabel => ImpactRisk switch
    {
        1 => "★☆☆☆☆",
        2 => "★★☆☆☆",
        3 => "★★★☆☆",
        4 => "★★★★☆",
        5 => "★★★★★",
        _ => ImpactRisk.ToString()
    };

    /// <summary>Confidence 1–3 als Text.</summary>
    public string ConfidenceLabel => Confidence switch
    {
        1 => "Niedrig",
        2 => "Mittel",
        3 => "Hoch",
        _ => Confidence.ToString()
    };

    /// <summary>Kosten formatiert in EUR.</summary>
    public string CostLabel => CostEur.ToString("N0") + " €";
}
