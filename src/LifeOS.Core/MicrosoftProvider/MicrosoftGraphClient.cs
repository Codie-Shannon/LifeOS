using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace LifeOS.Core.MicrosoftProvider;

public sealed class MicrosoftGraphException : Exception
{
    public MicrosoftGraphException(
        HttpStatusCode statusCode,
        string code,
        string sanitizedMessage)
        : base(sanitizedMessage)
    {
        StatusCode = statusCode;
        Code = code;
    }

    public HttpStatusCode StatusCode { get; }

    public string Code { get; }
}

public sealed class MicrosoftGraphClient
{
    private readonly HttpClient _httpClient;

    public MicrosoftGraphClient(
        HttpClient httpClient,
        string accessToken)
    {
        ArgumentNullException.ThrowIfNull(httpClient);

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new ArgumentException(
                "Access token is required.",
                nameof(accessToken));
        }

        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<MicrosoftGraphIdentity> GetIdentityAsync(
        CancellationToken cancellationToken = default)
    {
        using JsonDocument document = await GetJsonAsync(
            "https://graph.microsoft.com/v1.0/me?" +
            "$select=id,displayName,userPrincipalName,mail",
            cancellationToken);

        JsonElement root = document.RootElement;
        return new MicrosoftGraphIdentity(
            GetString(root, "id"),
            GetString(root, "displayName"),
            GetString(root, "userPrincipalName"),
            GetString(root, "mail"),
            ReadTenantIdFromClaimsUnavailable());
    }

    public async Task<IReadOnlyList<MicrosoftGraphMessage>> GetMessagesAsync(
        string folderId,
        DateTimeOffset fromUtc,
        int maximumItems,
        CancellationToken cancellationToken = default)
    {
        string escapedFolder = Uri.EscapeDataString(
            string.IsNullOrWhiteSpace(folderId) ? "inbox" : folderId.Trim());

        string filter = Uri.EscapeDataString(
            $"receivedDateTime ge {fromUtc.UtcDateTime:yyyy-MM-ddTHH:mm:ssZ}");

        string url =
            $"https://graph.microsoft.com/v1.0/me/mailFolders/{escapedFolder}/messages" +
            "?$select=id,subject,from,toRecipients,receivedDateTime,sentDateTime," +
            "importance,isRead,conversationId,hasAttachments,lastModifiedDateTime" +
            "&$orderby=receivedDateTime desc" +
            $"&$filter={filter}" +
            $"&$top={Math.Clamp(maximumItems, 1, 100)}";

        List<MicrosoftGraphMessage> results = [];
        await ReadPagedAsync(
            url,
            Math.Clamp(maximumItems, 1, 200),
            async element =>
            {
                bool hasAttachments = GetBoolean(
                    element,
                    "hasAttachments");

                IReadOnlyList<MicrosoftAttachmentMetadata> attachments =
                    hasAttachments
                        ? await GetAttachmentMetadataAsync(
                            GetString(element, "id"),
                            cancellationToken)
                        : [];

                results.Add(new MicrosoftGraphMessage(
                    GetString(element, "id"),
                    GetString(element, "subject"),
                    ReadEmailAddress(element, "from"),
                    ReadRecipients(element, "toRecipients"),
                    GetDateTimeOffset(element, "receivedDateTime"),
                    GetNullableDateTimeOffset(element, "sentDateTime"),
                    GetString(element, "importance"),
                    GetBoolean(element, "isRead"),
                    GetString(element, "conversationId"),
                    hasAttachments,
                    GetNullableDateTimeOffset(
                        element,
                        "lastModifiedDateTime"),
                    attachments,
                    IsRemoved(element)));
            },
            cancellationToken);

        return results;
    }

