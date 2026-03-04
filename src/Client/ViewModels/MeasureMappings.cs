using Measures.Application.DTOs;
using Riok.Mapperly.Abstractions;

namespace App.ViewModels;

[Mapper]
public static partial class MeasureMappings
{
    // string → Guid Konverter: wird von Mapperly für alle Id-Felder verwendet
    private static Guid ParseGuid(string id) => Guid.Parse(id);

    // ── DTO → ViewModel ─────────────────────────────────────────────────────

    public static partial MeasureListItemViewModel ToViewModel(this MeasureSummaryDto dto);

    public static List<MeasureListItemViewModel> ToViewModels(this IEnumerable<MeasureSummaryDto> dtos)
        => dtos.Select(ToViewModel).ToList();

    // ── ViewModel → DTO (Create) ─────────────────────────────────────────────
    // SelectedCategoryIds/SelectedTagIds im ViewModel → CategoryIds/TagIds im DTO
    // Felder ohne Entsprechung im Formular (Graphdaten, Conf-Detaildaten) bleiben auf Default.

    [MapProperty(nameof(CreateMeasureViewModel.SelectedCategoryIds), nameof(CreateMeasureDto.CategoryIds))]
    [MapProperty(nameof(CreateMeasureViewModel.SelectedTagIds), nameof(CreateMeasureDto.TagIds))]
    [MapperIgnoreTarget(nameof(CreateMeasureDto.Dependencies))]
    [MapperIgnoreTarget(nameof(CreateMeasureDto.ConfDataQuality))]
    [MapperIgnoreTarget(nameof(CreateMeasureDto.ConfDataSourceCount))]
    [MapperIgnoreTarget(nameof(CreateMeasureDto.ConfDataRecency))]
    [MapperIgnoreTarget(nameof(CreateMeasureDto.ConfSpecificity))]
    [MapperIgnoreTarget(nameof(CreateMeasureDto.GraphDependentsCount))]
    [MapperIgnoreTarget(nameof(CreateMeasureDto.GraphImpactMultiplier))]
    [MapperIgnoreTarget(nameof(CreateMeasureDto.GraphTotalCost))]
    [MapperIgnoreTarget(nameof(CreateMeasureDto.GraphCostEfficiency))]
    public static partial CreateMeasureDto ToDto(this CreateMeasureViewModel vm);

    // ── MeasureDto → EditViewModel + EditState (Dialogvorbefüllung) ──────────
    // MeasureEditState kapselt die nicht-editierbaren technischen Felder,
    // ohne den Namespace-Konflikt "Measures" (Komponente vs. Modul) zu verursachen.

    public static CreateMeasureViewModel ToEditModel(this MeasureDto dto) => new()
    {
        IsoId               = dto.IsoId,
        Category            = dto.Category,
        Name                = dto.Name,
        CostEur             = dto.CostEur,
        EffortHours         = dto.EffortHours,
        ImpactRisk          = dto.ImpactRisk,
        Confidence          = dto.Confidence,
        Justification       = dto.Justification,
        SelectedCategoryIds = [.. dto.CategoryIds],
        SelectedTagIds      = [.. dto.TagIds],
    };

    public static MeasureEditState ToEditState(this MeasureDto dto) => new(
        Id                   : Guid.Parse(dto.Id),
        IsoId                : dto.IsoId,
        Dependencies         : dto.Dependencies,
        ConfDataQuality      : dto.ConfDataQuality,
        ConfDataSourceCount  : dto.ConfDataSourceCount,
        ConfDataRecency      : dto.ConfDataRecency,
        ConfSpecificity      : dto.ConfSpecificity,
        GraphDependentsCount : dto.GraphDependentsCount,
        GraphImpactMultiplier: dto.GraphImpactMultiplier,
        GraphTotalCost       : dto.GraphTotalCost,
        GraphCostEfficiency  : dto.GraphCostEfficiency);

    // ── EditViewModel + EditState → UpdateDto ─────────────────────────────────
    // Mischt die im Formular bearbeiteten Felder mit den unveränderlichen technischen Feldern.

    public static UpdateMeasureDto ToUpdateDto(this CreateMeasureViewModel vm, MeasureEditState state) => new()
    {
        Category              = vm.Category,
        Name                  = vm.Name,
        CostEur               = vm.CostEur,
        EffortHours           = vm.EffortHours,
        ImpactRisk            = vm.ImpactRisk,
        Confidence            = vm.Confidence,
        Justification         = vm.Justification,
        CategoryIds           = [.. vm.SelectedCategoryIds],
        TagIds                = [.. vm.SelectedTagIds],
        // Nicht-editierbare Felder aus dem EditState übernehmen
        Dependencies          = state.Dependencies,
        ConfDataQuality       = state.ConfDataQuality,
        ConfDataSourceCount   = state.ConfDataSourceCount,
        ConfDataRecency       = state.ConfDataRecency,
        ConfSpecificity       = state.ConfSpecificity,
        GraphDependentsCount  = state.GraphDependentsCount,
        GraphImpactMultiplier = state.GraphImpactMultiplier,
        GraphTotalCost        = state.GraphTotalCost,
        GraphCostEfficiency   = state.GraphCostEfficiency,
    };
}
