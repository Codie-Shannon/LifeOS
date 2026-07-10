using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using LifeOS.Core.IntegrationConnectors.GoogleCalendar;

namespace LifeOS.Shared.IntegrationConnectors.GoogleCalendar;

public sealed class GoogleCalendarApiProvider(HttpClient httpClient, GoogleCalendarConnectorConfiguration configuration, Func<Task<string>> accessTokenFactory)
    : IReadOnlyCalendarProvider
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly GoogleCalendarConnectorConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    private readonly Func<Task<string>> _accessTokenFactory = accessTokenFactory ?? throw new ArgumentNullException(nameof(accessTokenFactory));

    public async Task<IReadOnlyList<ExternalCalendarEvent>> GetEventsAsync(CalendarRefreshRange range, CancellationToken cancellationToken = default)
    {
        range.Validate(DateTimeOffset.Now);
        _configuration.Validate();
        var token = await _accessTokenFactory().ConfigureAwait(false);
        var calendarId = Uri.EscapeDataString(_configuration.CalendarId);
        var query = $"https://www.googleapis.com/calendar/v3/calendars/{calendarId}/events?singleEvents=true&orderBy=startTime&maxResults=2500&timeMin={Uri.EscapeDataString(range.From.UtcDateTime.ToString("O", CultureInfo.InvariantCulture))}&timeMax={Uri.EscapeDataString(range.To.UtcDateTime.ToString("O", CultureInfo.InvariantCulture))}";
        using var request = new HttpRequestMessage(HttpMethod.Get, query);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Google Calendar refresh failed ({(int)response.StatusCode}). {ReadGoogleError(body)}");
        }

        using var document = JsonDocument.Parse(body);
        var events = new List<ExternalCalendarEvent>();
        if (!document.RootElement.TryGetProperty("items", out var items) || items.ValueKind != JsonValueKind.Array) return events;
        foreach (var item in items.EnumerateArray())
        {
            var status = String(item, "status");
            if (status.Equals("cancelled", StringComparison.OrdinalIgnoreCase)) continue;
            var id = String(item, "id");
            var title = String(item, "summary");
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(title)) continue;
            events.Add(new ExternalCalendarEvent(
                id, title, Date(item, "start"), Date(item, "end"), String(item, "location"), String(item, "description"),
                _configuration.CalendarId, "Google Calendar", "Connected Google account", String(item, "htmlLink"), DateTimeOffset.Now));
        }
        return events;
    }

    private static string String(JsonElement element, string name) => element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() ?? string.Empty : string.Empty;
    private static DateTimeOffset? Date(JsonElement item, string name)
    {
        if (!item.TryGetProperty(name, out var container)) return null;
        var raw = String(container, "dateTime");
        if (string.IsNullOrWhiteSpace(raw)) raw = String(container, "date");
        return DateTimeOffset.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var value) ? value : null;
    }
    private static string ReadGoogleError(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            return doc.RootElement.TryGetProperty("error", out var error) && error.TryGetProperty("message", out var message) ? message.GetString() ?? "Provider error." : "Provider error.";
        }
        catch { return "Provider returned an unreadable error."; }
    }
}
