using Microsoft.Extensions.Logging.Abstractions;
using Virentum.Api.Contracts.Requests;
using Virentum.Api.Contracts.Responses;
using Virentum.Api.Exceptions;
using Virentum.Api.Infrastructure.Persistence.Entities;
using Virentum.Api.Services.Auth;
using Virentum.Api.Services.Security;
using Virentum.Api.Tests.TestDoubles;
using Xunit;

namespace Virentum.Api.Tests.Services.Auth;

public sealed class AuthServiceTests
{
    private const string Password = "changeit";

    private static readonly IPasswordHasher Hasher = new Pbkdf2PasswordHasher();

    private static UserAccount Operator() =>
        new()
        {
            Id = Guid.NewGuid(),
            StoreId = "demo-store",
            PasswordHash = Hasher.Hash(Password),
            DisplayName = "Store Associate",
            Station = "Station #4",
            Role = "associate",
            CreatedAt = DateTimeOffset.UtcNow,
        };

    private static AuthService CreateService(UserAccount? user, StubTokenService? tokens = null) =>
        new(
            new StubUserRepository(user),
            Hasher,
            tokens ?? new StubTokenService(),
            NullLogger<AuthService>.Instance);

    [Fact]
    public async Task Issues_a_token_for_correct_credentials()
    {
        var tokens = new StubTokenService();
        var user = Operator();

        var response = await CreateService(user, tokens).LoginAsync(
            new LoginRequest(user.StoreId, Password));

        Assert.Equal(StubTokenService.Token, response.Token);
        Assert.Equal(user.StoreId, tokens.IssuedFor?.StoreId);
    }

    [Fact]
    public async Task Returns_the_operator_profile_the_frontend_caches()
    {
        var user = Operator();

        var response = await CreateService(user).LoginAsync(new LoginRequest(user.StoreId, Password));

        Assert.Equal("demo-store", response.User.StoreId);
        Assert.Equal("Store Associate", response.User.DisplayName);
        Assert.Equal("Station #4", response.User.Station);
    }

    /// <summary>
    /// UserDto has no password field at all, so the hash cannot be serialised
    /// even by accident. This asserts the shape rather than the value.
    /// </summary>
    [Fact]
    public void UserDto_exposes_no_secret_fields()
    {
        var names = typeof(UserDto).GetProperties().Select(property => property.Name).ToArray();

        Assert.Equal(3, names.Length);
        Assert.Contains("StoreId", names);
        Assert.Contains("DisplayName", names);
        Assert.Contains("Station", names);
        Assert.DoesNotContain(
            names,
            name => name.Contains("Password", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Rejects_a_wrong_password()
    {
        var user = Operator();

        await Assert.ThrowsAsync<AuthenticationFailedException>(
            () => CreateService(user).LoginAsync(new LoginRequest(user.StoreId, "wrong-password")));
    }

    [Fact]
    public async Task Rejects_an_unknown_store()
    {
        await Assert.ThrowsAsync<AuthenticationFailedException>(
            () => CreateService(user: null).LoginAsync(new LoginRequest("no-such-store", Password)));
    }

    /// <summary>
    /// The security property worth protecting: an attacker must not be able to
    /// tell a valid store id with a bad password from a store id that does not
    /// exist. Both paths must be indistinguishable to the caller.
    /// </summary>
    [Fact]
    public async Task Reports_a_wrong_password_and_an_unknown_store_identically()
    {
        var user = Operator();

        var wrongPassword = await Assert.ThrowsAsync<AuthenticationFailedException>(
            () => CreateService(user).LoginAsync(new LoginRequest(user.StoreId, "wrong-password")));

        var unknownStore = await Assert.ThrowsAsync<AuthenticationFailedException>(
            () => CreateService(user: null).LoginAsync(new LoginRequest("no-such-store", Password)));

        Assert.Equal(wrongPassword.Message, unknownStore.Message);
        Assert.Equal(wrongPassword.Title, unknownStore.Title);
        Assert.Equal(401, wrongPassword.StatusCode);
        Assert.Equal(401, unknownStore.StatusCode);
    }

    [Fact]
    public async Task Issues_no_token_when_authentication_fails()
    {
        var tokens = new StubTokenService();

        await Assert.ThrowsAsync<AuthenticationFailedException>(
            () => CreateService(Operator(), tokens).LoginAsync(
                new LoginRequest("demo-store", "wrong-password")));

        Assert.Null(tokens.IssuedFor);
    }
}
