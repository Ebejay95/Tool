using MediatR;
using SharedKernel;
using Taxonomy.Application.DTOs;
using Taxonomy.Application.Ports;
using Taxonomy.Domain.Categories;

namespace Taxonomy.Application.UseCases.Commands;

// ── Create ───────────────────────────────────────────────────────────────────

public sealed record CreateCategoryCommand(UserId CurrentUserId, CreateCategoryDto Data) : Command<CategoryDto>;

public sealed class CreateCategoryHandler(ICategoryRepository repository, ITaxonomyUnitOfWork unitOfWork)
    : IRequestHandler<CreateCategoryCommand, Result<CategoryDto>>
{
    public async Task<Result<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var existing = await repository.GetByLabelAndUserIdAsync(request.Data.Label, request.CurrentUserId, cancellationToken);
        if (existing is not null)
            return Result.Failure<CategoryDto>(CategoryErrors.LabelAlreadyExists);

        var result = Category.Create(request.CurrentUserId, request.Data.Label, request.Data.Color);
        if (result.IsFailure)
            return Result.Failure<CategoryDto>(result.Error);

        repository.Add(result.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(TaxonomyMapper.ToDto(result.Value));
    }
}

// ── Update ───────────────────────────────────────────────────────────────────

public sealed record UpdateCategoryCommand(UserId CurrentUserId, CategoryId CategoryId, UpdateCategoryDto Data) : Command<CategoryDto>;

public sealed class UpdateCategoryHandler(ICategoryRepository repository, ITaxonomyUnitOfWork unitOfWork)
    : IRequestHandler<UpdateCategoryCommand, Result<CategoryDto>>
{
    public async Task<Result<CategoryDto>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await repository.GetByIdAccessibleByUserAsync(request.CategoryId, request.CurrentUserId, cancellationToken);
        if (category is null)
            return Result.Failure<CategoryDto>(CategoryErrors.NotFound);

        if (category.IsGlobal)
            return Result.Failure<CategoryDto>(CategoryErrors.GlobalReadOnly);

        if (!category.IsOwnedBy(request.CurrentUserId))
            return Result.Failure<CategoryDto>(CategoryErrors.AccessDenied);

        var updateResult = category.Update(request.Data.Label, request.Data.Color);
        if (updateResult.IsFailure)
            return Result.Failure<CategoryDto>(updateResult.Error);

        repository.Update(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(TaxonomyMapper.ToDto(category));
    }
}

// ── Delete ───────────────────────────────────────────────────────────────────

public sealed record DeleteCategoryCommand(UserId CurrentUserId, CategoryId CategoryId) : Command;

public sealed class DeleteCategoryHandler(ICategoryRepository repository, ITaxonomyUnitOfWork unitOfWork)
    : IRequestHandler<DeleteCategoryCommand, Result>
{
    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await repository.GetByIdAccessibleByUserAsync(request.CategoryId, request.CurrentUserId, cancellationToken);
        if (category is null)
            return Result.Failure(CategoryErrors.NotFound);

        if (category.IsGlobal)
            return Result.Failure(CategoryErrors.GlobalReadOnly);

        if (!category.IsOwnedBy(request.CurrentUserId))
            return Result.Failure(CategoryErrors.AccessDenied);

        category.MarkForDeletion();
        repository.Remove(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
