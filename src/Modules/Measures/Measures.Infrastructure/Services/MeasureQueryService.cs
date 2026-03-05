using Measures.Application.DTOs;
using Measures.Application.Ports;
using Measures.Domain.Measures;
using Measures.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Measures.Infrastructure.Services;

public sealed class MeasureQueryService(MeasuresDbContext context) : IMeasureQueryService
{
    public async Task<IReadOnlyList<MeasureDto>> GetUserMeasuresAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var entities = await context.Measures
            .Where(m => m.UserId == userId)
            .OrderBy(m => m.IsoId)
            .ToListAsync(cancellationToken);

        return entities.Select(ToDto).ToList();
    }

    public async Task<MeasureSummaryDto[]> GetUserMeasureSummariesAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var entities = await context.Measures
            .Where(m => m.UserId == userId)
            .OrderBy(m => m.IsoId)
            .ToListAsync(cancellationToken);

        return entities.Select(ToSummaryDto).ToArray();
    }

    public async Task<MeasureDto?> GetUserMeasureByIdAsync(MeasureId measureId, UserId userId, CancellationToken cancellationToken = default)
    {
        var entity = await context.Measures
            .FirstOrDefaultAsync(m => m.Id == measureId && m.UserId == userId, cancellationToken);

        return entity is null ? null : ToDto(entity);
    }

    private static MeasureDto ToDto(Measure m) => new(
        m.Id.Value.ToString(),
        m.UserId.Value.ToString(),
        m.IsoId,
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

    private static MeasureSummaryDto ToSummaryDto(Measure m) => new(
        m.Id.Value.ToString(),
        m.IsoId,
        m.Name,
        m.CostEur,
        m.ImpactRisk,
        m.Confidence,
        m.CategoryIds,
        m.TagIds);
}
