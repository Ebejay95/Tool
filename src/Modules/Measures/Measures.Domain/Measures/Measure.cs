using SharedKernel;

namespace Measures.Domain.Measures;

public sealed class Measure : AggregateRoot, IResourceOwner
{
    // IResourceOwner: erlaubt generischen OwnershipHandler ohne Measures-spezifischen Code in Api
    string IResourceOwner.OwnerId => UserId.Value.ToString();

    private Measure() { } // For EF

    private Measure(
        MeasureId id,
        UserId userId,
        string isoId,
        string category,
        string name,
        decimal costEur,
        double effortHours,
        int impactRisk,
        int confidence,
        List<string> dependencies,
        string? justification,
        int confDataQuality,
        int confDataSourceCount,
        int confDataRecency,
        int confSpecificity,
        int graphDependentsCount,
        double graphImpactMultiplier,
        decimal graphTotalCost,
        double graphCostEfficiency)
    {
        Id = id;
        UserId = userId;
        IsoId = isoId;
        Category = category;
        Name = name;
        CostEur = costEur;
        EffortHours = effortHours;
        ImpactRisk = impactRisk;
        Confidence = confidence;
        Dependencies = dependencies;
        Justification = justification;
        ConfDataQuality = confDataQuality;
        ConfDataSourceCount = confDataSourceCount;
        ConfDataRecency = confDataRecency;
        ConfSpecificity = confSpecificity;
        GraphDependentsCount = graphDependentsCount;
        GraphImpactMultiplier = graphImpactMultiplier;
        GraphTotalCost = graphTotalCost;
        GraphCostEfficiency = graphCostEfficiency;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new MeasureCreatedEvent(id, userId, name));
    }

    public new MeasureId Id { get; private set; } = null!;
    public UserId UserId { get; private set; } = null!;

    /// <summary>ISO-ID z. B. A.5.1</summary>
    public string IsoId { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public decimal CostEur { get; private set; }
    public double EffortHours { get; private set; }

    /// <summary>Risiko-Wirksamkeit 1–5</summary>
    public int ImpactRisk { get; private set; }

    /// <summary>Gesamt-Verlässlichkeit 1–3</summary>
    public int Confidence { get; private set; }

    /// <summary>ISO-IDs abhängiger Maßnahmen</summary>
    public List<string> Dependencies { get; private set; } = [];

    public string? Justification { get; private set; }

    public List<Guid> CategoryIds { get; private set; } = [];
    public List<Guid> TagIds { get; private set; } = [];

    public void SetCategories(IEnumerable<Guid> ids) { CategoryIds = ids.ToList(); UpdatedAt = DateTime.UtcNow; }
    public void SetTags(IEnumerable<Guid> ids)       { TagIds       = ids.ToList(); UpdatedAt = DateTime.UtcNow; }

    // Confidence-Subparameter (1–3)
    public int ConfDataQuality { get; private set; }
    public int ConfDataSourceCount { get; private set; }
    public int ConfDataRecency { get; private set; }
    public int ConfSpecificity { get; private set; }

    // Graph-Felder
    public int GraphDependentsCount { get; private set; }

    /// <summary>Multiplikator, max. 2.0</summary>
    public double GraphImpactMultiplier { get; private set; }
    public decimal GraphTotalCost { get; private set; }
    public double GraphCostEfficiency { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public static Result<Measure> Create(
        UserId userId,
        string isoId,
        string category,
        string name,
        decimal costEur,
        double effortHours,
        int impactRisk,
        int confidence,
        List<string> dependencies,
        string? justification,
        int confDataQuality,
        int confDataSourceCount,
        int confDataRecency,
        int confSpecificity,
        int graphDependentsCount,
        double graphImpactMultiplier,
        decimal graphTotalCost,
        double graphCostEfficiency)
    {
        if (string.IsNullOrWhiteSpace(isoId))
            return Result.Failure<Measure>(MeasureErrors.IsoIdRequired);

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Measure>(MeasureErrors.NameRequired);

        if (impactRisk is < 1 or > 5)
            return Result.Failure<Measure>(MeasureErrors.ImpactRiskOutOfRange);

        if (confidence is < 1 or > 3)
            return Result.Failure<Measure>(MeasureErrors.ConfidenceOutOfRange);

        if (graphImpactMultiplier > 2.0)
            return Result.Failure<Measure>(MeasureErrors.ImpactMultiplierTooHigh);

        var measure = new Measure(
            MeasureId.New(), userId,
            isoId.Trim(), category.Trim(), name.Trim(),
            costEur, effortHours,
            impactRisk, confidence,
            dependencies, justification,
            confDataQuality, confDataSourceCount,
            confDataRecency, confSpecificity,
            graphDependentsCount, graphImpactMultiplier,
            graphTotalCost, graphCostEfficiency);

        return Result.Success(measure);
    }

    public Result Update(
        string category,
        string name,
        decimal costEur,
        double effortHours,
        int impactRisk,
        int confidence,
        List<string> dependencies,
        string? justification,
        int confDataQuality,
        int confDataSourceCount,
        int confDataRecency,
        int confSpecificity,
        int graphDependentsCount,
        double graphImpactMultiplier,
        decimal graphTotalCost,
        double graphCostEfficiency)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(MeasureErrors.NameRequired);

        if (impactRisk is < 1 or > 5)
            return Result.Failure(MeasureErrors.ImpactRiskOutOfRange);

        if (confidence is < 1 or > 3)
            return Result.Failure(MeasureErrors.ConfidenceOutOfRange);

        if (graphImpactMultiplier > 2.0)
            return Result.Failure(MeasureErrors.ImpactMultiplierTooHigh);

        Category = category.Trim();
        Name = name.Trim();
        CostEur = costEur;
        EffortHours = effortHours;
        ImpactRisk = impactRisk;
        Confidence = confidence;
        Dependencies = dependencies;
        Justification = justification;
        ConfDataQuality = confDataQuality;
        ConfDataSourceCount = confDataSourceCount;
        ConfDataRecency = confDataRecency;
        ConfSpecificity = confSpecificity;
        GraphDependentsCount = graphDependentsCount;
        GraphImpactMultiplier = graphImpactMultiplier;
        GraphTotalCost = graphTotalCost;
        GraphCostEfficiency = graphCostEfficiency;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new MeasureUpdatedEvent(Id, UserId, Name));

        return Result.Success();
    }

    public void MarkForDeletion()
    {
        AddDomainEvent(new MeasureDeletedEvent(Id, UserId, Name));
    }
}
