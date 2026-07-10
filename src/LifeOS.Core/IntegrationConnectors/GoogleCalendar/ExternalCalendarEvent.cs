namespace LifeOS.Core.IntegrationConnectors.GoogleCalendar;

public sealed record ExternalCalendarEvent(
    string ExternalId,
    string Title,
    DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt,
    string Location,
    string Description,
    string CalendarId,
    string CalendarLabel,
    string ProviderAccountLabel,
    string HtmlLink,
    DateTimeOffset FetchedAt);
