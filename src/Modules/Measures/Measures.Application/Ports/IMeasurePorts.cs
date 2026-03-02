using Measures.Application.DTOs;
using Measures.Domain.Measures;
using SharedKernel;

namespace Measures.Application.Ports;

public interface IMeasureRepository : IRepository<Measure>
{
    Task<Measure?> GetByIdAndUserIdAsync(MeasureId measureId, UserId userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Measure>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<Measure?> GetByIsoIdAndUserIdAsync(string isoId, UserId userId, CancellationToken cancellationToken = default);
}

public interface IMeasureQueryService
{
    Task<IReadOnlyList<MeasureDto>> GetUserMeasuresAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<MeasureSummaryDto[]> GetUserMeasureSummariesAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<MeasureDto?> GetUserMeasureByIdAsync(MeasureId measureId, UserId userId, CancellationToken cancellationToken = default);
}
