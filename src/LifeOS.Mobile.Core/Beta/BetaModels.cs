namespace LifeOS.Mobile.Core.Beta;

public enum SyncHealth
{
    Healthy,
    Degraded,
    Offline,
    Stopped
}

public enum RecoveryState
{
    Ready,
    CheckpointAvailable,
    RecoveryRequired,
    Restored
}

public sealed record SyncStatus(
    SyncHealth Health,
    DateTimeOffset LastSuccessfulSyncUtc,
    int PendingQueue,
    bool ProviderWritesEnabled,
    string Freshness);

public sealed record RecoveryCheckpoint(
    string Id,
    string Label,
    DateTimeOffset CreatedUtc,
    RecoveryState State,
    string IntegrityHash,
    int PendingItems);

public sealed record AccessibilityAudit(
    int MinimumTouchTargetDp,
    bool ScreenReaderLabels,
    bool LargeTextSupported,
    bool SensitivePreviewsHidden,
    bool AppLockRequired);

public sealed record PerformanceSnapshot(
    int StartupMilliseconds,
    int HomeRenderMilliseconds,
    int WorkRenderMilliseconds,
    int MoneyRenderMilliseconds,
    int LifeRenderMilliseconds,
    int ProjectsRenderMilliseconds,
    long ManagedMemoryBytes);

public sealed record BetaClosureSnapshot(
    SyncStatus Sync,
    IReadOnlyList<RecoveryCheckpoint> Recovery,
    AccessibilityAudit Accessibility,
    PerformanceSnapshot Performance,
    IReadOnlyList<string> ReleaseChecks);
