using LifeOS.Mobile.Core.Beta;
using Xunit;

namespace LifeOS.Mobile.Tests;

public sealed class BetaClosureServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 18, 12, 0, 0, TimeSpan.FromHours(12));

    [Fact]
    public void SnapshotDisablesProviderWrites()
    {
        var snapshot = new BetaClosureService().BuildSnapshot(Now);

        Assert.False(snapshot.Sync.ProviderWritesEnabled);
    }

    [Fact]
    public void OfflineQueueIncrementsPendingWork()
    {
        var service = new BetaClosureService();
        var snapshot = service.BuildSnapshot(Now);

        var updated = service.QueueOffline(snapshot.Sync);

        Assert.Equal(SyncHealth.Offline, updated.Health);
        Assert.Equal(snapshot.Sync.PendingQueue + 1, updated.PendingQueue);
        Assert.False(updated.ProviderWritesEnabled);
    }

    [Fact]
    public void StopSyncIsExplicitAndSafe()
    {
        var service = new BetaClosureService();
        var snapshot = service.BuildSnapshot(Now);

        var stopped = service.StopSync(snapshot.Sync);

        Assert.Equal(SyncHealth.Stopped, stopped.Health);
        Assert.False(stopped.ProviderWritesEnabled);
    }

    [Fact]
    public void RestoreCheckpointIsIdempotent()
    {
        var service = new BetaClosureService();
        var checkpoint = service.BuildSnapshot(Now).Recovery[0];

        var first = service.RestoreCheckpoint(checkpoint, Now);
        var second = service.RestoreCheckpoint(
            checkpoint,
            Now.AddMinutes(5));

        Assert.Same(first, second);
        Assert.Equal(RecoveryState.Restored, first.State);
    }

    [Fact]
    public void AccessibilityAuditMeetsMinimumTargets()
    {
        var audit = new BetaClosureService()
            .BuildSnapshot(Now)
            .Accessibility;

        Assert.True(audit.MinimumTouchTargetDp >= 48);
        Assert.True(audit.ScreenReaderLabels);
        Assert.True(audit.LargeTextSupported);
        Assert.True(audit.SensitivePreviewsHidden);
        Assert.True(audit.AppLockRequired);
    }

    [Fact]
    public void PerformanceIsWithinBetaBudget()
    {
        var service = new BetaClosureService();
        var performance = service.BuildSnapshot(Now).Performance;

        Assert.True(service.IsPerformanceWithinBetaBudget(performance));
    }

    [Fact]
    public void ReleaseChecklistContainsAllCoreWorkspaces()
    {
        var checks = new BetaClosureService()
            .BuildSnapshot(Now)
            .ReleaseChecks;

        Assert.Contains(checks, x => x.StartsWith("Home"));
        Assert.Contains(checks, x => x.StartsWith("Work"));
        Assert.Contains(checks, x => x.StartsWith("Money"));
        Assert.Contains(checks, x => x.StartsWith("Life"));
        Assert.Contains(checks, x => x.StartsWith("Projects"));
    }

    [Fact]
    public void RecoveryNeverClaimsDestructiveOverwrite()
    {
        var snapshot = new BetaClosureService().BuildSnapshot(Now);

        Assert.DoesNotContain(
            snapshot.Recovery,
            x => x.Label.Contains(
                "destructive",
                StringComparison.OrdinalIgnoreCase));
    }
}
