using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using Virentum.Api.DependencyInjection;
using Virentum.Api.Infrastructure.Persistence;
using Virentum.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ── Configuration & Options (strongly-typed, validated at startup) ───────────
builder.Services.AddVirentumOptions(builder.Configuration);

// ── Persistence (in-memory in Development, PostgreSQL otherwise) ──────────────
builder.Services.AddVirentumPersistence(builder.Configuration, builder.Environment);

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
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Virentum API", Version = "v1" });

    // JWT bearer support so the Swagger "Authorize" button can attach a token
    // to protected endpoints (e.g. POST /api/inspection/scan).
    var bearerScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Paste the JWT from /api/auth/login (just the token, no 'Bearer ' prefix).",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer",
        },
    };

    options.AddSecurityDefinition("Bearer", bearerScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [bearerScheme] = Array.Empty<string>(),
    });
});

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
