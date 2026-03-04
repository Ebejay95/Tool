using SharedKernel;

namespace ImportExport.Application.UseCases;

/// <summary>
/// Jedes Modul, das Export unterstützt, registriert eine Implementierung.
/// Sie kapselt den modulspezifischen Datenabruf (Repository/Query-Service).
/// </summary>
public interface IExportSource
{
    string EntityTypeName { get; }

    /// <summary>Liefert alle Entitäten des aktuellen Users als flache Objektliste.</summary>
    Task<IReadOnlyList<object>> GetAllForUserAsync(UserId userId, CancellationToken ct = default);
}
