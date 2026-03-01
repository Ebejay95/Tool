# Tool

Full web application based on **.NET 8 / Blazor WASM** with a **Modular Monolith** backend and Kubernetes deployment.

---

## Technology Stack

| Layer | Technology |
|---|---|
| Frontend | Blazor WebAssembly (.NET 8) |
| Backend | ASP.NET Core 8 Web API (Hosted WASM) |
| Database | PostgreSQL 15 (via Npgsql / EF Core) |
| Real-Time | SignalR + Redis Backplane |
| Cache / Pub-Sub | Redis |
| ORM / Migrations | Entity Framework Core 8 |
| Auth | JWT Bearer (stateless) |
| Logging | Serilog → Console (Dev) / Seq (Prod) |
| Tracing / Metrics | OpenTelemetry (OTLP Export) |
| Container | Docker (multi-stage build) |
| Orchestration | Kubernetes + Helm-style YAML manifests |
| Autoscaling | Kubernetes HPA (min 2 / max 10 replicas) |

---

## Architecture

### Modular Monolith

The backend is structured as a **Modular Monolith**: a single deployment artifact, but with clearly separated modules that function internally like independent services.

```
src/
├── Api/                        ← Entry point (Program.cs, Bootstrap)
├── Client/                     ← Blazor WASM (runs in the browser)
├── Modules/
│   ├── Identity/               ← Authentication & users
│   ├── Notifications/          ← Push notifications (SignalR, email, persistent)
│   └── Todos/                  ← Example business module
└── Shared/
    ├── SharedKernel/           ← WASM-safe: DDD building blocks, Result<T>, Outbox entity
    ├── ServerKernel/           ← Server-only: IMapModule (ASP.NET Core)
    └── Notifications.Abstractions/ ← Interfaces & channel constants
```

Each module follows the **Vertical Slice** structure:

```
Module/
├── Domain/          ← Entities, Value Objects, Domain Events, Repository interfaces
├── Application/     ← Use Cases (MediatR Commands/Queries), DTOs, Ports
├── Infrastructure/  ← EF DbContext, Repositories, Outbox Processor, external services
└── Api/             ← Controllers, Blazor components, Endpoint mapping (IMapModule)
```

### Module Coupling

Modules communicate **exclusively via Domain Events** — never through direct service references. The flow:

1. Module A writes the Domain Event + business data **atomically** into a database transaction (Outbox Pattern)
2. The module's `OutboxProcessor` reads unprocessed events and dispatches them via MediatR
3. An event handler in Module B (e.g. `Notifications.Application`) reacts to it

Direct module access via project references is architecturally excluded — `Todos.Application` has no knowledge of `Notifications.Application`.

---

## Features & Functionality

### Auto-Discovery (Modules)

`ModuleDiscovery` scans all referenced assemblies at startup for implementations of `IModule` (service registration) and `IMapModule` (endpoint mapping). A new module is automatically discovered as soon as its assembly is referenced in the Api project — `Program.cs` remains untouched.

- `[ModuleOrder(n)]` attribute controls the registration order
- Duplicate order values are checked at startup (`InvalidOperationException`)
- Compile-time safety through `IMapModule` (static abstract interface member)

### Authentication

- **JWT Bearer** — stateless, no session state, multi-pod capable without sticky sessions
- Token generation and validation encapsulated in the `Identity` module
- Blazor WASM stores the token in `TokenService` (in-memory + LocalStorage option)
- Authorization policies: `RequireAuthenticatedUser`, `RequireOwnership` (resource-based)

### Transactional Outbox (at-least-once Delivery)

Each business module and the Identity core use the **Transactional Outbox Pattern**:

- Domain Events are stored together with business data in **a single** database transaction
- `OutboxProcessor` (Background Service, one per module) reads unprocessed events and dispatches them via MediatR
- `FOR UPDATE SKIP LOCKED` (PostgreSQL-native) prevents double processing across multiple running instances
- **Retry mechanism**: `RetryCount` is incremented on each failure; after `MaxRetries = 5` the message is no longer retried automatically and a warning is logged
- Events that reach `MaxRetries` remain visible in the DB (no silent deletion)

### Notifications (Multi-Channel)

The `NotificationPublisher` dispatches a `NotificationMessage` to any number of channels simultaneously:

| Channel | Transport | Multi-Pod |
|---|---|---|
| `signalr` | SignalR Hub + Redis Backplane | ✅ |
| `email` | SMTP (Socket) / Simulation in Dev | ✅ |
| `persistent` | PostgreSQL via `NotificationsDbContext` | ✅ |

In local dev mode, the email channel only simulates delivery (no actual SMTP sending).

### Real-Time (SignalR)

- SignalR Hub at `/hubs/notifications`
- Redis Backplane (`AddStackExchangeRedis`) — messages work across pods
- Blazor WASM client connects via `HubConnection` and displays notifications as a snackbar
- Raw WebSocket path was intentionally removed (was pod-local and not backplane-capable)

