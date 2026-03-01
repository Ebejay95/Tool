using System.Reflection;
using ServerKernel;
using SharedKernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Api.Bootstrap;

/// <summary>
/// Assembly-basierter Auto-Discovery-Mechanismus für IModule / IMapModule.
///
/// Verwendung in Program.cs:
///   builder.AddAllModules();          // registriert alle IModule-Implementierungen
///   app.MapAllModuleEndpoints();       // mappt alle IMapModule-Endpunkte
///
/// Ein neues Modul wird automatisch erkannt sobald sein Web-Layer-Assembly
/// von Api referenziert wird – ohne Änderung an Program.cs.
///
/// Reihenfolge: [ModuleOrder(n)] aufsteigend. Default = 0.
///
/// Endpoint-Mapping: Module die Endpunkte anbieten implementieren IMapModule
/// (aus ServerKernel) und erhalten damit Compile-Time-Sicherheit.
/// SharedKernel bleibt WASM-kompatibel, da IEndpointRouteBuilder nicht dort lebt.
/// </summary>
public static class ModuleDiscovery
{
    /// <summary>
    /// Scannt alle direkten Referenzierungen des Entry-Assembly nach IModule-Implementierungen
    /// und ruft deren AddModule() auf, sortiert nach [ModuleOrder].
    /// </summary>
    public static WebApplicationBuilder AddAllModules(this WebApplicationBuilder builder)
    {
        EnsureDirectReferencesLoaded();

        var moduleTypes = FindConcreteImplementors(typeof(IModule))
            .OrderBy(t => t.GetCustomAttribute<ModuleOrderAttribute>()?.Order ?? 0)
            .ToList();

        var duplicates = moduleTypes
            .Where(t => t.GetCustomAttribute<ModuleOrderAttribute>() is not null)
            .GroupBy(t => t.GetCustomAttribute<ModuleOrderAttribute>()!.Order)
            .Where(g => g.Count() > 1)
            .Select(g => $"Order {g.Key}: [{string.Join(", ", g.Select(t => t.Name))}]")
            .ToList();

        if (duplicates.Count > 0)
            throw new InvalidOperationException(
                $"Doppelte [ModuleOrder]-Werte gefunden – Registrierungsreihenfolge wäre nichtdeterministisch: {string.Join(" | ", duplicates)}");

        foreach (var type in moduleTypes)
        {
            var method = FindStaticMethod(type, nameof(IModule.AddModule), 3);
            method?.Invoke(null, [builder.Services, builder.Configuration, builder.Environment]);
        }

        return builder;
    }

    /// <summary>
    /// Scannt alle IMapModule-Implementierungen und ruft deren MapEndpoints() auf.
    /// Compile-Time-Sicherheit: nur Klassen mit expliziter IMapModule-Implementierung werden gefunden.
    /// Aufruf nach app.UseRouting().
    /// </summary>
    public static IEndpointRouteBuilder MapAllModuleEndpoints(this IEndpointRouteBuilder app)
    {
        var moduleTypes = FindConcreteImplementors(typeof(IMapModule))
            .OrderBy(t => t.GetCustomAttribute<ModuleOrderAttribute>()?.Order ?? 0);

        foreach (var type in moduleTypes)
        {
            var method = FindStaticMethod(type, nameof(IMapModule.MapEndpoints), 1);
            method?.Invoke(null, [app]);
        }

        return app;
    }

    // ─── Internals ────────────────────────────────────────────────────────────

    private static void EnsureDirectReferencesLoaded()
    {
        var alreadyLoaded = AppDomain.CurrentDomain
            .GetAssemblies()
            .Select(a => a.FullName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly is null) return;

        foreach (var refName in entryAssembly.GetReferencedAssemblies())
        {
            if (alreadyLoaded.Contains(refName.FullName)) continue;
            try { Assembly.Load(refName); }
            catch { /* System-/nicht-ladbare Assemblies überspringen */ }
        }
    }

    private static IEnumerable<Type> FindConcreteImplementors(Type interfaceType)
        => AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(SafeGetTypes)
            .Where(t => t is { IsAbstract: false, IsInterface: false }
                        && interfaceType.IsAssignableFrom(t));

    private static MethodInfo? FindStaticMethod(Type type, string name, int paramCount)
        => type.GetMethods(BindingFlags.Public | BindingFlags.Static)
               .FirstOrDefault(m => m.Name == name && m.GetParameters().Length == paramCount);

    private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
    {
        try { return assembly.GetTypes(); }
        catch (ReflectionTypeLoadException ex) { return ex.Types.OfType<Type>(); }
        catch { return []; }
    }
}
