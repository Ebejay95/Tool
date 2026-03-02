using Microsoft.EntityFrameworkCore;
using SharedKernel;
using Taxonomy.Application.Ports;
using Taxonomy.Domain.Tags;
using Taxonomy.Infrastructure.Persistence;

namespace Taxonomy.Infrastructure.Repositories;

public sealed class TagRepository(TaxonomyDbContext context) : ITagRepository
{
    /// <summary>Globale + eigene Tags des Users.</summary>
    public async Task<IReadOnlyList<Tag>> GetAccessibleByUserAsync(UserId userId, CancellationToken cancellationToken = default)
        => await context.Tags
            .Where(t => t.UserId == null || t.UserId == userId)
            .OrderBy(t => t.Label)
            .ToListAsync(cancellationToken);

    public async Task<Tag?> GetByIdAccessibleByUserAsync(TagId id, UserId userId, CancellationToken cancellationToken = default)
        => await context.Tags
            .FirstOrDefaultAsync(t => t.Id == id && (t.UserId == null || t.UserId == userId), cancellationToken);

    public async Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Tags.FirstOrDefaultAsync(t => t.Id == TagId.From(id), cancellationToken);

    public async Task<Tag?> GetByLabelAndUserIdAsync(string label, UserId userId, CancellationToken cancellationToken = default)
        => await context.Tags.FirstOrDefaultAsync(
            t => t.Label == label && t.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.Tags.OrderBy(t => t.Label).ToListAsync(cancellationToken);

    public void Add(Tag entity)    => context.Tags.Add(entity);
    public void Update(Tag entity) => context.Tags.Update(entity);
    public void Remove(Tag entity) => context.Tags.Remove(entity);
}
