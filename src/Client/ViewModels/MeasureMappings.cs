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

    // ── ViewModel → DTO ─────────────────────────────────────────────────────
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
}
