using MediatR;
using SharedKernel;
using Taxonomy.Application.DTOs;
using Taxonomy.Application.Ports;

namespace Taxonomy.Application.UseCases.Queries;

// ── Categories ────────────────────────────────────────────────────────────────

public sealed record GetAccessibleCategoriesQuery(UserId UserId) : Query<IReadOnlyList<CategoryDto>>;

public sealed class GetAccessibleCategoriesHandler(ICategoryQueryService queryService)
    : IRequestHandler<GetAccessibleCategoriesQuery, Result<IReadOnlyList<CategoryDto>>>
{
    public async Task<Result<IReadOnlyList<CategoryDto>>> Handle(
        GetAccessibleCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var items = await queryService.GetAccessibleByUserAsync(request.UserId, cancellationToken);
        return Result.Success(items);
    }
}

public sealed record GetCategoryByIdQuery(UserId UserId, CategoryId CategoryId) : Query<CategoryDto>;

public sealed class GetCategoryByIdHandler(ICategoryQueryService queryService)
    : IRequestHandler<GetCategoryByIdQuery, Result<CategoryDto>>
{
    public async Task<Result<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await queryService.GetByIdAccessibleByUserAsync(request.CategoryId, request.UserId, cancellationToken);
        return item is not null
            ? Result.Success(item)
            : Result.Failure<CategoryDto>(new Error("General.NotFound", "Kategorie nicht gefunden."));
    }
}

// ── Tags ──────────────────────────────────────────────────────────────────────

public sealed record GetAccessibleTagsQuery(UserId UserId) : Query<IReadOnlyList<TagDto>>;

public sealed class GetAccessibleTagsHandler(ITagQueryService queryService)
    : IRequestHandler<GetAccessibleTagsQuery, Result<IReadOnlyList<TagDto>>>
{
    public async Task<Result<IReadOnlyList<TagDto>>> Handle(
        GetAccessibleTagsQuery request,
        CancellationToken cancellationToken)
    {
        var items = await queryService.GetAccessibleByUserAsync(request.UserId, cancellationToken);
        return Result.Success(items);
    }
}

public sealed record GetTagByIdQuery(UserId UserId, TagId TagId) : Query<TagDto>;

public sealed class GetTagByIdHandler(ITagQueryService queryService)
    : IRequestHandler<GetTagByIdQuery, Result<TagDto>>
{
    public async Task<Result<TagDto>> Handle(GetTagByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await queryService.GetByIdAccessibleByUserAsync(request.TagId, request.UserId, cancellationToken);
        return item is not null
            ? Result.Success(item)
            : Result.Failure<TagDto>(new Error("General.NotFound", "Tag nicht gefunden."));
    }
}
