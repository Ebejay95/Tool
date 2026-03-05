using ImportExport.Application.UseCases;
using Measures.Application.Ports;
using Measures.Domain.Measures;
using SharedKernel;

namespace Measures.Infrastructure.ImportExport;

/// <summary>
/// Erstellt neue Measure-Entitäten aus einer Import-Zeile.
/// Owner = aktueller User (Default-CRUD).
/// </summary>
public sealed class MeasureImportAdapter : IImportAdapter
{
    private readonly IMeasureRepository   _repository;
    private readonly IMeasuresUnitOfWork  _unitOfWork;

    public MeasureImportAdapter(IMeasureRepository repository, IMeasuresUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public string EntityTypeName => Measure.ExportableTypeName;

    public async Task<string?> ImportRowAsync(
        UserId                              ownerId,
        IReadOnlyDictionary<string, string?> row,
        CancellationToken                   ct = default)
    {
        var isoId    = row.GetValueOrDefault("IsoId")?.Trim();
        var name     = row.GetValueOrDefault("Name")?.Trim();

        if (string.IsNullOrWhiteSpace(isoId))    return "Pflichtfeld 'IsoId' (ISO-ID) fehlt.";
        if (string.IsNullOrWhiteSpace(name))     return "Pflichtfeld 'Name' fehlt.";

        // Numerische Felder mit Defaults
        if (!TryParseDecimal(row, "CostEur",        out var costEur))        costEur        = 0m;
        if (!TryParseDouble(row,  "EffortHours",    out var effortHours))    effortHours    = 0d;
        if (!TryParseInt(row,     "ImpactRisk",     out var impactRisk))     impactRisk     = 1;
        if (!TryParseInt(row,     "Confidence",     out var confidence))     confidence     = 1;
        if (!TryParseInt(row,     "ConfDataQuality",out var cdq))            cdq            = 1;
        if (!TryParseInt(row,     "ConfDataSourceCount", out var cdsc))      cdsc           = 1;
        if (!TryParseInt(row,     "ConfDataRecency",out var cdr))            cdr            = 1;
        if (!TryParseInt(row,     "ConfSpecificity",out var cs))             cs             = 1;
        if (!TryParseInt(row,     "GraphDependentsCount", out var gdc))      gdc            = 0;
        if (!TryParseDouble(row,  "GraphImpactMultiplier", out var gim))     gim            = 1d;
        if (!TryParseDecimal(row, "GraphTotalCost", out var gtc))            gtc            = 0m;
        if (!TryParseDouble(row,  "GraphCostEfficiency", out var gce))       gce            = 0d;

        var justification = row.GetValueOrDefault("Justification");
        var dependencies  = ParseDependencies(row.GetValueOrDefault("Dependencies"));

        var result = Measure.Create(
            ownerId, isoId, name,
            costEur, effortHours,
            impactRisk, confidence,
            dependencies, justification,
            cdq, cdsc, cdr, cs,
            gdc, gim, gtc, gce);

        if (result.IsFailure)
            return result.Error.Description;

        _repository.Add(result.Value);
        await _unitOfWork.SaveChangesAsync(ct);
        return null;
    }

    // ── Hilfsmethoden ─────────────────────────────────────────────────────

    private static bool TryParseInt(IReadOnlyDictionary<string, string?> row, string key, out int value)
    {
        value = 0;
        return row.TryGetValue(key, out var s) && !string.IsNullOrWhiteSpace(s) && int.TryParse(s, out value);
    }

    private static bool TryParseDecimal(IReadOnlyDictionary<string, string?> row, string key, out decimal value)
    {
        value = 0;
        return row.TryGetValue(key, out var s) && !string.IsNullOrWhiteSpace(s)
            && decimal.TryParse(s, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out value);
    }

    private static bool TryParseDouble(IReadOnlyDictionary<string, string?> row, string key, out double value)
    {
        value = 0;
        return row.TryGetValue(key, out var s) && !string.IsNullOrWhiteSpace(s)
            && double.TryParse(s, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out value);
    }

    /// <summary>
    /// Parst eine kommagetrennte Liste von ISO-IDs, z.B. "A.5.1, A.5.2".
    /// </summary>
    private static List<string> ParseDependencies(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return [];
        return raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }
}
