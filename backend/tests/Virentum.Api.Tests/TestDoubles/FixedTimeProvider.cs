namespace Virentum.Api.Tests.TestDoubles;

/// <summary>
/// A clock frozen at a known instant, so ScannedAt is assertable. Overriding
/// TimeProvider directly avoids taking a dependency on a testing package for
/// one overridden method.
/// </summary>
internal sealed class FixedTimeProvider : TimeProvider
{
    private readonly DateTimeOffset _now;

    public FixedTimeProvider(DateTimeOffset now)
    {
        _now = now;
    }

    public override DateTimeOffset GetUtcNow() => _now;
}
