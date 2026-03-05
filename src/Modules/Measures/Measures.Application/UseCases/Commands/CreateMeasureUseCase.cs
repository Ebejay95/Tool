using Measures.Application.DTOs;
using Measures.Application.Ports;
using Measures.Domain.Measures;
using MediatR;
using SharedKernel;

namespace Measures.Application.UseCases.Commands;

public sealed record CreateMeasureCommand(UserId UserId, CreateMeasureDto Data) : Command<MeasureDto>;

public sealed class CreateMeasureHandler(IMeasureRepository repository, IMeasuresUnitOfWork unitOfWork)
    : IRequestHandler<CreateMeasureCommand, Result<MeasureDto>>
{
    public async Task<Result<MeasureDto>> Handle(CreateMeasureCommand request, CancellationToken cancellationToken)
    {
        var existing = await repository.GetByIsoIdAndUserIdAsync(request.Data.IsoId, request.UserId, cancellationToken);
        if (existing is not null)
            return Result.Failure<MeasureDto>(MeasureErrors.IsoIdAlreadyExists);

        var result = Measure.Create(
            request.UserId,
            request.Data.IsoId,
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

        if (result.IsFailure)
            return Result.Failure<MeasureDto>(result.Error);

        var measure = result.Value;
        measure.SetCategories(request.Data.CategoryIds);
        measure.SetTags(request.Data.TagIds);
        repository.Add(measure);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(MeasureMapper.ToDto(measure));
    }
}
