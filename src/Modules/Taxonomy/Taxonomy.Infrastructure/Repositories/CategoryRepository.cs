using Microsoft.EntityFrameworkCore;
using SharedKernel;
using Taxonomy.Application.Ports;
using Taxonomy.Domain.Categories;
using Taxonomy.Infrastructure.Persistence;

namespace Taxonomy.Infrastructure.Repositories;

public sealed class CategoryRepository(TaxonomyDbContext context) : ICategoryRepository
{
    /// <summary>Globale + eigene Kategorien des Users.</summary>
    public async Task<IReadOnlyList<Category>> GetAccessibleByUserAsync(UserId userId, CancellationToken cancellationToken = default)
        => await context.Categories
            .Where(c => c.UserId == null || c.UserId == userId)
            .OrderBy(c => c.Label)
            .ToListAsync(cancellationToken);

    public async Task<Category?> GetByIdAccessibleByUserAsync(CategoryId id, UserId userId, CancellationToken cancellationToken = default)
        => await context.Categories
            .FirstOrDefaultAsync(c => c.Id == id && (c.UserId == null || c.UserId == userId), cancellationToken);

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Categories.FirstOrDefaultAsync(c => c.Id == CategoryId.From(id), cancellationToken);

    public async Task<Category?> GetByLabelAndUserIdAsync(string label, UserId userId, CancellationToken cancellationToken = default)
        => await context.Categories.FirstOrDefaultAsync(
            c => c.Label == label && c.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.Categories.OrderBy(c => c.Label).ToListAsync(cancellationToken);

    public void Add(Category entity)    => context.Categories.Add(entity);
    public void Update(Category entity) => context.Categories.Update(entity);
    public void Remove(Category entity) => context.Categories.Remove(entity);
}
