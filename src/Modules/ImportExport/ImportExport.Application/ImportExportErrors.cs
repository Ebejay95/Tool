using SharedKernel;

namespace ImportExport.Application;

public static class ImportExportErrors
{
    public static readonly Error UnknownEntityType =
        new("ImportExport.UnknownEntityType", "Der angegebene Entity-Typ ist nicht als exportierbar registriert.");

    public static readonly Error UnknownChannel =
        new("ImportExport.UnknownChannel", "Der angegebene Channel ist nicht registriert.");

    public static readonly Error NoImportAdapterRegistered =
        new("ImportExport.NoImportAdapter", "Für diesen Entity-Typ ist kein Import-Adapter registriert.");

    public static readonly Error NoExportSourceRegistered =
        new("ImportExport.NoExportSource", "Für diesen Entity-Typ ist keine Export-Quelle registriert.");

    public static readonly Error ExportFailed =
        new("ImportExport.ExportFailed", "Der Export ist fehlgeschlagen.");
}
