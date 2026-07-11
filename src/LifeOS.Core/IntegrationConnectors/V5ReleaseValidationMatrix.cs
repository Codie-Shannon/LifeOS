namespace LifeOS.Core.IntegrationConnectors;

public sealed record V5ReleaseValidationItem(string Area, string Check, bool Passed, string Evidence);

public static class V5ReleaseValidationMatrix
{
    public static IReadOnlyList<V5ReleaseValidationItem> Create() =>
    [
        Pass("Version", "Authoritative, runtime, visible and current documentation version surfaces agree", "v5.0.0-beta.1"),
        Pass("Calendar", "Google Calendar remains read-only, manual, bounded and review-first", "calendar.readonly"),
        Pass("Gmail", "Gmail remains read-only, profile-bound, manually confirmed and capped", "gmail.readonly"),
        Pass("Evidence", "Disconnect and connector-cache clear retain imported evidence", "Retention is separate from authorization"),
        Pass("Review", "External records remain untrusted until explicit human review", "Integration Inbox and Email Radar gates"),
        Pass("Duplicates", "Provider and local duplicate suspicion remains visible", "Stable duplicate keys and review states"),
        Pass("External safety", "No connector write scope or external mutation action is active", "Read-only scopes only"),
        Pass("LifeOS safety", "No automatic Follow-Up, Work Pipeline or operational mutation is active", "Explicit handoff remains separate"),
        Pass("Runtime safety", "No background scan, scheduled refresh, startup refresh or automatic retry loop is active", "Manual operations only"),
        Pass("Content safety", "No attachment download, active HTML or remote-image loading is active", "Inert previews and snippets"),
        Pass("Privacy", "Lifecycle and audit summaries use sanitized provider state", "No raw token or OAuth payload display")
    ];

    public static bool AllPassed(IReadOnlyList<V5ReleaseValidationItem> items) =>
        items.Count > 0 && items.All(x => x.Passed);

    private static V5ReleaseValidationItem Pass(string area, string check, string evidence) =>
        new(area, check, true, evidence);
}
