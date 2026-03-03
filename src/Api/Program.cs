using Api.Bootstrap;
using Api.Authorization;
using Api.Extensions;
using Api.Middleware;
using Microsoft.AspNetCore.Authorization;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Serilog;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ───────────────────────────────────────────────────────────────
builder.Host.UseSerilog((ctx, lc) =>
{
    lc.ReadFrom.Configuration(ctx.Configuration)
      .Enrich.FromLogContext()
      .Enrich.WithProperty("Application", "Api")
      .Enrich.WithProperty("Environment", ctx.HostingEnvironment.EnvironmentName);

    if (ctx.HostingEnvironment.IsDevelopment())
        lc.WriteTo.Console();
    else
        lc.WriteTo.Console(new CompactJsonFormatter());

    var seqUrl = ctx.Configuration["Seq:ServerUrl"];
    if (!string.IsNullOrWhiteSpace(seqUrl))
        lc.WriteTo.Seq(seqUrl,
            apiKey: ctx.Configuration["Seq:ApiKey"]);
});

// Add services to the container
// Add API Controllers
builder.Services.AddControllers(opts =>
    opts.Conventions.Add(new DevOnlyConvention(builder.Environment))); // REST API controllers – ApplicationParts werden von Modulen selbst registriert
builder.Services.AddEndpointsApiExplorer(); // API Documentation
builder.Services.AddSwaggerGen(); // Swagger UI for API Tests

// ── Module Registration (auto-discovery via ModuleDiscovery) ────────────
builder.AddAllModules();

// Add Authorization policies (Authentication is already configured by Identity Infrastructure)
builder.Services.AddAuthorization(options =>
{
    // Define specific policies instead of global requirements
    options.AddPolicy(Policies.RequireAuthenticatedUser, policy =>
        policy.RequireAuthenticatedUser());

    options.AddPolicy(Policies.RequireOwnership, policy =>
        policy.Requirements.Add(new OwnershipRequirement()));
});

// Register authorization handlers
builder.Services.AddScoped<IAuthorizationHandler, OwnershipHandler>();

// Add cross-cutting concerns
builder.Services.AddCurrentUser();
builder.Services.AddGlobalExceptionHandling();

// Add Health Checks
builder.Services.AddHealthChecks();

// ── OpenTelemetry ─────────────────────────────────────────────────────────────
var otlpEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"];

builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(r => r
        .AddService(
            serviceName:    "Api",
            serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "0.0.0")
        .AddAttributes(new Dictionary<string, object>
        {
            ["deployment.environment"] = builder.Environment.EnvironmentName.ToLowerInvariant()
        }))
    .WithTracing(t =>
    {
        t.AddAspNetCoreInstrumentation(o => o.RecordException = true)
         .AddHttpClientInstrumentation();
        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
            t.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
    })
    .WithMetrics(m =>
    {
        m.AddAspNetCoreInstrumentation()
         .AddHttpClientInstrumentation()
         .AddRuntimeInstrumentation();
        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
            m.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
    });

// CORS – offen in Dev, konfigurierbar in Prod (Cors:AllowedOrigins in appsettings)
builder.Services.AddCors(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.AddDefaultPolicy(policy =>
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
    }
    else
    {
        var origins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];
        options.AddDefaultPolicy(policy =>
            policy.WithOrigins(origins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials());
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseCors();

// Global exception handling
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();
// WASM-Dateien aus Client als Static Web Assets ausliefern
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// WebSocket support (benötigt von SignalR)
app.UseWebSockets();

// Map endpoints
app.MapAllModuleEndpoints(); // SignalR-Hubs und weitere Modul-Endpunkte
app.MapControllers();
// WASM SPA-Fallback: Alle nicht-API-Routen liefern index.html zurück
app.MapFallbackToFile("index.html");

// Health Check endpoint
app.MapHealthChecks("/health");

// --migrate-only: Nur Migrations ausführen, dann beenden (K8s Job, k8s/prod/migrate-job.yaml)
// In Dev: Makefile-Target `make migrate` oder `dotnet run -- --migrate-only` nutzen
if (args.Contains("--migrate-only"))
{
    await app.Services.MigrateAllModulesAsync();
    Console.WriteLine("Migrations completed. Exiting (--migrate-only mode).");
    return;
}

app.Run();
