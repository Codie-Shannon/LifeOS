namespace LifeOS.Shared.IntegrationConnectors.GoogleCalendar;

public enum GoogleCalendarLifecycleState
{
    ConfigurationMissing,
    ConfiguredDisconnected,
    ConnectedLocally,
    RefreshInProgress,
    RefreshSucceeded,
    RefreshEmpty,
    RefreshPartial,
    RefreshFailed,
    AuthenticationExpired,
    AccessRevoked,
    TokenCacheMissing,
    Disconnected,
    LocalCacheCleared
}

public sealed class GoogleCalendarLifecycleSnapshot
{
    public GoogleCalendarLifecycleState State { get; set; } = GoogleCalendarLifecycleState.ConfigurationMissing;
    public DateTimeOffset? LastConnectionAttemptAt { get; set; }
    public DateTimeOffset? LastConnectedAt { get; set; }
    public DateTimeOffset? LastRefreshAttemptAt { get; set; }
    public DateTimeOffset? LastSuccessfulRefreshAt { get; set; }
    public DateTimeOffset? LastDisconnectAt { get; set; }
    public DateTimeOffset? LastCacheClearAt { get; set; }
    public DateTimeOffset? LastRangeFrom { get; set; }
    public DateTimeOffset? LastRangeTo { get; set; }
    public int LastReceivedCount { get; set; }
    public int LastDuplicateCount { get; set; }
    public int LastSkippedCount { get; set; }
    public string LastResultSummary { get; set; } = "No connector action has been recorded yet.";
    public string LastSanitizedError { get; set; } = string.Empty;
}
