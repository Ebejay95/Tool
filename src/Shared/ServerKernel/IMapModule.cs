using Microsoft.AspNetCore.Routing;

namespace ServerKernel;

/// <summary>
/// Kontrakt für Module, die HTTP-Endpunkte anbieten.
///
/// Vorteile gegenüber Duck Typing:
///   - Compile-Time-Sicherheit: fehlende MapEndpoints-Methode → Compilerfehler
///   - IDE-Unterstützung: "Alle Implementierungen finden" funktioniert
///   - Explizite Abhängigkeit statt Konvention
///
/// Verwendung in Modul-Klassen:
///   public sealed class MyModule : IModule, IMapModule
///   {
///       public static void MapEndpoints(IEndpointRouteBuilder app)  { ... }
///   }
///
/// Lebt in ServerKernel (nicht SharedKernel), da IEndpointRouteBuilder
/// nur in ASP.NET Core verfügbar ist und nicht in Blazor WASM genutzt werden darf.
/// </summary>
public interface IMapModule
{
    static abstract void MapEndpoints(IEndpointRouteBuilder app);
}
