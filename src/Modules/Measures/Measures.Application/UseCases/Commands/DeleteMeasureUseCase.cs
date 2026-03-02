using Measures.Application.Ports;
using Measures.Domain.Measures;
using MediatR;
using SharedKernel;

namespace Measures.Application.UseCases.Commands;

public sealed record DeleteMeasureCommand(UserId UserId, MeasureId MeasureId) : Command;

public sealed class DeleteMeasureHandler(IMeasureRepository repository, IMeasuresUnitOfWork unitOfWork)
    : IRequestHandler<DeleteMeasureCommand, Result>
{
    public async Task<Result> Handle(DeleteMeasureCommand request, CancellationToken cancellationToken)
    {
        var measure = await repository.GetByIdAndUserIdAsync(request.MeasureId, request.UserId, cancellationToken);
        if (measure is null)
            return Result.Failure(MeasureErrors.NotFound);

        measure.MarkForDeletion();
        repository.Remove(measure);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
