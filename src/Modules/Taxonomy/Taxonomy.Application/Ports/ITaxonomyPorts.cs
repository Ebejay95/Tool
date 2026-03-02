using SharedKernel;
using Taxonomy.Application.DTOs;
using Taxonomy.Domain.Categories;
using Taxonomy.Domain.Tags;

namespace Taxonomy.Application.Ports;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Category?> GetByIdAccessibleByUserAsync(CategoryId id, UserId userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetAccessibleByUserAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<Category?> GetByLabelAndUserIdAsync(string label, UserId userId, CancellationToken cancellationToken = default);
}

public interface ITagRepository : IRepository<Tag>
{
    Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Tag?> GetByIdAccessibleByUserAsync(TagId id, UserId userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Tag>> GetAccessibleByUserAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<Tag?> GetByLabelAndUserIdAsync(string label, UserId userId, CancellationToken cancellationToken = default);
}

public interface ICategoryQueryService
{
    Task<IReadOnlyList<CategoryDto>> GetAccessibleByUserAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<CategoryDto?> GetByIdAccessibleByUserAsync(CategoryId id, UserId userId, CancellationToken cancellationToken = default);
}

public interface ITagQueryService
{
    Task<IReadOnlyList<TagDto>> GetAccessibleByUserAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<TagDto?> GetByIdAccessibleByUserAsync(TagId id, UserId userId, CancellationToken cancellationToken = default);
}

public interface ITaxonomyUnitOfWork : IUnitOfWork { }
