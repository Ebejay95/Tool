using MediatR;
using SharedKernel;
using Taxonomy.Application.DTOs;
using Taxonomy.Application.Ports;

namespace Taxonomy.Application.UseCases.Queries;

public sealed record GetAllCategoriesQuery : Query<IReadOnlyList<CategoryDto>>;
public sealed class GetAllCategoriesHandler(ICategoryQueryService queryService)
    : IRequestHandler<GetAllCategoriesQuery, Result<IReadOnlyList<CategoryDto>>>
{
    public async Task<Result<IReadOnlyList<CategoryDto>>> Handle(
        GetAllCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var items = await queryService.GetAllAsync(cancellationToken);
        return Result.Success(items);
    }
}

public sealed record GetCategoryByIdQuery(CategoryId CategoryId) : Query<CategoryDto>;
public sealed class GetCategoryByIdHandler(ICategoryQueryService queryService)
    : IRequestHandler<GetCategoryByIdQuery, Result<CategoryDto>>
{
    public async Task<Result<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await queryService.GetByIdAsync(request.CategoryId, cancellationToken);
        return item is not null
            ? Result.Success(item)
            : Result.Failure<CategoryDto>(new Error("General.NotFound", "Kategorie nicht gefunden."));
    }
}

public sealed record GetAllTagsQuery : Query<IReadOnlyList<TagDto>>;
public sealed class GetAllTagsHandler(ITagQueryService queryService)
    : IRequestHandler<GetAllTagsQuery, Result<IReadOnlyList<TagDto>>>
{
    public async Task<Result<IReadOnlyList<TagDto>>> Handle(
        GetAllTagsQuery request,
        CancellationToken cancellationToken)
    {
        var items = await queryService.GetAllAsync(cancellationToken);
        return Result.Success(items);
    }
}

public sealed record GetTagByIdQuery(TagId TagId) : Query<TagDto>;
public sealed class GetTagByIdHandler(ITagQueryService queryService)
    : IRequestHandler<GetTagByIdQuery, Result<TagDto>>
{
    public async Task<Result<TagDto>> Handle(GetTagByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await queryService.GetByIdAsync(request.TagId, cancellationToken);
        return item is not null
            ? Result.Success(item)
            : Result.Failure<TagDto>(new Error("General.NotFound", "Tag nicht gefunden."));
    }
}
