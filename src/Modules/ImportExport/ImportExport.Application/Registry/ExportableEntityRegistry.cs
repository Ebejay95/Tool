using System.Reflection;
using SharedKernel;
using Microsoft.Extensions.Logging;

namespace ImportExport.Application.Registry;

/// <summary>
/// Singleton-Service: Scannt beim Start alle registrierten Assemblies nach
/// Typen, die <see cref="IExportable"/> implementieren, und hält die
/// <see cref="ExportableEntityDescriptor"/>-Metadaten im Speicher.
/// </summary>
public sealed class ExportableEntityRegistry
{
    private readonly Dictionary<string, ExportableEntityDescriptor> _descriptors = new();
    private readonly ILogger<ExportableEntityRegistry> _logger;

    public ExportableEntityRegistry(ILogger<ExportableEntityRegistry> logger)
    {
        _logger = logger;
    }

    /// <summary>Registriert alle IExportable-Typen aus den übergebenen Assemblies.</summary>
    public void RegisterAssemblies(IEnumerable<Assembly> assemblies)
    {
        var exportableInterface = typeof(IExportable);

        foreach (var assembly in assemblies)
        {
            IEnumerable<Type> candidates;
            try
            {
                candidates = assembly.GetTypes()
                    .Where(t => t is { IsClass: true, IsAbstract: false }
                                && t.IsAssignableTo(exportableInterface));
            }
            catch (ReflectionTypeLoadException ex)
            {
                _logger.LogWarning(ex, "Konnte Assembly {Assembly} nicht vollständig analysieren.", assembly.FullName);
                candidates = ex.Types.OfType<Type>()
                    .Where(t => t.IsAssignableTo(exportableInterface));
            }

            foreach (var type in candidates)
            {
                var typeName = GetTypeName(type);
                if (_descriptors.ContainsKey(typeName))
                {
                    _logger.LogWarning(
                        "Doppelter ExportableTypeName '{TypeName}' in {Type}. Wird übersprungen.", typeName, type.FullName);
                    continue;
                }

                var fields = BuildFieldDescriptors(type);
                _descriptors[typeName] = new ExportableEntityDescriptor(typeName, type, fields);
                _logger.LogInformation("Exportierbarer Typ registriert: {TypeName} ({Fields} Felder)", typeName, fields.Count);
            }
        }
    }

    public IReadOnlyList<ExportableEntityDescriptor> GetAll()
        => _descriptors.Values.ToList().AsReadOnly();

    public ExportableEntityDescriptor? TryGet(string typeName)
        => _descriptors.TryGetValue(typeName, out var d) ? d : null;

    public ExportableEntityDescriptor Get(string typeName)
        => TryGet(typeName) ?? throw new KeyNotFoundException($"Kein exportierbarer Typ '{typeName}' gefunden.");

    // ── Hilfsmethoden ─────────────────────────────────────────────────────

    private static string GetTypeName(Type type)
    {
        // IExportable.ExportableTypeName ist ein static abstract member (C# 11)
        var prop = type.GetProperty(nameof(IExportable.ExportableTypeName),
            BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

        return prop?.GetValue(null) as string
               ?? type.Name; // Fallback auf Klassenname
    }

    private static IReadOnlyList<ExportFieldDescriptor> BuildFieldDescriptors(Type type)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => (Property: p, Attr: p.GetCustomAttribute<ExportFieldAttribute>()))
            .Where(x => x.Attr is not null)
            .OrderBy(x => x.Attr!.Order)
            .Select(x => new ExportFieldDescriptor(
                PropertyName:  x.Property.Name,
                DisplayName:   x.Attr!.DisplayName,
                PropertyType:  x.Property.PropertyType,
                Order:         x.Attr.Order,
                CanImport:     x.Attr.CanImport,
                CanExport:     x.Attr.CanExport))
            .ToList()
            .AsReadOnly();
    }
}
