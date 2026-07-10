namespace LifeOS.Shared.IntegrationConnectors.GoogleCalendar;

public sealed class GoogleCalendarConnectorConfiguration
{
    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
    public string RedirectUri { get; init; } = "http://127.0.0.1:53682/";
    public string CalendarId { get; init; } = "primary";

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ClientId) || ClientId.StartsWith("REPLACE_", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Google Calendar client ID is not configured. Add the Desktop OAuth client ID to the local connector configuration file.");
        }

        if (string.IsNullOrWhiteSpace(ClientSecret) || ClientSecret.StartsWith("REPLACE_", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Google Calendar client secret is not configured. Add the Desktop OAuth client secret to the local connector configuration file. This value remains local and must not be committed.");
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
