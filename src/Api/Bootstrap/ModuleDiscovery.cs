using System.Reflection;
using ServerKernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System.IO;

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

        // ApplicationParts zentral registrieren – Module müssen AddControllers() nicht mehr selbst aufrufen.
        // AddControllers() ist idempotent; der bereits in Program.cs registrierte Builder wird zurückgegeben.
        var mvcBuilder = builder.Services.AddControllers();
        foreach (var type in moduleTypes)
        {
            mvcBuilder.AddApplicationPart(type.Assembly);
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

    /// <summary>
    /// Scannt alle IMigrateModule-Implementierungen und führt deren MigrateAsync() auf.
    /// Aufruf in --migrate-only-Modus.
    /// </summary>
    public static async Task MigrateAllModulesAsync(this IServiceProvider services)
    {
        var moduleTypes = FindConcreteImplementors(typeof(IMigrateModule))
            .OrderBy(t => t.GetCustomAttribute<ModuleOrderAttribute>()?.Order ?? 0);

        foreach (var type in moduleTypes)
        {
            var method = FindStaticMethod(type, nameof(IMigrateModule.MigrateAsync), 1);
            if (method != null)
                await (Task)method.Invoke(null, [services])!;
        }
    }

    // ─── Internals ────────────────────────────────────────────────────────────

    private static void EnsureDirectReferencesLoaded()
    {
        // GetReferencedAssemblies() liefert nur Assemblies die im IL-Code der Entry-Assembly
        // tatsächlich verwendet werden. Module-Assemblies (*.Api) werden dort nicht gelistet,
        // wenn Program.cs keine ihrer Typen direkt nutzt.
        // Lösung: alle DLLs im Ausgabeverzeichnis laden → garantiert dass alle Module gefunden werden.
        var alreadyLoaded = AppDomain.CurrentDomain
            .GetAssemblies()
            .Select(a => a.GetName().Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var dll in Directory.EnumerateFiles(AppContext.BaseDirectory, "*.dll"))
        {
            try
            {
                var name = AssemblyName.GetAssemblyName(dll);
                if (name.Name is null || alreadyLoaded.Contains(name.Name)) continue;
                Assembly.Load(name);
                alreadyLoaded.Add(name.Name);
            }
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
