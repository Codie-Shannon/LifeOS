using System.Globalization;
using System.Text;
using LifeOS.Core.IntegrationInbox;

namespace LifeOS.Core.IntegrationConnectors;

public static class IcsCalendarImportConnector
{
    public static ManualIntegrationImportResult Import(string content, string sourceEvidence)
    {
        var connector = IntegrationConnectorRegistry.GetRequired("ics-import");
        var events = ParseEvents(content);
        var previews = new List<IntegrationPreviewItem>();
        var errors = new List<ManualIntegrationImportError>();
        var evidence = string.IsNullOrWhiteSpace(sourceEvidence) ? connector.DisplayName : sourceEvidence.Trim();

        for (var index = 0; index < events.Count; index++)
        {
            var eventNumber = index + 1;
            var calendarEvent = events[index];

            try
            {
                var title = Value(calendarEvent, "SUMMARY");
                if (string.IsNullOrWhiteSpace(title))
                {
                    throw new ArgumentException("Calendar event summary is required.");
                }

                var uid = Value(calendarEvent, "UID");
                if (string.IsNullOrWhiteSpace(uid))
                {
                    uid = $"event-{eventNumber}";
                }

                var startsAt = ParseIcsDate(Value(calendarEvent, "DTSTART"));
                var endsAt = ParseIcsDate(Value(calendarEvent, "DTEND"));
                var location = Value(calendarEvent, "LOCATION");
                var description = Value(calendarEvent, "DESCRIPTION");

                var summary = BuildSummary(description, startsAt, endsAt, location);

                var draft = new IntegrationPreviewDraft
                {
                    SourceKind = connector.SourceKind,
                    SourceLabel = connector.Key,
                    ExternalReference = uid,
                    Title = title,
                    Summary = summary,
                    OccurredAt = startsAt,
                    SuggestedTarget = IntegrationTargetKind.ItemState,
                    SuggestedAction = "Review calendar commitment and decide whether it needs prep, protection, or follow-up.",
                    SourceEvidence = $"{evidence}#VEVENT-{eventNumber}",
                    DuplicateKey = BuildDuplicateKey(connector.Key, uid, startsAt)
                };

                previews.Add(IntegrationPreviewIntake.CreatePreview(draft));
            }
            catch (Exception ex) when (ex is ArgumentException or FormatException)
            {
                errors.Add(new ManualIntegrationImportError(eventNumber, ex.Message));
            }
        }

        return new ManualIntegrationImportResult(connector.Key, previews, errors);
    }

    private static IReadOnlyList<Dictionary<string, string>> ParseEvents(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return [];
        }

        var unfolded = UnfoldLines(content);
        var events = new List<Dictionary<string, string>>();
        Dictionary<string, string>? currentEvent = null;

        foreach (var line in unfolded)
        {
            if (line.Equals("BEGIN:VEVENT", StringComparison.OrdinalIgnoreCase))
            {
                currentEvent = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                continue;
            }

            if (line.Equals("END:VEVENT", StringComparison.OrdinalIgnoreCase))
            {
                if (currentEvent is not null)
                {
                    events.Add(currentEvent);
                    currentEvent = null;
                }

                continue;
            }

            if (currentEvent is null)
            {
                continue;
            }

            var separator = line.IndexOf(':');
            if (separator <= 0)
            {
                continue;
            }

            var key = line[..separator];
            var parameterIndex = key.IndexOf(';');
            if (parameterIndex >= 0)
            {
                key = key[..parameterIndex];
            }

            currentEvent[key] = Unescape(line[(separator + 1)..]);
        }

        return events;
    }

    private static List<string> UnfoldLines(string content)
    {
        var lines = content.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        var unfolded = new List<string>();

        foreach (var line in lines)
        {
            if ((line.StartsWith(' ') || line.StartsWith('\t')) && unfolded.Count > 0)
            {
                unfolded[^1] += line[1..];
            }
            else if (!string.IsNullOrWhiteSpace(line))
            {
                unfolded.Add(line.TrimEnd());
            }
        }

        return unfolded;
    }

    private static string Value(IReadOnlyDictionary<string, string> calendarEvent, string key)
    {
        return calendarEvent.TryGetValue(key, out var value) ? value.Trim() : string.Empty;
    }

    private static DateTime? ParseIcsDate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        var formats = new[] { "yyyyMMdd'T'HHmmss'Z'", "yyyyMMdd'T'HHmmss", "yyyyMMdd" };
        var styles = normalized.EndsWith('Z')
            ? DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal
            : DateTimeStyles.AssumeLocal;

        return DateTime.TryParseExact(normalized, formats, CultureInfo.InvariantCulture, styles, out var parsed)
            ? parsed
            : throw new FormatException($"Unsupported ICS date value '{value}'.");
    }

    private static string BuildSummary(string description, DateTime? startsAt, DateTime? endsAt, string location)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(description))
        {
            parts.Add(description.Trim());
        }

        if (startsAt.HasValue)
        {
            parts.Add($"Starts: {startsAt.Value:g}");
        }

        if (endsAt.HasValue)
        {
            parts.Add($"Ends: {endsAt.Value:g}");
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            parts.Add($"Location: {location.Trim()}");
        }

        return string.Join(" | ", parts);
    }

    private static string BuildDuplicateKey(string connectorKey, string externalReference, DateTime? startsAt)
    {
        var datePart = startsAt?.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture) ?? "nodate";

        return string.Join(
            ":",
            connectorKey.Trim().ToLowerInvariant(),
            externalReference.Trim().ToLowerInvariant(),
            datePart,
            "noamount");
    }

    private static string Unescape(string value)
    {
        return value
            .Replace("\\n", " ", StringComparison.OrdinalIgnoreCase)
            .Replace("\\,", ",")
            .Replace("\\;", ";")
            .Replace("\\\\", "\\");
    }
}
