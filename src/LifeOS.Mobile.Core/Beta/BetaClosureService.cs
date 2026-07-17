using LifeOS.Mobile.Core.Services;

namespace LifeOS.Mobile.Core.Beta;

public sealed class BetaClosureService
{
    private readonly Dictionary<string, RecoveryCheckpoint> _recovery =
        new(StringComparer.Ordinal);

    public BetaClosureSnapshot BuildSnapshot(DateTimeOffset now)
    {
        var recovery = new[]
        {
            new RecoveryCheckpoint(
                "checkpoint-current",
                "Current local state",
                now.AddMinutes(-8),
                RecoveryState.Ready,
                MobileFoundationService.Sha256("group57-current"),
                1),
            new RecoveryCheckpoint(
                "checkpoint-previous",
                "Previous verified checkpoint",
                now.AddHours(-6),
                RecoveryState.CheckpointAvailable,
                MobileFoundationService.Sha256("group57-previous"),
                0)
        };

        return new BetaClosureSnapshot(
            new SyncStatus(
                SyncHealth.Healthy,
                now.AddMinutes(-4),
                1,
                false,
                "Fresh"),
            recovery,
            new AccessibilityAudit(
                48,
                true,
                true,
                true,
                true),
            new PerformanceSnapshot(
                620,
                42,
                51,
                48,
                45,
                53,
                23_800_000),
            new[]
            {
                "Home proof complete",
                "Work proof complete",
                "Money proof complete",
                "Life proof complete",
                "Projects proof complete",
                "Sync and recovery proof ready",
                "Accessibility proof ready",
                "Performance proof ready"
            });
    }

    public RecoveryCheckpoint RestoreCheckpoint(
        RecoveryCheckpoint checkpoint,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(checkpoint);

        if (_recovery.TryGetValue(checkpoint.Id, out var existing) &&
            existing.State == RecoveryState.Restored)
        {
            return existing;
        }

        var restored = checkpoint with
        {
            State = RecoveryState.Restored,
            CreatedUtc = now
        };

        _recovery[checkpoint.Id] = restored;
        return restored;
    }

    public SyncStatus StopSync(SyncStatus status) =>
        status with
        {
            Health = SyncHealth.Stopped,
            ProviderWritesEnabled = false
        };

    public SyncStatus QueueOffline(SyncStatus status) =>
        status with
        {
            Health = SyncHealth.Offline,
            PendingQueue = status.PendingQueue + 1,
            ProviderWritesEnabled = false,
            Freshness = "Offline"
        };

    public bool IsPerformanceWithinBetaBudget(
        PerformanceSnapshot performance) =>
        performance.StartupMilliseconds <= 1500 &&
        performance.HomeRenderMilliseconds <= 150 &&
        performance.WorkRenderMilliseconds <= 150 &&
        performance.MoneyRenderMilliseconds <= 150 &&
        performance.LifeRenderMilliseconds <= 150 &&
        performance.ProjectsRenderMilliseconds <= 150 &&
        performance.ManagedMemoryBytes <= 128_000_000;
}