### Observability

- **Serilog**: structured logging to console (Dev) and Seq (Prod); JSON format in Prod
- **OpenTelemetry**: Tracing (ASP.NET Core + HttpClient, RecordException) + Metrics (Runtime, ASP.NET Core, HttpClient); OTLP export configurable
- **Health Check** at `/health` (used by Kubernetes Readiness & Liveness Probes)
- **Swagger UI** in dev mode at `/swagger`

### Database

- Three independent `DbContext` instances (Identity, Todos, Notifications) — could point to separate databases
- EF Core Migrations per module (separate `MigrationsAssembly`)
- Migration as a **separate Kubernetes Job** (`migrate-job.yaml`) — not at app startup
- `--migrate-only` flag for direct CLI invocation
- `DateTimeOffset` used throughout (no `DateTime`)

### Kubernetes & Deployment

- **HPA**: min 2 / max 10 replicas (CPU ≥ 65% or Memory ≥ 75%)
- **PodDisruptionBudget**: at least 1 pod always available (rolling deployments without downtime)
- Scale-up: +2 pods/minute, Scale-down: −1 pod/minute after 5 minutes stabilization
- Init container waits for Postgres readiness before app startup
- `MIGRATE_ON_STARTUP=false` in Prod (migration via dedicated K8s Job)

### CORS

- Dev: fully open (`AllowAnyOrigin`)
- Prod: `AllowedOrigins` from `appsettings.json`, with `AllowCredentials` for SignalR

### Blazor WASM (Client)

- Hosted WASM — server serves WASM files as Static Web Assets
- SPA fallback: all non-API routes return `index.html`
- `ThemeService` (Dark/Light Mode)
- `AuthApiService`, `TodoApiService`, `NotificationHubService` as injectable services
- `TokenService` encapsulates JWT storage and provision

---

## Development

```bash
make dev        # Start K8s DB + Seq + Redis, run migrations, hot-reload
make clean      # Stop dev processes (DB is preserved)
make db-clean   # Completely delete DB data
make status     # K8s pod status
make logs       # Postgres logs
make seq-logs   # Seq logs (structured logging)
```

After `make dev`:
- App: http://localhost:8080
- Swagger: http://localhost:8080/swagger
- Seq: http://localhost:5341

## Production

```bash
make prod       # Build Docker image, push to registry, deploy k8s/prod/
```

The image is pushed to `ghcr.io/ebejay95/tool/cmc-web:latest`.

---

## Configuration (Environment Variables / appsettings)

| Key | Description |
|---|---|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string |
| `Jwt__SecretKey` | JWT signing secret (≥ 32 characters) |
| `Jwt__Issuer` / `Jwt__Audience` | JWT validation parameters |
| `Jwt__ExpirationDays` | Token lifetime in days |
| `Redis__ConnectionString` | Redis (empty = no SignalR backplane, no Redis) |
| `Cors__AllowedOrigins` | CORS whitelist (Prod) |
| `EmailNotifications__Host` | SMTP host |
| `EmailNotifications__SimulateOnly` | `true` = no actual email sending |
| `Seq__ServerUrl` | Seq logging endpoint (optional) |
| `OpenTelemetry__OtlpEndpoint` | OTLP tracing endpoint (optional) |
| `MIGRATE_ON_STARTUP` | `true` / `false` (for local deployments only) |

---

## Known Weaknesses

### 🟠 DB Connection Pool Without Explicit Limit

Npgsql defaults to `MaxPoolSize=20` per `DbContext`. With 10 pods (HPA maximum) × 3 DbContexts = theoretically 600 connections against a Postgres instance that allows 100 by default. Recommendation: set `MaxPoolSize=5` in connection strings or put PgBouncer in front.

### 🟡 Dead-Letter Without Management UI

OutboxMessages that have reached `MaxRetries` (= 5) are logged but there is no dedicated admin endpoint (`GET /admin/outbox/dead-letters`) and no dashboard. Lost events are only visible via Seq search or direct DB query.

### 🟡 No Shared Deployment Cycle

As a Modular Monolith, all modules must be deployed together — a bugfix in `Notifications` requires a new image for the entire application. This is the fundamental trade-off compared to microservices and is a known, deliberate choice.

### 🟡 No Automated Tests

The core mechanics (Outbox SKIP LOCKED, Auto-Discovery, Event-Routing Todos → Notifications) are not covered by tests. Recommendation: integration tests with Testcontainers (PostgreSQL + Redis) for the critical paths.

### 🟡 Outbox Processors Not Capped

With N pods, N × 2 `BackgroundService` instances run (one per module). SKIP LOCKED prevents double processing, but without an explicit connection pool limit (see above) this accumulates at high scale.
