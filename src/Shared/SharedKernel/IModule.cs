using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SharedKernel;

/// <summary>
/// Kontrakt für selbst-registrierende Module.
/// Jeder Modul-Web-Layer implementiert dieses Interface und kapselt
/// seine eigene DI-Registration (Application + Infrastructure).
/// </summary>
public interface IModule
{
    static abstract IServiceCollection AddModule(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment);
}

/// <summary>
/// Steuert die Reihenfolge, in der Module während des Auto-Discovery registriert werden.
/// Niedrigere Werte laufen zuerst. Default = 0.
/// Beispiel: [ModuleOrder(100)] für Module, die auf SignalR angewiesen sind.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ModuleOrderAttribute(int order = 0) : Attribute
{
    public int Order { get; } = order;
}
