using Measures.Application.DTOs;
using Measures.Application.Ports;
using Measures.Domain.Measures;
using MediatR;
using SharedKernel;

namespace Measures.Application.UseCases.Queries;

public sealed record GetUserMeasuresQuery(UserId UserId) : Query<IReadOnlyList<MeasureDto>>;

public sealed class GetUserMeasuresHandler(IMeasureQueryService queryService)
    : IRequestHandler<GetUserMeasuresQuery, Result<IReadOnlyList<MeasureDto>>>
{
    public async Task<Result<IReadOnlyList<MeasureDto>>> Handle(GetUserMeasuresQuery request, CancellationToken cancellationToken)
    {
        var measures = await queryService.GetUserMeasuresAsync(request.UserId, cancellationToken);
        return Result.Success(measures);
    }
}

public sealed record GetUserMeasureByIdQuery(UserId UserId, MeasureId MeasureId) : Query<MeasureDto>;

public sealed class GetUserMeasureByIdHandler(IMeasureQueryService queryService)
    : IRequestHandler<GetUserMeasureByIdQuery, Result<MeasureDto>>
{
    public async Task<Result<MeasureDto>> Handle(GetUserMeasureByIdQuery request, CancellationToken cancellationToken)
    {
        var measure = await queryService.GetUserMeasureByIdAsync(request.MeasureId, request.UserId, cancellationToken);
        if (measure is null)
            return Result.Failure<MeasureDto>(Errors.General.NotFound);

        return Result.Success(measure);
    }
}
