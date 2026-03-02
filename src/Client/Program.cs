using Identity.Application;
using App.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App.Root>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ── HTTP Client ────────────────────────────────────────────────────────────
// Basis-URL = Origin des Servers (Hosted WASM: gleicher Host wie API)
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// ── Auth + Token ───────────────────────────────────────────────────────────
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<IAuthService, AuthApiService>();

// ── Fach-Services (HTTP-basiert, ersetzen die MediatR-Direct-Calls) ────────
builder.Services.AddScoped<TodoApiService>();
builder.Services.AddScoped<MeasureApiService>();
builder.Services.AddScoped<TaxonomyApiService>();
builder.Services.AddScoped<NotificationHubService>();
builder.Services.AddScoped<ThemeService>();

// ── UI ─────────────────────────────────────────────────────────────────────
builder.Services.AddMudServices();

await builder.Build().RunAsync();
