using System.Text.Json.Serialization;
using Virentum.Api.DependencyInjection;
using Virentum.Api.Infrastructure.Persistence;
using Virentum.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ── Configuration & Options (strongly-typed, validated at startup) ───────────
builder.Services.AddVirentumOptions(builder.Configuration);

// ── Persistence (EF Core + repositories) ─────────────────────────────────────
builder.Services.AddVirentumPersistence(builder.Configuration);

// ── Domain services, fruit-processor factory, vision client ──────────────────
builder.Services.AddVirentumDomainServices();

// ── Security: JWT auth + CORS for the SPA ────────────────────────────────────
builder.Services.AddVirentumAuthentication(builder.Configuration);
builder.Services.AddVirentumCors(builder.Configuration);

// ── MVC + JSON (enums as strings to match the frontend contract) ─────────────
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// ── Centralised error handling (RFC 7807 Problem Details) ────────────────────
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// ── API surface documentation ────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// The exception handler must run first so it can catch everything downstream.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseCors(ServiceCollectionExtensions.CorsPolicyName);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await DatabaseInitializer.InitialiseAsync(app);

app.Run();
