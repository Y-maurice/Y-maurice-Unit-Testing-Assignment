using Microsoft.Extensions.Time.Testing;

namespace DriveNow.UnitTests.Support;


public static class TestClock
{
    public static readonly DateTimeOffset Now = new(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);

    /// <summary>The same meaning as DateTime.Today in the production code: the local date at midnight.</summary>
    public static DateTime Today => Now.Date;

    public static FakeTimeProvider Create() => new(Now);
}