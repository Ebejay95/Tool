using Measures.Application.Ports;
using Measures.Domain.Measures;
using Measures.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Measures.Infrastructure.Repositories;

public sealed class MeasureRepository(MeasuresDbContext context) : IMeasureRepository
{
    public async Task<Measure?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Measures.FirstOrDefaultAsync(m => m.Id == MeasureId.From(id), cancellationToken);

    public async Task<Measure?> GetByIdAndUserIdAsync(MeasureId measureId, UserId userId, CancellationToken cancellationToken = default)
        => await context.Measures.FirstOrDefaultAsync(m => m.Id == measureId && m.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<Measure>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
        => await context.Measures
            .Where(m => m.UserId == userId)
            .OrderBy(m => m.IsoId)
            .ToListAsync(cancellationToken);

    public async Task<Measure?> GetByIsoIdAndUserIdAsync(string isoId, UserId userId, CancellationToken cancellationToken = default)
        => await context.Measures.FirstOrDefaultAsync(m => m.IsoId == isoId && m.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<Measure>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.Measures.OrderBy(m => m.IsoId).ToListAsync(cancellationToken);

    public void Add(Measure entity) => context.Measures.Add(entity);
    public void Update(Measure entity) => context.Measures.Update(entity);
    public void Remove(Measure entity) => context.Measures.Remove(entity);
}
