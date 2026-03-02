using Measures.Application.DTOs;

namespace App.ViewModels;

/// <summary>
/// Mapping-Methoden zwischen DTOs (Application-Layer) und ViewModels (Client-Layer).
/// </summary>
public static class MeasureMappings
{
    // ── DTO → ViewModel ─────────────────────────────────────────────────────

    public static MeasureListItemViewModel ToViewModel(this MeasureSummaryDto dto) =>
        new(
            Id:          Guid.Parse(dto.Id),
            IsoId:       dto.IsoId,
            Category:    dto.Category,
            Name:        dto.Name,
            CostEur:     dto.CostEur,
            ImpactRisk:  dto.ImpactRisk,
            Confidence:  dto.Confidence,
            CategoryIds: dto.CategoryIds,
            TagIds:      dto.TagIds
        );

    public static List<MeasureListItemViewModel> ToViewModels(this IEnumerable<MeasureSummaryDto> dtos) =>
        dtos.Select(ToViewModel).ToList();

    // ── ViewModel → DTO ─────────────────────────────────────────────────────

    public static CreateMeasureDto ToDto(this CreateMeasureViewModel vm) =>
        new()
        {
            IsoId         = vm.IsoId,
            Category      = vm.Category,
            Name          = vm.Name,
            CostEur       = vm.CostEur,
            EffortHours   = vm.EffortHours,
            ImpactRisk    = vm.ImpactRisk,
            Confidence    = vm.Confidence,
            Justification = vm.Justification,
            CategoryIds   = vm.SelectedCategoryIds.ToList(),
            TagIds        = vm.SelectedTagIds.ToList()
        };
}
