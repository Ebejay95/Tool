using MediatR;
using SharedKernel;
using Taxonomy.Application.DTOs;
using Taxonomy.Application.Ports;
using Taxonomy.Domain.Tags;

namespace Taxonomy.Application.UseCases.Commands;

// ── Create ───────────────────────────────────────────────────────────────────

public sealed record CreateTagCommand(UserId CurrentUserId, CreateTagDto Data) : Command<TagDto>;

public sealed class CreateTagHandler(ITagRepository repository, ITaxonomyUnitOfWork unitOfWork)
    : IRequestHandler<CreateTagCommand, Result<TagDto>>
{
    public async Task<Result<TagDto>> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        var existing = await repository.GetByLabelAndUserIdAsync(request.Data.Label, request.CurrentUserId, cancellationToken);
        if (existing is not null)
            return Result.Failure<TagDto>(TagErrors.LabelAlreadyExists);

        var result = Tag.Create(request.CurrentUserId, request.Data.Label, request.Data.Color);
        if (result.IsFailure)
            return Result.Failure<TagDto>(result.Error);

        repository.Add(result.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(TaxonomyMapper.ToDto(result.Value));
    }
}

// ── Update ───────────────────────────────────────────────────────────────────

public sealed record UpdateTagCommand(UserId CurrentUserId, TagId TagId, UpdateTagDto Data) : Command<TagDto>;

public sealed class UpdateTagHandler(ITagRepository repository, ITaxonomyUnitOfWork unitOfWork)
    : IRequestHandler<UpdateTagCommand, Result<TagDto>>
{
    public async Task<Result<TagDto>> Handle(UpdateTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await repository.GetByIdAccessibleByUserAsync(request.TagId, request.CurrentUserId, cancellationToken);
        if (tag is null)
            return Result.Failure<TagDto>(TagErrors.NotFound);

        if (tag.IsGlobal)
            return Result.Failure<TagDto>(TagErrors.GlobalReadOnly);

        if (!tag.IsOwnedBy(request.CurrentUserId))
            return Result.Failure<TagDto>(TagErrors.AccessDenied);

        var updateResult = tag.Update(request.Data.Label, request.Data.Color);
        if (updateResult.IsFailure)
            return Result.Failure<TagDto>(updateResult.Error);

        repository.Update(tag);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(TaxonomyMapper.ToDto(tag));
    }
}

// ── Delete ───────────────────────────────────────────────────────────────────

public sealed record DeleteTagCommand(UserId CurrentUserId, TagId TagId) : Command;

public sealed class DeleteTagHandler(ITagRepository repository, ITaxonomyUnitOfWork unitOfWork)
    : IRequestHandler<DeleteTagCommand, Result>
{
    public async Task<Result> Handle(DeleteTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await repository.GetByIdAccessibleByUserAsync(request.TagId, request.CurrentUserId, cancellationToken);
        if (tag is null)
            return Result.Failure(TagErrors.NotFound);

        if (tag.IsGlobal)
            return Result.Failure(TagErrors.GlobalReadOnly);

        if (!tag.IsOwnedBy(request.CurrentUserId))
            return Result.Failure(TagErrors.AccessDenied);

        tag.MarkForDeletion();
        repository.Remove(tag);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
