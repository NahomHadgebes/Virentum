using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Virentum.Api.Infrastructure.Persistence;
using Virentum.Api.Infrastructure.Persistence.Repositories;
using Virentum.Api.Options;
using Virentum.Api.Services.Auth;
using Virentum.Api.Services.Catalog;
using Virentum.Api.Services.Inspection;
using Virentum.Api.Services.Security;
using Virentum.Api.Services.Vision;

namespace Virentum.Api.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public const string CorsPolicyName = "VirentumFrontend";

    public static IServiceCollection AddVirentumOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<CustomVisionOptions>()
            .Bind(configuration.GetSection(CustomVisionOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<CorsOptions>()
            .Bind(configuration.GetSection(CorsOptions.SectionName));

        // Not validated: every field is optional by design, and an unset
        // section is the normal state for an instance with no demo account.
        services.AddOptions<DemoAccountOptions>()
            .Bind(configuration.GetSection(DemoAccountOptions.SectionName));

        return services;
    }

    /// <summary>
    /// PostgreSQL in every environment, including development.
    ///
    /// This used to fall back to an in-memory store locally, which needed no
    /// setup but silently diverged from production: no schema, no enum-to-string
    /// conversions, no indexes, and every inspection lost on restart. Since the
    /// app now shows scan history, that divergence would be visible as a feature
    /// that looks broken. A local database is the smaller cost.
    /// </summary>
    public static IServiceCollection AddVirentumPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = ResolvePostgresConnectionString(configuration)
            ?? throw new InvalidOperationException(
                "PostgreSQL connection string is not configured. Set " +
                "ConnectionStrings__Postgres or provide DATABASE_URL.");

        services.AddDbContext<VirentumDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IInspectionRepository, InspectionRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        return services;
    }

    /// <summary>
    /// Resolves the PostgreSQL connection string, preferring an explicit
    /// <c>ConnectionStrings:Postgres</c> and falling back to Railway's
    /// <c>DATABASE_URL</c> (a single <c>postgres://</c> URL), which is converted
    /// into the Npgsql key/value format.
    /// </summary>
    private static string? ResolvePostgresConnectionString(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        var databaseUrl = configuration["DATABASE_URL"]
            ?? Environment.GetEnvironmentVariable("DATABASE_URL");

        return string.IsNullOrWhiteSpace(databaseUrl)
            ? null
            : ConvertDatabaseUrl(databaseUrl);
    }

    private static string ConvertDatabaseUrl(string databaseUrl)
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':', 2);

        var builder = new Npgsql.NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Username = Uri.UnescapeDataString(userInfo[0]),
            Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty,
            Database = uri.AbsolutePath.TrimStart('/'),
            // Railway requires TLS to its managed Postgres.
            SslMode = Npgsql.SslMode.Require
        };

        return builder.ConnectionString;
    }

    public static IServiceCollection AddVirentumDomainServices(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);

        services.AddFruitProcessors();

        services.AddScoped<IInspectionService, InspectionService>();
        // Built once from the registered processors; the catalogue never changes
        // at runtime.
        services.AddSingleton<IFruitCatalogService, FruitCatalogService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();

        // Typed HttpClient for the vision provider, configured from options.
        services.AddScoped<IVisionService, ColorHeuristicVisionService>();

        return services;
    }

    public static IServiceCollection AddVirentumAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("Missing 'Jwt' configuration section.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Preserve the original "sub" claim instead of remapping it.
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
            });

        services.AddAuthorization();
        return services;
    }

    public static IServiceCollection AddVirentumCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var corsOptions = configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>()
            ?? new CorsOptions();

        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName, policy =>
            {
                policy
                    .WithOrigins(corsOptions.AllowedOrigins)
                    .AllowAnyMethod()
                    // Explicitly permit the Authorization header so the SPA can
                    // attach its Bearer token, and allow credentials to flow.
                    .WithHeaders("Authorization", "Content-Type")
                    .AllowCredentials();
            });
        });

        return services;
    }
}
