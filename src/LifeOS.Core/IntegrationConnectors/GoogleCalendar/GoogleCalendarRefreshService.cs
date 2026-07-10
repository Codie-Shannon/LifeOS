using LifeOS.Core.IntegrationInbox;

namespace LifeOS.Core.IntegrationConnectors.GoogleCalendar;

public sealed class GoogleCalendarRefreshService(IReadOnlyCalendarProvider provider, Func<DateTimeOffset>? clock = null)
{
    private readonly IReadOnlyCalendarProvider _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    private readonly Func<DateTimeOffset> _clock = clock ?? (() => DateTimeOffset.Now);

    public async Task<GoogleCalendarRefreshResult> RefreshAsync(
        CalendarRefreshRange range,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(range);
        range.Validate(_clock());

        var events = await _provider.GetEventsAsync(range, cancellationToken).ConfigureAwait(false);
        var previews = new List<IntegrationPreviewItem>();
        var errors = new List<string>();

        foreach (var calendarEvent in events)
        {
            try
            {
                previews.Add(GoogleCalendarPreviewMapper.Map(calendarEvent));
            }
            catch (ArgumentException ex)
            {
                errors.Add(ex.Message);
            }
        }

        return new GoogleCalendarRefreshResult(range, previews, errors, _clock());
    }
}
