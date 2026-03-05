using Microsoft.EntityFrameworkCore;
using SharedKernel;
using Taxonomy.Application;
using Taxonomy.Application.DTOs;
using Taxonomy.Application.Ports;
using Taxonomy.Domain.Tags;
using Taxonomy.Infrastructure.Persistence;

namespace Taxonomy.Infrastructure.Services;

public sealed class TagQueryService(TaxonomyDbContext context) : ITagQueryService
{
    public async Task<IReadOnlyList<TagDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await context.Tags
            .OrderBy(t => t.Label)
            .ToListAsync(cancellationToken);

        return entities.Select(TaxonomyMapper.ToDto).ToList();
    }

    public async Task<TagDto?> GetByIdAsync(TagId id, CancellationToken cancellationToken = default)
    {
        var entity = await context.Tags.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        return entity is null ? null : TaxonomyMapper.ToDto(entity);
    }
}
