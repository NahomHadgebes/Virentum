# Virentum Backend

Enterprise-grade ASP.NET Core (.NET 8) API powering the Virentum produce
inspection app. It accepts a produce photo, runs it through a computer-vision
model, applies **fruit-specific** commercial rules, persists the result, and
returns a clean DTO to the frontend.

> **API contract consumed by the frontend**
> - `POST /api/auth/login` → `{ token, user: { storeId, displayName, station } }`
> - `GET /api/fruits` → every fruit this build can inspect, with its full band table
> - `POST /api/inspection/scan` (multipart `Images` ×1–3, `FruitType`, `Audience`;
>   `Authorization: Bearer <jwt>`) → the verdict, its factors and its evidence
> - `GET /api/inspection/history`, `GET /api/inspection/summary`
> - `commercialStatus ∈ { Underripe, ReadyForSale, ActionRequired, Expired }`
> - `edibility ∈ { NotReadyYet, Good, EatSoon, DoNotEat }` — a separate scale on
>   purpose: a banana a shop must pull is often still fine for baking.

## How the architecture honours the guardrails

### 1. Open/Closed Principle — the Fruit Factory
No switch-statements or `if/else` chains on fruit type anywhere in the
controllers or services.

- `Domain/Processors/IFruitProcessor.cs` — strategy contract (one class per fruit).
- `Domain/Processors/BananaProcessor.cs`, `AvocadoProcessor.cs`,
  `PearProcessor.cs`, `MangoProcessor.cs` — isolated logic, one class per fruit.
- `Domain/Processors/FruitProcessorFactory.cs` — dictionary-backed resolver.
- `DependencyInjection/FruitProcessorRegistration.cs` — **auto-discovers** every
  `IFruitProcessor` by assembly scan.

**Adding a new fruit (e.g. Apple):** add `Apple` to `SupportedFruit`, create
`AppleProcessor : FruitProcessor` with its bands, its colour profile and what
each colour means for it. Nothing else changes — not the factory, not the
controller, not the DI wiring, and not the frontend's fruit list, which is built
from `GET /api/fruits`. Pear and mango were added exactly this way.

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
│   ├── Enums/                      # SupportedFruit, CommercialStatus, Audience, EdibilityVerdict
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

Prerequisites: the **.NET 8 SDK** and a local **PostgreSQL**. Development
deliberately runs against a real database rather than the in-memory provider, so
the enum-to-string conversions and the history queries behave exactly as they do
in production.

```bash
# From backend/.

createdb virentum_dev     # credentials in appsettings.Development.json

# The schema is applied by migrations at startup; generate one the first time:
dotnet ef migrations add InitialCreate --project src/Virentum.Api

# Optional: keep the dev secret out of the file rather than using the checked-in one.
dotnet user-secrets set "Jwt:Secret" "a-long-random-dev-secret-min-32-characters" --project src/Virentum.Api

dotnet restore
dotnet run --project src/Virentum.Api
# http://localhost:5000 · Swagger UI at /swagger
```

In Development the API runs with `CustomVision:UseStub=true` (the colour
heuristic, no Azure account needed) and seeds one operator on first run:
**storeId** `demo-store`, **password** `changeit`. Nothing is seeded outside
Development.

## Database

| Environment | Provider | Configuration |
|---|---|---|
| Development | **PostgreSQL** (Npgsql) | `appsettings.Development.json`, or user-secrets |
| Production (Railway) | **PostgreSQL** (Npgsql) | `ConnectionStrings:Postgres` **or** `DATABASE_URL` |

Both environments use Npgsql, and `DatabaseInitializer` applies any pending
migrations at startup. The connection string is resolved in this order:

1. `ConnectionStrings:Postgres` (e.g. `ConnectionStrings__Postgres` env var), then
2. `DATABASE_URL` (Railway's `postgres://user:pass@host:port/db`), which is
   automatically converted to the Npgsql key/value format (TLS enabled).

So on Railway you can simply attach the PostgreSQL plugin — it injects
`DATABASE_URL` and the API picks it up with no extra config.

### Pointing the frontend at the backend
Set `VITE_API_BASE_URL` in `frontend/.env` (the dev API listens on
`http://localhost:5000`) and make sure that origin is in `Cors:AllowedOrigins`.
`http://localhost:5173` is already allowed in Development.

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
- Migrations are applied automatically at startup by `DatabaseInitializer`;
  commit every migration you generate.
- Set `CustomVision:UseStub=false` and supply real Custom Vision credentials.
- Provide all secrets via environment variables / a managed secret store.
