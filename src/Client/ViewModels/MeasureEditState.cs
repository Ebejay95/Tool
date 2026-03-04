namespace App.ViewModels;

/// <summary>
/// Kapselt die nicht-editierbaren technischen Felder einer Maßnahme (Graphdaten, Konfidenz-Details),
/// die beim Update-Request erhalten bleiben müssen, aber im Edit-Formular nicht angezeigt werden.
/// Vermeidet den C#-Namenskonflikt zwischen der Blazor-Komponente "Measures" und dem
/// Modul-Namespace "Measures.Application.DTOs".
/// </summary>
public sealed record MeasureEditState(
    Guid   Id,
    string IsoId,           // unveränderlich nach dem Anlegen
    List<string> Dependencies,
    int    ConfDataQuality,
    int    ConfDataSourceCount,
    int    ConfDataRecency,
    int    ConfSpecificity,
    int    GraphDependentsCount,
    double GraphImpactMultiplier,
    decimal GraphTotalCost,
    double GraphCostEfficiency);
