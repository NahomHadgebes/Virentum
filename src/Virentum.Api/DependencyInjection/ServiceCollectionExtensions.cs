using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Virentum.Api.Infrastructure.Persistence;
using Virentum.Api.Infrastructure.Persistence.Repositories;
using Virentum.Api.Options;
using Virentum.Api.Services.Auth;
using Virentum.Api.Services.Inspection;
using Virentum.Api.Services.Security;
using Virentum.Api.Services.Vision;

namespace Virentum.Api.DependencyInjection;

/// <summary>
/// Composition-root helpers that keep <c>Program.cs</c> declarative. Each method
/// owns one concern (options, persistence, domain services, auth, CORS).
/// </summary>
public static class ServiceCollectionExtensions
{
    public const string CorsPolicyName = "VirentumFrontend";

    /// <summary>
    /// Binds and validates every configuration section using the Options pattern.
    /// Validation runs at startup (<c>ValidateOnStart</c>) so misconfiguration
    /// fails fast instead of at the first request.
    /// </summary>
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

        return services;
    }

    public static IServiceCollection AddVirentumPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException(
                "Connection string 'Postgres' is not configured. Set ConnectionStrings__Postgres.");

        services.AddDbContext<VirentumDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IInspectionRepository, InspectionRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        return services;
    }

    public static IServiceCollection AddVirentumDomainServices(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);

        services.AddFruitProcessors();

        services.AddScoped<IInspectionService, InspectionService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();

        // Typed HttpClient for the vision provider, configured from options.
        services.AddHttpClient<IVisionService, AzureCustomVisionService>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<CustomVisionOptions>>().Value;
            if (!string.IsNullOrWhiteSpace(options.Endpoint))
            {
                client.BaseAddress = new Uri(options.Endpoint.TrimEnd('/') + "/");
            }
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

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
