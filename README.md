# Virentum Backend

Enterprise-grade ASP.NET Core (.NET 8) API powering the Virentum produce
inspection app. It accepts a produce photo, runs it through a computer-vision
model, applies **fruit-specific** commercial rules, persists the result, and
returns a clean DTO to the frontend.

> **API contract consumed by the frontend**
> - `POST /api/auth/login` → `{ token, user: { storeId, displayName, station } }`
> - `POST /api/inspection/scan` (multipart `Image`, `FruitType`; `Authorization: Bearer <jwt>`)
>   → `{ fruitType, ripenessPercent, commercialStatus, recommendation, scannedAt }`
> - `commercialStatus ∈ { ReadyForSale, ActionRequired, Expired }`

## How the architecture honours the guardrails

### 1. Open/Closed Principle — the Fruit Factory
No switch-statements or `if/else` chains on fruit type anywhere in the
controllers or services.

- `Domain/Processors/IFruitProcessor.cs` — strategy contract (one class per fruit).
- `Domain/Processors/BananaProcessor.cs`, `AvocadoProcessor.cs` — isolated logic.
- `Domain/Processors/FruitProcessorFactory.cs` — dictionary-backed resolver.
- `DependencyInjection/FruitProcessorRegistration.cs` — **auto-discovers** every
  `IFruitProcessor` by assembly scan.

**Adding a new fruit (e.g. Apple):** add `Apple` to `SupportedFruit`, create
`AppleProcessor : IFruitProcessor`. Nothing else changes — not the factory, not
the controller, not the DI wiring.

### 2. Domain isolation & DTOs
- `Contracts/Requests` & `Contracts/Responses` — immutable `record` DTOs.
- `Infrastructure/Persistence/Entities` — EF Core entities that **never** leave
  the persistence layer; mapping to DTOs happens in the services.
- Strongly-typed `enum`s (`SupportedFruit`, `CommercialStatus`) and pattern
  matching (`switch` expressions in the processors) keep the code self-documenting.

### 3. Professional error handling
- `Middleware/GlobalExceptionHandler.cs` — a single `IExceptionHandler` that
  converts every exception into an RFC 7807 **Problem Details** response.
- Expected failures derive from `Exceptions/DomainException` and carry their own
  status + title; unexpected ones become an opaque `500` with **no stack trace**
  leaked to the client.
- All failures are logged via `ILogger` with **structured** context (trace id,
  route, method).

### 4. Production-ready configuration & security
- `Options/*` bound with the **Options pattern** (`IOptions<T>`) and validated at
  startup (`ValidateOnStart`). Secrets (`Jwt:Secret`, Custom Vision keys, the
  database connection string) are **never** committed — supply them via
  environment variables or `dotnet user-secrets` (see `.env.example`).
- JWT bearer authentication; `[Authorize]` on the inspection endpoint.
- CORS policy explicitly allows the frontend origins **and** the `Authorization`
  header (`DependencyInjection/ServiceCollectionExtensions.AddVirentumCors`).

## Project layout

```
src/Virentum.Api
├── Program.cs                      # Composition root (declarative)
├── Controllers/                    # Thin HTTP edge (Auth, Inspection)
├── Contracts/                      # Request/Response DTOs (records)
├── Domain/
│   ├── Enums/                      # SupportedFruit, CommercialStatus
│   ├── Models/                     # VisionPrediction, RipenessAssessment
│   └── Processors/                 # IFruitProcessor + factory + per-fruit classes
├── Services/
│   ├── Inspection/                 # Orchestration
│   ├── Vision/                     # Azure Custom Vision client (+ stub)
│   ├── Auth/                       # Login flow
│   └── Security/                   # JWT + PBKDF2 password hashing
├── Infrastructure/Persistence/     # DbContext, entities, repositories, seeder
├── Middleware/                     # GlobalExceptionHandler
├── Options/                        # Strongly-typed settings
└── DependencyInjection/            # Composition helpers
```

## Running locally

Prerequisites: just the **.NET 8 SDK**. Local development uses an **in-memory
EF Core database**, so no database server or connection string is required to
run the API.

```bash
# From the repository root.

# Provide the dev secret out-of-band (or rely on appsettings.Development.json):
dotnet user-secrets init --project src/Virentum.Api
dotnet user-secrets set "Jwt:Secret" "a-long-random-dev-secret-min-32-characters" --project src/Virentum.Api

dotnet restore
dotnet run --project src/Virentum.Api
# Swagger UI: https://localhost:5001/swagger
```

In Development the API uses an in-memory store (`UseInMemoryDatabase`), runs with
`CustomVision:UseStub=true` (deterministic prediction from the image hash — no
Azure account needed), and seeds a demo operator: **storeId** `demo-store`,
**password** `changeit`. Data resets each time the process restarts.

## Database

| Environment | Provider | Configuration |
|---|---|---|
| Development | EF Core **In-Memory** | none — works out of the box |
| Production (Railway) | **PostgreSQL** (Npgsql) | `ConnectionStrings:Postgres` **or** `DATABASE_URL` |

The provider is selected by environment in `AddVirentumPersistence`. For
PostgreSQL the connection string is resolved in this order:

1. `ConnectionStrings:Postgres` (e.g. `ConnectionStrings__Postgres` env var), then
2. `DATABASE_URL` (Railway's `postgres://user:pass@host:port/db`), which is
   automatically converted to the Npgsql key/value format (TLS enabled).

So on Railway you can simply attach the PostgreSQL plugin — it injects
`DATABASE_URL` and the API picks it up with no extra config.

### Pointing the frontend at the backend
The frontend posts to `/api/inspection/scan`. Either run it behind a dev proxy
that forwards `/api` to `https://localhost:5001`, or set the appropriate base URL
and add the origin to `Cors:AllowedOrigins`.

### Docker

```bash
docker build -t virentum-backend .
docker run --rm -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__Postgres="Host=...;Port=5432;Database=virentum;Username=...;Password=..." \
  -e Jwt__Secret="a-long-random-secret-min-32-chars" \
  virentum-backend
```

The image runs as a non-root user and listens on port 8080.

## Production notes
- Replace `EnsureCreated` with EF Core **migrations**:
  `dotnet ef migrations add InitialCreate` then `dotnet ef database update`.
- Set `CustomVision:UseStub=false` and supply real Custom Vision credentials.
- Provide all secrets via environment variables / a managed secret store.
