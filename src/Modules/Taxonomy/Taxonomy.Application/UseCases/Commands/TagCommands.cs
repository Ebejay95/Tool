using MediatR;
using SharedKernel;
using Taxonomy.Application.DTOs;
using Taxonomy.Application.Ports;
using Taxonomy.Domain.Tags;

namespace Taxonomy.Application.UseCases.Commands;

public sealed record CreateTagCommand(CreateTagDto Data) : Command<TagDto>;
public sealed class CreateTagHandler(ITagRepository repository, ITaxonomyUnitOfWork unitOfWork)
    : IRequestHandler<CreateTagCommand, Result<TagDto>>
{
    public async Task<Result<TagDto>> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        var existing = await repository.GetByLabelAsync(request.Data.Label, cancellationToken);
        if (existing is not null)
            return Result.Failure<TagDto>(TagErrors.LabelAlreadyExists);

        var result = Tag.Create(request.Data.Label, request.Data.Color);
        if (result.IsFailure)
            return Result.Failure<TagDto>(result.Error);

        repository.Add(result.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(TaxonomyMapper.ToDto(result.Value));
    }
}

public sealed record UpdateTagCommand(TagId TagId, UpdateTagDto Data) : Command<TagDto>;
public sealed class UpdateTagHandler(ITagRepository repository, ITaxonomyUnitOfWork unitOfWork)
    : IRequestHandler<UpdateTagCommand, Result<TagDto>>
{
    public async Task<Result<TagDto>> Handle(UpdateTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await repository.GetByIdAsync(request.TagId, cancellationToken);
        if (tag is null)
            return Result.Failure<TagDto>(TagErrors.NotFound);

        var updateResult = tag.Update(request.Data.Label, request.Data.Color);
        if (updateResult.IsFailure)
            return Result.Failure<TagDto>(updateResult.Error);

        repository.Update(tag);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(TaxonomyMapper.ToDto(tag));
    }
}

public sealed record DeleteTagCommand(TagId TagId) : Command;
public sealed class DeleteTagHandler(ITagRepository repository, ITaxonomyUnitOfWork unitOfWork)
    : IRequestHandler<DeleteTagCommand, Result>
{
    public async Task<Result> Handle(DeleteTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await repository.GetByIdAsync(request.TagId, cancellationToken);
        if (tag is null)
            return Result.Failure(TagErrors.NotFound);

        tag.MarkForDeletion();
        repository.Remove(tag);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
