using Measures.Application.DTOs;
using Measures.Domain.Measures;

namespace Measures.Application;

internal static class MeasureMapper
{
    internal static MeasureDto ToDto(Measure m) => new(
        m.Id.Value.ToString(),
        m.UserId.Value.ToString(),
        m.IsoId,
        m.Category,
        m.Name,
        m.CostEur,
        m.EffortHours,
        m.ImpactRisk,
        m.Confidence,
        m.Dependencies,
        m.Justification,
        m.ConfDataQuality,
        m.ConfDataSourceCount,
        m.ConfDataRecency,
        m.ConfSpecificity,
        m.GraphDependentsCount,
        m.GraphImpactMultiplier,
        m.GraphTotalCost,
        m.GraphCostEfficiency,
        m.CreatedAt,
        m.UpdatedAt,
        m.CategoryIds,
        m.TagIds);

    internal static MeasureSummaryDto ToSummaryDto(Measure m) => new(
        m.Id.Value.ToString(),
        m.IsoId,
        m.Category,
        m.Name,
        m.CostEur,
        m.ImpactRisk,
        m.Confidence,
        m.CategoryIds,
        m.TagIds);
}
