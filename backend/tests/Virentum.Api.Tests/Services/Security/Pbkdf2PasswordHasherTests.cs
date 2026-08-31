using Virentum.Api.Services.Security;
using Xunit;

namespace Virentum.Api.Tests.Services.Security;

public sealed class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _hasher = new();

    [Fact]
    public void Verifies_the_password_it_hashed()
    {
        Assert.True(_hasher.Verify("changeit", _hasher.Hash("changeit")));
    }

    [Fact]
    public void Rejects_a_different_password()
    {
        Assert.False(_hasher.Verify("changeit ", _hasher.Hash("changeit")));
        Assert.False(_hasher.Verify("CHANGEIT", _hasher.Hash("changeit")));
        Assert.False(_hasher.Verify(string.Empty, _hasher.Hash("changeit")));
    }

    /// <summary>
    /// A per-password salt is what stops one rainbow table from covering every
    /// account that chose the same password.
    /// </summary>
    [Fact]
    public void Produces_a_different_hash_each_time_for_the_same_password()
    {
        Assert.NotEqual(_hasher.Hash("changeit"), _hasher.Hash("changeit"));
    }

    [Fact]
    public void Stores_iterations_salt_and_key_as_three_parts()
    {
        var parts = _hasher.Hash("changeit").Split('.');

        Assert.Equal(3, parts.Length);
        Assert.Equal(100_000, int.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture));
        Assert.Equal(16, Convert.FromBase64String(parts[1]).Length);
        Assert.Equal(32, Convert.FromBase64String(parts[2]).Length);
    }

    /// <summary>
    /// A stored value from an older or corrupted format must fail closed, not
    /// throw its way into a 500.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("not-a-hash")]
    [InlineData("100000.onlytwo")]
    [InlineData("notanumber.c2FsdA==.a2V5")]
    [InlineData("100000.!!!not-base64!!!.a2V5")]
    public void Returns_false_for_a_malformed_stored_hash(string stored)
    {
        Assert.False(_hasher.Verify("changeit", stored));
    }

    [Fact]
    public void Reads_the_iteration_count_back_from_the_stored_value()
    {
        var hash = _hasher.Hash("changeit");
        var parts = hash.Split('.', 3);
        var tampered = $"1.{parts[1]}.{parts[2]}";

        // Same salt and key, fewer iterations: the derived key no longer matches.
        Assert.False(_hasher.Verify("changeit", tampered));
    }
}