    public async Task<IReadOnlyList<MicrosoftGraphCalendarEvent>>
        GetCalendarEventsAsync(
            string calendarId,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            int maximumItems,
            CancellationToken cancellationToken = default)
    {
        string calendarPath =
            string.IsNullOrWhiteSpace(calendarId) ||
            string.Equals(
                calendarId,
                "default",
                StringComparison.OrdinalIgnoreCase)
                ? "me/calendarView"
                : $"me/calendars/{Uri.EscapeDataString(calendarId)}/calendarView";

        string url =
            $"https://graph.microsoft.com/v1.0/{calendarPath}" +
            $"?startDateTime={Uri.EscapeDataString(startUtc.UtcDateTime.ToString("O"))}" +
            $"&endDateTime={Uri.EscapeDataString(endUtc.UtcDateTime.ToString("O"))}" +
            "&$select=id,subject,start,end,location,organizer,attendees," +
            "responseStatus,recurrence,onlineMeeting,onlineMeetingUrl," +
            "lastModifiedDateTime,isCancelled" +
            "&$orderby=start/dateTime" +
            $"&$top={Math.Clamp(maximumItems, 1, 100)}";

        using HttpRequestMessage request = new(HttpMethod.Get, url);
        request.Headers.TryAddWithoutValidation(
            "Prefer",
            "outlook.timezone=\"UTC\"");

        List<MicrosoftGraphCalendarEvent> results = [];
        await ReadPagedAsync(
            request,
            Math.Clamp(maximumItems, 1, 500),
            element =>
            {
                results.Add(new MicrosoftGraphCalendarEvent(
                    GetString(element, "id"),
                    GetString(element, "subject"),
                    ReadGraphDateTime(element, "start"),
                    ReadGraphDateTime(element, "end"),
                    ReadGraphTimeZone(element, "start"),
                    ReadLocation(element),
                    ReadEmailAddress(element, "organizer"),
                    ReadRecipients(element, "attendees"),
                    ReadNestedString(
                        element,
                        "responseStatus",
                        "response"),
                    ReadRecurrence(element),
                    ReadOnlineMeeting(element),
                    GetNullableDateTimeOffset(
                        element,
                        "lastModifiedDateTime"),
                    GetBoolean(element, "isCancelled"),
                    IsRemoved(element)));
                return Task.CompletedTask;
            },
            cancellationToken);

        return results;
    }

    private async Task<IReadOnlyList<MicrosoftAttachmentMetadata>>
        GetAttachmentMetadataAsync(
            string messageId,
            CancellationToken cancellationToken)
    {
        string url =
            $"https://graph.microsoft.com/v1.0/me/messages/" +
            $"{Uri.EscapeDataString(messageId)}/attachments" +
            "?$select=id,name,contentType,size,isInline";

        List<MicrosoftAttachmentMetadata> results = [];
        await ReadPagedAsync(
            url,
            maximumItems: 25,
            element =>
            {
                results.Add(new MicrosoftAttachmentMetadata(
                    GetString(element, "id"),
                    GetString(element, "name"),
                    GetString(element, "contentType"),
                    GetInt64(element, "size"),
                    GetBoolean(element, "isInline"),
                    GetString(element, "@odata.type")));
                return Task.CompletedTask;
            },
            cancellationToken);

        return results;
    }

