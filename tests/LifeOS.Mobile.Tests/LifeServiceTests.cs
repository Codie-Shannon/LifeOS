using LifeOS.Mobile.Core.Life;
using Xunit;

namespace LifeOS.Mobile.Tests;

public sealed class LifeServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 17, 10, 0, 0, TimeSpan.FromHours(12));

    [Fact]
    public void SnapshotContainsPrivateLifeAreas()
    {
        var snapshot = new LifeService().BuildSnapshot(Now);

        Assert.Contains(snapshot.Areas, x => x.Name == "Home");
        Assert.Contains(snapshot.Areas, x => x.Name == "Family");
        Assert.Contains(snapshot.Areas, x => x.Name == "Health");
        Assert.Contains(snapshot.Areas, x => x.Name == "Personal");
    }

    [Fact]
    public void RoutineCompletionIsIdempotent()
    {
        var service = new LifeService();

        var first = service.CompleteRoutine("routine", Now);
        var second = service.CompleteRoutine(
            "routine",
            Now.AddMinutes(5));

        Assert.Same(first, second);
    }

    [Fact]
    public void ReminderDeferralRequiresFutureTime()
    {
        var service = new LifeService();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => service.DeferReminder(
                "reminder",
                Now,
                Now));
    }

    [Fact]
    public void HealthAreaDoesNotContainDiagnosisClaim()
    {
        var snapshot = new LifeService().BuildSnapshot(Now);

        var health = snapshot.Areas.Single(x => x.Name == "Health");

        Assert.Contains(
            "no diagnosis",
            health.Summary,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "treatment decisions",
            health.Summary,
            StringComparison.OrdinalIgnoreCase);
    }
}
