using Microsoft.EntityFrameworkCore;
using SharedKernel;
using Taxonomy.Application;
using Taxonomy.Application.DTOs;
using Taxonomy.Application.Ports;
using Taxonomy.Domain.Categories;
using Taxonomy.Infrastructure.Persistence;

namespace Taxonomy.Infrastructure.Services;

public sealed class CategoryQueryService(TaxonomyDbContext context) : ICategoryQueryService
{
    public async Task<IReadOnlyList<CategoryDto>> GetAccessibleByUserAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var entities = await context.Categories
            .Where(c => c.UserId == null || c.UserId == userId)
            .OrderBy(c => c.Label)
            .ToListAsync(cancellationToken);

        return entities.Select(TaxonomyMapper.ToDto).ToList();
    }

    public async Task<CategoryDto?> GetByIdAccessibleByUserAsync(CategoryId id, UserId userId, CancellationToken cancellationToken = default)
    {
        var entity = await context.Categories
            .FirstOrDefaultAsync(c => c.Id == id && (c.UserId == null || c.UserId == userId), cancellationToken);

        return entity is null ? null : TaxonomyMapper.ToDto(entity);
    }
}
