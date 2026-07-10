using LifeOS.Core.IntegrationInbox;

namespace LifeOS.Core.IntegrationConnectors.GoogleCalendar;

public sealed record GoogleCalendarRefreshResult(
    CalendarRefreshRange Range,
    IReadOnlyList<IntegrationPreviewItem> Previews,
    IReadOnlyList<string> Errors,
    DateTimeOffset RefreshedAt);
