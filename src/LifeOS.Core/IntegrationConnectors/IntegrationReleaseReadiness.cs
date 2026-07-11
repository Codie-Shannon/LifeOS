namespace LifeOS.Core.IntegrationConnectors;

public enum IntegrationLaneKind
{
    LocalImport,
    AuthenticatedCalendar,
    LocalCommunication,
    AuthenticatedEmail
}

public sealed record IntegrationLaneReadiness(
    string Key,
    string Name,
    IntegrationLaneKind Kind,
    bool IsAvailable,
    bool IsAuthenticated,
    bool IsReadOnly,
    bool IsManualOnly,
    bool RequiresHumanReview,
    bool RetainsEvidenceAfterDisconnect,
    string OperationLabel,
    string ScopeLabel,
    string CurrentState);

public sealed record IntegrationReleaseSummary(
    IReadOnlyList<IntegrationLaneReadiness> Lanes,
    int AvailableCount,
    int AuthenticatedCount,
    int ReadOnlyCount,
    int ManualOnlyCount,
    int ReviewRequiredCount,
    bool AnyAutomaticExternalMutation,
    bool AnyAutomaticLifeOsMutation,
    string GlobalSafetyStatement);

public static class IntegrationReleaseReadiness
{
    public const string GlobalSafetyStatement =
        "No connector changes an external system or LifeOS operational module automatically.";

    public static IntegrationReleaseSummary Create(
        string calendarState = "Local lifecycle state",
        string gmailState = "Local lifecycle state")
    {
        IntegrationLaneReadiness[] lanes =
        [
            new(
                "manual-csv-json",
                "Manual CSV / JSON intake",
                IntegrationLaneKind.LocalImport,
                true,
                false,
                true,
                true,
                true,
                true,
                "Explicit local preview import",
                "Local file only",
                "Available"),
            new(
                "local-ics",
                "Local ICS calendar intake",
                IntegrationLaneKind.LocalImport,
                true,
                false,
                true,
                true,
                true,
                true,
                "Explicit local calendar preview import",
                "Local file only",
                "Available"),
            new(
                "google-calendar",
                "Google Calendar read-only",
                IntegrationLaneKind.AuthenticatedCalendar,
                true,
                true,
                true,
                true,
                true,
                true,
                "Manual bounded refresh",
                "calendar.readonly",
                SanitizeState(calendarState)),
            new(
                "local-email-radar",
                "Local Email Radar import",
                IntegrationLaneKind.LocalCommunication,
                true,
                false,
                true,
                true,
                true,
                true,
                "Explicit JSON / CSV evidence import",
                "Local file only",
                "Available"),
            new(
                "gmail",
                "Gmail read-only",
                IntegrationLaneKind.AuthenticatedEmail,
                true,
                true,
                true,
                true,
                true,
                true,
                "Manual profile-bound bounded search",
                "gmail.readonly",
                SanitizeState(gmailState))
        ];

        return new(
            lanes,
            lanes.Count(x => x.IsAvailable),
            lanes.Count(x => x.IsAuthenticated),
            lanes.Count(x => x.IsReadOnly),
            lanes.Count(x => x.IsManualOnly),
            lanes.Count(x => x.RequiresHumanReview),
            false,
            false,
            GlobalSafetyStatement);
    }

    public static string SanitizeState(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "No operation recorded";
        }

        var singleLine = value.Replace("\r", " ").Replace("\n", " ").Trim();
        foreach (var marker in new[] { "access_token", "refresh_token", "client_secret", "authorization_code" })
        {
            if (singleLine.Contains(marker, StringComparison.OrdinalIgnoreCase))
            {
                return "Sanitized connector state available";
            }
        }

        return singleLine.Length <= 140 ? singleLine : singleLine[..140] + "...";
    }
}