    private async Task ReadPagedAsync(
        string url,
        int maximumItems,
        Func<JsonElement, Task> add,
        CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, url);
        await ReadPagedAsync(
            request,
            maximumItems,
            add,
            cancellationToken);
    }

    private async Task ReadPagedAsync(
        HttpRequestMessage firstRequest,
        int maximumItems,
        Func<JsonElement, Task> add,
        CancellationToken cancellationToken)
    {
        string? nextUrl = firstRequest.RequestUri?.ToString();
        Dictionary<string, IEnumerable<string>> headers =
            firstRequest.Headers.ToDictionary(
                header => header.Key,
                header => header.Value,
                StringComparer.OrdinalIgnoreCase);
        int pages = 0;
        int items = 0;

        while (!string.IsNullOrWhiteSpace(nextUrl) &&
               pages < 20 &&
               items < maximumItems)
        {
            using HttpRequestMessage request =
                new(HttpMethod.Get, nextUrl);

            foreach (KeyValuePair<string, IEnumerable<string>> header in
                     headers)
            {
                request.Headers.TryAddWithoutValidation(
                    header.Key,
                    header.Value);
            }

            using JsonDocument document = await SendJsonAsync(
                request,
                cancellationToken);

            JsonElement root = document.RootElement;
            if (root.TryGetProperty("value", out JsonElement value) &&
                value.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement element in value.EnumerateArray())
                {
                    await add(element);
                    items++;
                    if (items >= maximumItems)
                    {
                        break;
                    }
                }
            }

            nextUrl = root.TryGetProperty(
                    "@odata.nextLink",
                    out JsonElement next)
                ? next.GetString()
                : null;
            pages++;
        }
    }

    private async Task<JsonDocument> GetJsonAsync(
        string url,
        CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, url);
        return await SendJsonAsync(request, cancellationToken);
    }

    private async Task<JsonDocument> SendJsonAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        string json = await response.Content.ReadAsStringAsync(
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            string code = "graph-error";
            string message =
                $"Microsoft Graph returned {(int)response.StatusCode}.";

            try
            {
                using JsonDocument errorDocument =
                    JsonDocument.Parse(json);
                if (errorDocument.RootElement.TryGetProperty(
                        "error",
                        out JsonElement error))
                {
                    code = GetString(error, "code");
                    string providerMessage =
                        GetString(error, "message");
                    if (!string.IsNullOrWhiteSpace(providerMessage))
                    {
                        message = providerMessage.Length > 240
                            ? providerMessage[..240]
                            : providerMessage;
                    }
                }
            }
            catch (JsonException)
            {
                // Keep sanitized status-only error.
            }

            throw new MicrosoftGraphException(
                response.StatusCode,
                string.IsNullOrWhiteSpace(code)
                    ? "graph-error"
                    : code,
                message);
        }

        return JsonDocument.Parse(
            string.IsNullOrWhiteSpace(json) ? "{}" : json);
    }

    private static string GetString(
        JsonElement element,
        string propertyName) =>
        element.TryGetProperty(propertyName, out JsonElement value) &&
        value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;

    private static bool GetBoolean(
        JsonElement element,
        string propertyName) =>
        element.TryGetProperty(propertyName, out JsonElement value) &&
        value.ValueKind is JsonValueKind.True or JsonValueKind.False &&
        value.GetBoolean();

    private static long GetInt64(
        JsonElement element,
        string propertyName) =>
        element.TryGetProperty(propertyName, out JsonElement value) &&
        value.TryGetInt64(out long parsed)
            ? parsed
            : 0;

    private static DateTimeOffset GetDateTimeOffset(
        JsonElement element,
        string propertyName) =>
        GetNullableDateTimeOffset(element, propertyName) ??
        DateTimeOffset.UnixEpoch;

    private static DateTimeOffset? GetNullableDateTimeOffset(
        JsonElement element,
        string propertyName)
    {
        string value = GetString(element, propertyName);
        return DateTimeOffset.TryParse(value, out DateTimeOffset parsed)
            ? parsed
            : null;
    }

    private static string ReadNestedString(
        JsonElement element,
        string parentName,
        string childName)
    {
        if (!element.TryGetProperty(
                parentName,
                out JsonElement parent) ||
            parent.ValueKind != JsonValueKind.Object)
        {
            return string.Empty;
        }

        return GetString(parent, childName);
    }

    private static string ReadEmailAddress(
        JsonElement element,
        string propertyName)
    {
        if (!element.TryGetProperty(
                propertyName,
                out JsonElement parent) ||
            parent.ValueKind != JsonValueKind.Object ||
            !parent.TryGetProperty(
                "emailAddress",
                out JsonElement address) ||
            address.ValueKind != JsonValueKind.Object)
        {
            return string.Empty;
        }

        string name = GetString(address, "name");
        string email = GetString(address, "address");
        return string.IsNullOrWhiteSpace(name)
            ? email
            : $"{name} <{email}>";
    }

    private static string ReadRecipients(
        JsonElement element,
        string propertyName)
    {
        if (!element.TryGetProperty(
                propertyName,
                out JsonElement recipients) ||
            recipients.ValueKind != JsonValueKind.Array)
        {
            return string.Empty;
        }

        List<string> values = [];
        foreach (JsonElement recipient in recipients.EnumerateArray())
        {
            if (!recipient.TryGetProperty(
                    "emailAddress",
                    out JsonElement address) ||
                address.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            string name = GetString(address, "name");
            string email = GetString(address, "address");
            string value = string.IsNullOrWhiteSpace(name)
                ? email
                : $"{name} <{email}>";

            if (!string.IsNullOrWhiteSpace(value))
            {
                values.Add(value);
            }
        }

        return string.Join("; ", values);
    }

    private static DateTimeOffset ReadGraphDateTime(
        JsonElement element,
        string propertyName)
    {
        string value = ReadNestedString(
            element,
            propertyName,
            "dateTime");

        return DateTimeOffset.TryParse(
            value,
            out DateTimeOffset parsed)
            ? parsed
            : DateTimeOffset.UnixEpoch;
    }

    private static string ReadGraphTimeZone(
        JsonElement element,
        string propertyName) =>
        ReadNestedString(element, propertyName, "timeZone");

    private static string ReadLocation(JsonElement element)
    {
        if (!element.TryGetProperty(
                "location",
                out JsonElement location) ||
            location.ValueKind != JsonValueKind.Object)
        {
            return string.Empty;
        }

        return GetString(location, "displayName");
    }

    private static string ReadRecurrence(JsonElement element)
    {
        if (!element.TryGetProperty(
                "recurrence",
                out JsonElement recurrence) ||
            recurrence.ValueKind is JsonValueKind.Null or
                JsonValueKind.Undefined)
        {
            return string.Empty;
        }

        return recurrence.GetRawText();
    }

    private static string ReadOnlineMeeting(JsonElement element)
    {
        string direct = GetString(element, "onlineMeetingUrl");
        if (!string.IsNullOrWhiteSpace(direct))
        {
            return direct;
        }

        if (element.TryGetProperty(
                "onlineMeeting",
                out JsonElement meeting) &&
            meeting.ValueKind == JsonValueKind.Object)
        {
            return GetString(meeting, "joinUrl");
        }

        return string.Empty;
    }

    private static bool IsRemoved(JsonElement element) =>
        element.TryGetProperty(
            "@removed",
            out JsonElement removed) &&
        removed.ValueKind == JsonValueKind.Object;

    private static string ReadTenantIdFromClaimsUnavailable() =>
        string.Empty;

}
