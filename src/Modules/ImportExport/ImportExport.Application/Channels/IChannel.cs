using ImportExport.Domain.Profiles;

namespace ImportExport.Application.Channels;

/// <summary>
/// Repräsentiert eine Zeile aus einer Import-Datei als Schlüssel-Wert-Dictionary.
/// Key = Spaltenname der Datei, Value = Rohwert als String.
/// </summary>
public sealed record ImportRow(IReadOnlyDictionary<string, string?> Values);

/// <summary>
/// Abstrahiert das Einlesen (Parsen) einer Import-Datei.
/// Jeder Channel (CSV, Excel, …) implementiert dieses Interface.
/// </summary>
public interface IImportChannel
{
    /// <summary>Eindeutiger Channel-Schlüssel, z.B. "Csv" oder "Excel".</summary>
    string Key { get; }

    string DisplayName { get; }

    /// <summary>MIME-Types, die dieser Channel akzeptiert.</summary>
    IReadOnlyList<string> AcceptedMimeTypes { get; }

    /// <summary>Parst einen Dateistream und liefert alle Datenzeilen.</summary>
    Task<IReadOnlyList<ImportRow>> ParseAsync(Stream stream, CancellationToken ct = default);
}

/// <summary>
/// Abstrahiert das Schreiben (Serialisieren) von Export-Daten.
/// Jeder Channel (CSV, Excel, …) implementiert dieses Interface.
/// </summary>
public interface IExportChannel
{
    /// <summary>Eindeutiger Channel-Schlüssel.</summary>
    string Key { get; }

    string DisplayName { get; }

    /// <summary>Dateiendung inkl. Punkt, z.B. ".xlsx" oder ".csv".</summary>
    string FileExtension { get; }

    string ContentType { get; }

    /// <summary>
    /// Serialisiert die übergebenen Zeilen (Key = Spaltenname, Value = Rohwert)
    /// in einen Byte-Array.
    /// </summary>
    Task<byte[]> WriteAsync(
        IEnumerable<string>                   headers,
        IEnumerable<IReadOnlyDictionary<string, string?>> rows,
        CancellationToken ct = default);
}
