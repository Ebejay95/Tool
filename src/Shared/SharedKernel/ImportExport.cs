namespace SharedKernel;

/// <summary>
/// Marker-Interface: Jedes AggregateRoot, das dieses Interface implementiert,
/// steht automatisch im Import/Export-Registry zur Verfügung.
/// </summary>
public interface IExportable
{
    /// <summary>
    /// Eindeutiger, stabiler Typ-Schlüssel (z.B. "Todo", "Measure").
    /// Wird im MappingProfile gespeichert und vom Registry aufgelöst.
    /// </summary>
    static abstract string ExportableTypeName { get; }
}

/// <summary>
/// Markiert eine Property als exportierbar/importierbar und liefert Metadaten
/// für die Auto-Generierung von Spaltenköpfen, Mapping-Vorschlägen und UI-Labels.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class ExportFieldAttribute : Attribute
{
    /// <param name="displayName">Spaltenbezeichnung in Excel/CSV (z.B. "Titel", "Fälligkeitsdatum").</param>
    /// <param name="order">Reihenfolge in der Ausgabe (aufsteigend).</param>
    /// <param name="canImport">Ob dieses Feld beim Import beschreibbar ist.</param>
    /// <param name="canExport">Ob dieses Feld beim Export ausgelesen wird.</param>
    public ExportFieldAttribute(
        string displayName,
        int    order     = 100,
        bool   canImport = true,
        bool   canExport = true)
    {
        DisplayName = displayName;
        Order       = order;
        CanImport   = canImport;
        CanExport   = canExport;
    }

    public string DisplayName { get; }
    public int    Order       { get; }
    public bool   CanImport   { get; }
    public bool   CanExport   { get; }
}

/// <summary>
/// Beschreibt ein einzelnes export-fähiges Feld einer Entity,
/// inkl. Reflection-Metadaten. Wird durch ExportableEntityRegistry erzeugt.
/// </summary>
public sealed record ExportFieldDescriptor(
    string   PropertyName,
    string   DisplayName,
    Type     PropertyType,
    int      Order,
    bool     CanImport,
    bool     CanExport);

/// <summary>
/// Beschreibt einen registrierten exportierbaren Entitätstyp.
/// </summary>
public sealed record ExportableEntityDescriptor(
    string                          TypeName,
    Type                            ClrType,
    IReadOnlyList<ExportFieldDescriptor> Fields);
