using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CMC.Web.Components;
using CMC.Web.Components.Account;
using CMC.Persistence;
using CMC.Notifications.Socket;
using CMC.Todos.Application;
using CMC.Todos.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly(typeof(Program).Assembly.FullName)));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;

        options.Password.RequiredLength = 12;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services
    .AddDataProtection()
    .PersistKeysToDbContext<ApplicationDbContext>();

builder.Services.AddSocketEmailNotifications(builder.Configuration);
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, NotificationEmailSender>();

builder.Services.AddTodosModule();

builder.Services.AddAuthorization();
builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    // In Kubernetes/Ingress scenarios we usually don't know proxies ahead of time.
    KnownNetworks = { },
    KnownProxies = { },
});

if (app.Configuration.GetValue("HTTPS_REDIRECT", true))
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

if (app.Configuration.GetValue("MIGRATE_ON_STARTUP", false))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

if (args.Contains("--seed", StringComparer.OrdinalIgnoreCase))
{
    await SeedMasterUserAsync(app);
    return;
}

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.MapGroup("/api/todos")
    .RequireAuthorization()
    .MapTodosApi();

app.Run();

static async Task SeedMasterUserAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    var email = app.Configuration["MASTER_EMAIL"];
    var password = app.Configuration["MASTER_PASSWORD"];
    var role = app.Configuration["MASTER_ROLE"] ?? "super-admin";

    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
    {
        throw new InvalidOperationException("Seeding requires MASTER_EMAIL and MASTER_PASSWORD.");
    }

    if (!await roleManager.RoleExistsAsync(role))
    {
        await roleManager.CreateAsync(new IdentityRole(role));
    }

    var user = await userManager.FindByEmailAsync(email);
    if (user is null)
    {
        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
        };
        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException("Failed to create master user: " + string.Join("; ", createResult.Errors.Select(e => e.Description)));
        }
    }

    if (!await userManager.IsInRoleAsync(user, role))
    {
        await userManager.AddToRoleAsync(user, role);
    }
}

static class TodosApi
{
    public static RouteGroupBuilder MapTodosApi(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (ClaimsPrincipal user, ITodoService todos, CancellationToken ct) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();
            return Results.Ok(await todos.GetForUserAsync(userId, ct));
        });

        group.MapPost("/", async (ClaimsPrincipal user, ITodoService todos, CreateTodoRequest req, CancellationToken ct) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();
            var created = await todos.CreateAsync(userId, req.Title, ct);
            return Results.Created($"/api/todos/{created.Id}", created);
        });

        group.MapPut("/{id:guid}/title", async (ClaimsPrincipal user, ITodoService todos, Guid id, UpdateTitleRequest req, CancellationToken ct) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();
            return Results.Ok(await todos.UpdateTitleAsync(userId, id, req.Title, ct));
        });

        group.MapPut("/{id:guid}/completed", async (ClaimsPrincipal user, ITodoService todos, Guid id, SetCompletedRequest req, CancellationToken ct) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();
            return Results.Ok(await todos.SetCompletedAsync(userId, id, req.IsCompleted, ct));
        });

        group.MapDelete("/{id:guid}", async (ClaimsPrincipal user, ITodoService todos, Guid id, CancellationToken ct) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();
            await todos.DeleteAsync(userId, id, ct);
            return Results.NoContent();
        });

        return group;
    }
}

sealed record CreateTodoRequest(string Title);
sealed record UpdateTitleRequest(string Title);
sealed record SetCompletedRequest(bool IsCompleted);
