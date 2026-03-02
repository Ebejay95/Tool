using SharedKernel;

namespace Measures.Domain.Measures;

public static class MeasureErrors
{
    public static readonly Error IsoIdRequired =
        new("Measure.IsoIdRequired", "ISO-ID is required");

    public static readonly Error NameRequired =
        new("Measure.NameRequired", "Name is required");

    public static readonly Error ImpactRiskOutOfRange =
        new("Measure.ImpactRiskOutOfRange", "Impact risk must be between 1 and 5");

    public static readonly Error ConfidenceOutOfRange =
        new("Measure.ConfidenceOutOfRange", "Confidence must be between 1 and 3");

    public static readonly Error ImpactMultiplierTooHigh =
        new("Measure.ImpactMultiplierTooHigh", "Graph impact multiplier cannot exceed 2.0");

    public static readonly Error NotFound =
        new("Measure.NotFound", "Measure was not found");

    public static readonly Error IsoIdAlreadyExists =
        new("Measure.IsoIdAlreadyExists", "A measure with this ISO-ID already exists");

    public static readonly Error AccessDenied =
        new("Measure.AccessDenied", "You do not have access to this measure");
}
