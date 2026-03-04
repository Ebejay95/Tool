using SharedKernel;
using ImportExport.Contracts;

namespace ImportExport.Application.UseCases;

/// <summary>
/// Adapter-Interface: Jedes Modul, das Import unterstützt,
/// registriert einen konkreten Adapter für seinen Entity-Typ.
/// Der Adapter übernimmt die modulspezifische Erstellungslogik.
/// </summary>
public interface IImportAdapter
{
    /// <summary>Übereinstimmender ExportableTypeName.</summary>
    string EntityTypeName { get; }

    /// <summary>
    /// Empfängt eine einzelne geparste Zeile (Key = Property-Name, Value = String-Wert)
    /// sowie den aktuellen User als Default-Owner und erzeugt die Entity.
    /// Gibt eine Fehler-Beschreibung zurück, null bei Erfolg.
    /// </summary>
    Task<string?> ImportRowAsync(
        UserId                         ownerId,
        IReadOnlyDictionary<string, string?> row,
        CancellationToken              ct = default);
}
