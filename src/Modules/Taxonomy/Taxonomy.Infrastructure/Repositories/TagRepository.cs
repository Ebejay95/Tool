using Microsoft.EntityFrameworkCore;
using SharedKernel;
using Taxonomy.Application.Ports;
using Taxonomy.Domain.Tags;
using Taxonomy.Infrastructure.Persistence;

namespace Taxonomy.Infrastructure.Repositories;

public sealed class TagRepository(TaxonomyDbContext context) : ITagRepository
{
    public async Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.Tags.OrderBy(t => t.Label).ToListAsync(cancellationToken);

    public async Task<Tag?> GetByIdAsync(TagId id, CancellationToken cancellationToken = default)
        => await context.Tags.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Tags.FirstOrDefaultAsync(t => t.Id == TagId.From(id), cancellationToken);

    public async Task<Tag?> GetByLabelAsync(string label, CancellationToken cancellationToken = default)
        => await context.Tags.FirstOrDefaultAsync(t => t.Label == label, cancellationToken);

    public void Add(Tag entity)    => context.Tags.Add(entity);
    public void Update(Tag entity) => context.Tags.Update(entity);
    public void Remove(Tag entity) => context.Tags.Remove(entity);
}
