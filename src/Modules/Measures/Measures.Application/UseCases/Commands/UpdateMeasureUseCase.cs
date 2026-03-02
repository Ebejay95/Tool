using Measures.Application.DTOs;
using Measures.Application.Ports;
using Measures.Domain.Measures;
using MediatR;
using SharedKernel;

namespace Measures.Application.UseCases.Commands;

public sealed record UpdateMeasureCommand(UserId UserId, MeasureId MeasureId, UpdateMeasureDto Data) : Command<MeasureDto>;

public sealed class UpdateMeasureHandler(IMeasureRepository repository, IMeasuresUnitOfWork unitOfWork)
    : IRequestHandler<UpdateMeasureCommand, Result<MeasureDto>>
{
    public async Task<Result<MeasureDto>> Handle(UpdateMeasureCommand request, CancellationToken cancellationToken)
    {
        var measure = await repository.GetByIdAndUserIdAsync(request.MeasureId, request.UserId, cancellationToken);
        if (measure is null)
            return Result.Failure<MeasureDto>(MeasureErrors.NotFound);

        var updateResult = measure.Update(
            request.Data.Category,
            request.Data.Name,
            request.Data.CostEur,
            request.Data.EffortHours,
            request.Data.ImpactRisk,
            request.Data.Confidence,
            request.Data.Dependencies,
            request.Data.Justification,
            request.Data.ConfDataQuality,
            request.Data.ConfDataSourceCount,
            request.Data.ConfDataRecency,
            request.Data.ConfSpecificity,
            request.Data.GraphDependentsCount,
            request.Data.GraphImpactMultiplier,
            request.Data.GraphTotalCost,
            request.Data.GraphCostEfficiency);

        if (updateResult.IsFailure)
            return Result.Failure<MeasureDto>(updateResult.Error);

        measure.SetCategories(request.Data.CategoryIds);
        measure.SetTags(request.Data.TagIds);

        repository.Update(measure);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(MeasureMapper.ToDto(measure));
    }
}
