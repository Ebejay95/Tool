using SharedKernel;
using Taxonomy.Application.DTOs;
using Taxonomy.Domain.Categories;
using Taxonomy.Domain.Tags;

namespace Taxonomy.Application.Ports;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Category?> GetByIdAsync(CategoryId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Category?> GetByLabelAsync(string label, CancellationToken cancellationToken = default);
}

public interface ITagRepository : IRepository<Tag>
{
    Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Tag?> GetByIdAsync(TagId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Tag?> GetByLabelAsync(string label, CancellationToken cancellationToken = default);
}

public interface ICategoryQueryService
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CategoryDto?> GetByIdAsync(CategoryId id, CancellationToken cancellationToken = default);
}

public interface ITagQueryService
{
    Task<IReadOnlyList<TagDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TagDto?> GetByIdAsync(TagId id, CancellationToken cancellationToken = default);
}

public interface ITaxonomyUnitOfWork : IUnitOfWork { }
