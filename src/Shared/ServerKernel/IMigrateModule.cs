namespace ServerKernel;

/// <summary>
/// Kontrakt für Module, die eine eigene Datenbank-Migration besitzen.
/// Implementierung in der jeweiligen Modul-Klasse; Aufruf via
/// <c>app.Services.MigrateAllModulesAsync()</c> in Program.cs.
/// </summary>
public interface IMigrateModule
{
    static abstract Task MigrateAsync(IServiceProvider serviceProvider);
}
