using ImportExport.Application.UseCases;
using Measures.Application.Ports;
using Measures.Domain.Measures;
using SharedKernel;

namespace Measures.Infrastructure.ImportExport;

/// <summary>Stellt alle Measure-Entitäten eines Users für den Export bereit.</summary>
public sealed class MeasureExportSource : IExportSource
{
    private readonly IMeasureRepository _repository;

    public MeasureExportSource(IMeasureRepository repository) => _repository = repository;

    public string EntityTypeName => Measure.ExportableTypeName;

    public async Task<IReadOnlyList<object>> GetAllForUserAsync(UserId userId, CancellationToken ct = default)
    {
        var measures = await _repository.GetByUserIdAsync(userId, ct);
        return measures.Cast<object>().ToList();
    }
}
