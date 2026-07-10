namespace LifeOS.Shared.IntegrationConnectors.GoogleCalendar;

public sealed class GoogleCalendarConnectorConfiguration
{
    public string ClientId { get; init; } = string.Empty;
    public string RedirectUri { get; init; } = "http://127.0.0.1:53682/";
    public string CalendarId { get; init; } = "primary";

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ClientId))
        {
            throw new InvalidOperationException("Google Calendar client ID is not configured. Add it to the local connector configuration file.");
        }

        if (!Uri.TryCreate(RedirectUri, UriKind.Absolute, out var redirect) || redirect.Scheme != Uri.UriSchemeHttp || !redirect.IsLoopback)
        {
            throw new InvalidOperationException("Google Calendar redirect URI must be a local HTTP loopback address.");
        }

        if (string.IsNullOrWhiteSpace(CalendarId))
        {
            throw new InvalidOperationException("Google Calendar ID is required.");
        }
    }
}
