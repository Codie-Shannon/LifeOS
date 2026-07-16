namespace LifeOS.Mobile.Core.Foundation;

public enum MobileStartupState { Loading, Locked, SignedOut, Ready, Recovery }
public enum MobileSyncState { Idle, Syncing, Pending, Offline, Stale, Failed, Conflict, Stopped }
public enum MobileThemeMode { System, Light, Dark, HighContrast }
public enum MobileDensity { Comfortable, Compact }

public sealed record MobilePreferences(
    MobileThemeMode Theme,
    string Accent,
    MobileDensity Density,
    bool HideSensitivePreviews,
    bool RequireAppLock);

public sealed record MobileSessionState(bool IsSignedIn, bool IsLocked, string DeviceLabel);

public sealed record MobileSyncSnapshot(
    MobileSyncState State,
    int PendingCount,
    DateTimeOffset? LastSuccessfulSyncUtc,
    string FreshnessLabel,
    string? SafeError = null);

public sealed record MobileOutboxItem(
    string CommandId,
    string Kind,
    string PayloadHash,
    DateTimeOffset CreatedUtc,
    string State);

public sealed record MobileDiagnosticItem(string Name, string Value, bool IsSensitive = false);

public static class MobileWorkspaceCatalog
{
    public static readonly IReadOnlyList<string> Permanent =
        ["Home", "Work", "Career", "Money", "Life", "Projects", "Assistant", "Settings"];
}
