namespace LifeOS.Core.IntegrationConnectors.GoogleCalendar;

public interface IReadOnlyCalendarProvider
{
    Task<IReadOnlyList<ExternalCalendarEvent>> GetEventsAsync(
        CalendarRefreshRange range,
        CancellationToken cancellationToken = default);
}
