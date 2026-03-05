namespace Measures.Application.DTOs;

public sealed class CreateMeasureDto
{
    public string IsoId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal CostEur { get; set; }
    public double EffortHours { get; set; }
    public int ImpactRisk { get; set; }
    public int Confidence { get; set; }
    public List<string> Dependencies { get; set; } = [];
    public string? Justification { get; set; }
    public int ConfDataQuality { get; set; }
    public int ConfDataSourceCount { get; set; }
    public int ConfDataRecency { get; set; }
    public int ConfSpecificity { get; set; }
    public int GraphDependentsCount { get; set; }
    public double GraphImpactMultiplier { get; set; }
    public decimal GraphTotalCost { get; set; }
    public double GraphCostEfficiency { get; set; }
    public List<Guid> CategoryIds { get; set; } = [];
    public List<Guid> TagIds { get; set; } = [];
}

public sealed class UpdateMeasureDto
{
    public string Name { get; set; } = string.Empty;
    public decimal CostEur { get; set; }
    public double EffortHours { get; set; }
    public int ImpactRisk { get; set; }
    public int Confidence { get; set; }
    public List<string> Dependencies { get; set; } = [];
    public string? Justification { get; set; }
    public int ConfDataQuality { get; set; }
    public int ConfDataSourceCount { get; set; }
    public int ConfDataRecency { get; set; }
    public int ConfSpecificity { get; set; }
    public int GraphDependentsCount { get; set; }
    public double GraphImpactMultiplier { get; set; }
    public decimal GraphTotalCost { get; set; }
    public double GraphCostEfficiency { get; set; }
    public List<Guid> CategoryIds { get; set; } = [];
    public List<Guid> TagIds { get; set; } = [];
}

public sealed record MeasureDto(
    string Id,
    string UserId,
    string IsoId,
    string Name,
    decimal CostEur,
    double EffortHours,
    int ImpactRisk,
    int Confidence,
    List<string> Dependencies,
    string? Justification,
    int ConfDataQuality,
    int ConfDataSourceCount,
    int ConfDataRecency,
    int ConfSpecificity,
    int GraphDependentsCount,
    double GraphImpactMultiplier,
    decimal GraphTotalCost,
    double GraphCostEfficiency,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<Guid> CategoryIds,
    List<Guid> TagIds);

public sealed record MeasureSummaryDto(
    string Id,
    string IsoId,
    string Name,
    decimal CostEur,
    int ImpactRisk,
    int Confidence,
    List<Guid> CategoryIds,
    List<Guid> TagIds);
