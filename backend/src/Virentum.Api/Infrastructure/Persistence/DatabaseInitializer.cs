using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Virentum.Api.Infrastructure.Persistence.Entities;
using Virentum.Api.Options;
using Virentum.Api.Services.Security;

namespace Virentum.Api.Infrastructure.Persistence;

/// <summary>
/// Applies the schema at startup and seeds at most one account.
///
/// Development seeds a fixed operator so the frontend can sign in immediately.
/// Every other environment seeds nothing unless <c>DemoAccount</c> is
/// configured, which is what lets a public demo be signed into without an API
/// that hands out a login on its own.
/// </summary>
public static class DatabaseInitializer
{
    private const string DevelopmentStoreId = "demo-store";
    private const string DevelopmentPassword = "changeit";

    public static async Task InitialiseAsync(WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<VirentumDbContext>();

        // Apply any pending EF Core migrations. This replaces EnsureCreated,
        // which creates the schema once and then has no way to evolve it — the
        // first column added to an entity would silently fail against a database
        // that already exists.
        await db.Database.MigrateAsync();

        var seed = ResolveSeedAccount(app, scope.ServiceProvider);
        if (seed is null)
        {
            return;
        }

        // Keyed on the store id rather than "are there any users at all", so a
        // database that already has real operators is neither skipped nor
        // handed a duplicate.
        if (await db.Users.AnyAsync(user => user.StoreId == seed.StoreId))
        {
            return;
        }

        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        db.Users.Add(new UserAccount
        {
            Id = Guid.NewGuid(),
            StoreId = seed.StoreId,
            PasswordHash = hasher.Hash(seed.Password),
            DisplayName = seed.DisplayName,
            Station = seed.Station,
            Role = "associate",
            CreatedAt = DateTimeOffset.UtcNow,
        });

        await db.SaveChangesAsync();

        // The identifier is logged so an operator can find the account; the
        // password never is, in either environment.
        app.Logger.LogInformation(
            "Seeded the {Environment} account '{StoreId}'.",
            app.Environment.EnvironmentName,
            seed.StoreId);
    }

    /// <summary>
    /// The account this environment should have, or null for none. Development
    /// gets its fixed operator; anything else gets one only when both halves of
    /// DemoAccount were supplied deliberately.
    /// </summary>
    private static SeedAccount? ResolveSeedAccount(WebApplication app, IServiceProvider services)
    {
        var configured = services.GetRequiredService<IOptions<DemoAccountOptions>>().Value;

        if (configured.IsConfigured)
        {
            return new SeedAccount(
                configured.StoreId!,
                configured.Password!,
                configured.DisplayName,
                configured.Station);
        }

        if (app.Environment.IsDevelopment())
        {
            return new SeedAccount(
                DevelopmentStoreId,
                DevelopmentPassword,
                "Store Associate",
                "Station #4");
        }

        app.Logger.LogInformation(
            "No DemoAccount is configured, so no account was seeded. Set DemoAccount__StoreId " +
            "and DemoAccount__Password to make this instance signable-in.");

        return null;
    }

    private sealed record SeedAccount(
        string StoreId,
        string Password,
        string DisplayName,
        string Station);
}
