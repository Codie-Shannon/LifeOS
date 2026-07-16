using System.Net.Http.Headers;
using System.Text.Json;

namespace LifeOS.Core.MicrosoftProvider;

public sealed class MicrosoftGraphFilesReader : IMicrosoftFilesReader
{
    private readonly HttpClient _http;
    public MicrosoftGraphFilesReader(HttpClient http, string accessToken)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        if (string.IsNullOrWhiteSpace(accessToken)) throw new ArgumentException("Access token is required.", nameof(accessToken));
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    public async Task<IReadOnlyList<MicrosoftDriveDescriptor>> GetDrivesAsync(CancellationToken cancellationToken = default)
    {
        List<MicrosoftDriveDescriptor> values = [];
        await ReadPagedAsync("https://graph.microsoft.com/v1.0/me/drives?$select=id,name,driveType,webUrl", 100, e =>
            values.Add(new(Str(e,"id"), Str(e,"name"), Str(e,"driveType"), Str(e,"webUrl"))), cancellationToken);
        return values;
    }

    public async Task<IReadOnlyList<MicrosoftSiteDescriptor>> GetSitesAsync(CancellationToken cancellationToken = default)
    {
        List<MicrosoftSiteDescriptor> values = [];
        await ReadPagedAsync("https://graph.microsoft.com/v1.0/sites?search=*&$select=id,displayName,webUrl", 100, e =>
            values.Add(new(Str(e,"id"), Str(e,"displayName"), Str(e,"webUrl"))), cancellationToken);
        return values;
    }

    public async Task<IReadOnlyList<MicrosoftLibraryDescriptor>> GetLibrariesAsync(string siteId, CancellationToken cancellationToken = default)
    {
        List<MicrosoftLibraryDescriptor> values = [];
        string url = $"https://graph.microsoft.com/v1.0/sites/{Uri.EscapeDataString(siteId)}/drives?$select=id,name,webUrl";
        await ReadPagedAsync(url, 100, e => values.Add(new(Str(e,"id"), siteId, Str(e,"name"), Str(e,"webUrl"))), cancellationToken);
        return values;
    }

    public async Task<IReadOnlyList<MicrosoftFileDescriptor>> GetDriveItemsAsync(string driveId, string? folderId, DateTimeOffset modifiedSinceUtc, int maximumItems, CancellationToken cancellationToken = default)
    {
        string root = string.IsNullOrWhiteSpace(folderId) ? "root" : $"items/{Uri.EscapeDataString(folderId)}";
        string url = $"https://graph.microsoft.com/v1.0/drives/{Uri.EscapeDataString(driveId)}/{root}/children?$select=id,name,size,createdDateTime,lastModifiedDateTime,webUrl,parentReference,createdBy,lastModifiedBy,eTag,file,folder,deleted,sharepointIds&$top={Math.Clamp(maximumItems,1,200)}";
        List<MicrosoftFileDescriptor> values = [];
        await ReadPagedAsync(url, maximumItems, e =>
        {
            DateTimeOffset modified = Date(e,"lastModifiedDateTime");
            if (modified < modifiedSinceUtc) return;
            bool isFolder = e.TryGetProperty("folder", out _);
            string siteId = Nested(e,"parentReference","siteId");
            string libraryId = Nested(e,"sharepointIds","listId");
            MicrosoftFileSourceKind kind = string.IsNullOrWhiteSpace(siteId) ? MicrosoftFileSourceKind.OneDrive : MicrosoftFileSourceKind.SharePoint;
            MicrosoftFileSourceState state = e.TryGetProperty("deleted", out _) ? MicrosoftFileSourceState.SourceRemoved : MicrosoftFileSourceState.Current;
            values.Add(new(Str(e,"id"), Str(e,"name"), isFolder, Long(e,"size"), Date(e,"createdDateTime"), modified, Str(e,"webUrl"), Nested(e,"parentReference","path"), Owner(e), Str(e,"eTag"), driveId, siteId, libraryId, kind, state));
        }, cancellationToken);
        return values;
    }

    private async Task ReadPagedAsync(string url, int maximum, Action<JsonElement> add, CancellationToken ct)
    {
        int count=0, pages=0;
        while (!string.IsNullOrWhiteSpace(url) && count < maximum && pages++ < 20)
        {
            using HttpResponseMessage response = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
            string json = await response.Content.ReadAsStringAsync(ct);
            response.EnsureSuccessStatusCode();
            using JsonDocument doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("value", out JsonElement array))
                foreach (JsonElement item in array.EnumerateArray()) { add(item); if (++count >= maximum) break; }
            url = doc.RootElement.TryGetProperty("@odata.nextLink", out JsonElement next) ? next.GetString() ?? string.Empty : string.Empty;
        }
    }

    private static string Str(JsonElement e,string n)=>e.TryGetProperty(n,out var v)&&v.ValueKind==JsonValueKind.String?v.GetString()??string.Empty:string.Empty;
    private static long Long(JsonElement e,string n)=>e.TryGetProperty(n,out var v)&&v.TryGetInt64(out long x)?x:0;
    private static DateTimeOffset Date(JsonElement e,string n)=>DateTimeOffset.TryParse(Str(e,n),out var x)?x:DateTimeOffset.UnixEpoch;
    private static string Nested(JsonElement e,string p,string n)=>e.TryGetProperty(p,out var x)?Str(x,n):string.Empty;
    private static string Owner(JsonElement e) => e.TryGetProperty("lastModifiedBy", out var by) && by.TryGetProperty("user", out var user) ? Str(user, "displayName") : string.Empty;
}
