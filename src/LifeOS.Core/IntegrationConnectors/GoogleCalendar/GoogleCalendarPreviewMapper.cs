using System.Globalization;
using LifeOS.Core.IntegrationInbox;

namespace LifeOS.Core.IntegrationConnectors.GoogleCalendar;

public static class GoogleCalendarPreviewMapper
{
    public static IntegrationPreviewItem Map(ExternalCalendarEvent calendarEvent)
    {
        ArgumentNullException.ThrowIfNull(calendarEvent);

        if (string.IsNullOrWhiteSpace(calendarEvent.ExternalId))
        {
            throw new ArgumentException("External calendar event ID is required.", nameof(calendarEvent));
        }

        if (string.IsNullOrWhiteSpace(calendarEvent.Title))
        {
            throw new ArgumentException("Calendar event title is required.", nameof(calendarEvent));
        }

        var connector = IntegrationConnectorRegistry.GetRequired("google-calendar");
        var startsAt = calendarEvent.StartsAt?.LocalDateTime;
        var endsAt = calendarEvent.EndsAt?.LocalDateTime;
        var calendarIdentity = string.IsNullOrWhiteSpace(calendarEvent.CalendarLabel)
            ? "Selected calendar"
            : calendarEvent.CalendarLabel.Trim();

        var summaryParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(calendarEvent.Description)) summaryParts.Add(calendarEvent.Description.Trim());
        if (startsAt.HasValue) summaryParts.Add($"Starts: {startsAt.Value:g}");
        if (endsAt.HasValue) summaryParts.Add($"Ends: {endsAt.Value:g}");
        if (!string.IsNullOrWhiteSpace(calendarEvent.Location)) summaryParts.Add($"Location: {calendarEvent.Location.Trim()}");
        summaryParts.Add($"Calendar: {calendarIdentity}");

        var draft = new IntegrationPreviewDraft
        {
            SourceKind = connector.SourceKind,
            SourceLabel = connector.Key,
            ConnectorKey = connector.Key,
            ProviderAccountLabel = calendarEvent.ProviderAccountLabel.Trim(),
            ProviderContainerId = calendarEvent.CalendarId.Trim(),
            ExternalReference = calendarEvent.ExternalId.Trim(),
            Title = calendarEvent.Title.Trim(),
            Summary = string.Join(" | ", summaryParts),
            OccurredAt = startsAt,
            EndsAt = endsAt,
            FetchedAt = calendarEvent.FetchedAt.LocalDateTime,
            SuggestedTarget = IntegrationTargetKind.ItemState,
            SuggestedAction = "Review calendar commitment and decide whether it needs prep, protection, or follow-up.",
            SourceEvidence = string.IsNullOrWhiteSpace(calendarEvent.HtmlLink)
                ? $"google-calendar:{calendarEvent.CalendarId}:{calendarEvent.ExternalId}"
                : calendarEvent.HtmlLink.Trim(),
            DuplicateKey = BuildDuplicateKey(connector.Key, calendarEvent.CalendarId, calendarEvent.ExternalId, calendarEvent.StartsAt)
        };

        return IntegrationPreviewIntake.CreatePreview(draft);
    }

    public static string BuildDuplicateKey(string connectorKey, string calendarId, string externalId, DateTimeOffset? startsAt)
    {
        var datePart = startsAt?.UtcDateTime.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture) ?? "nodate";
        return string.Join(":",
            connectorKey.Trim().ToLowerInvariant(),
            calendarId.Trim().ToLowerInvariant(),
            externalId.Trim().ToLowerInvariant(),
            datePart,
            "noamount");
    }
}
