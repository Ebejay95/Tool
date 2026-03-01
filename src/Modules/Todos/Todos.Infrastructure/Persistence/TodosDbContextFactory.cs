using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Todos.Infrastructure.Persistence;

/// <summary>
/// Ermöglicht EF Core Tools (dotnet ef migrations) das Erstellen von TodosDbContext
/// ohne laufende Anwendung und ohne Verbindung zur Datenbank.
/// Wird ausschließlich zur Design-Zeit verwendet, nicht zur Laufzeit.
/// </summary>
internal sealed class TodosDbContextFactory : IDesignTimeDbContextFactory<TodosDbContext>
{
    public TodosDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<TodosDbContext>()
            .UseNpgsql("Host=localhost;Database=design_time_placeholder;Username=x;Password=x",
                npgsql => npgsql.MigrationsAssembly(typeof(TodosDbContext).Assembly.FullName))
            .Options;

        return new TodosDbContext(options);
    }
}
