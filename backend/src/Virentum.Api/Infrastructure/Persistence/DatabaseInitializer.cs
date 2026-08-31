using Microsoft.EntityFrameworkCore;
using Virentum.Api.Infrastructure.Persistence.Entities;
using Virentum.Api.Services.Security;

namespace Virentum.Api.Infrastructure.Persistence;

/// <summary>
/// Applies the schema at startup and, in Development only, seeds a single demo
/// operator so the frontend can sign in immediately. No credentials are seeded
/// outside Development.
/// </summary>
public static class DatabaseInitializer
{
    public static async Task InitialiseAsync(WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<VirentumDbContext>();

        // Apply any pending EF Core migrations. This replaces EnsureCreated,
        // which creates the schema once and then has no way to evolve it — the
        // first column added to an entity would silently fail against a database
        // that already exists.
        await db.Database.MigrateAsync();

        if (!app.Environment.IsDevelopment())
        {
            return;
        }

        if (await db.Users.AnyAsync())
        {
            return;
        }

        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        db.Users.Add(new UserAccount
        {
            Id = Guid.NewGuid(),
            StoreId = "demo-store",
            PasswordHash = hasher.Hash("changeit"),
            DisplayName = "Store Associate",
            Station = "Station #4",
            Role = "associate",
            CreatedAt = DateTimeOffset.UtcNow,
        });

        await db.SaveChangesAsync();

        app.Logger.LogInformation(
            "Seeded development operator 'demo-store' (password 'changeit'). Do not use in production.");
    }
}
