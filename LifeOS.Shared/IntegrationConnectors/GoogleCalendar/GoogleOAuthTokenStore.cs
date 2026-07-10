using System.Text;
using System.Text.Json;

namespace LifeOS.Shared.IntegrationConnectors.GoogleCalendar;

public static class GoogleOAuthTokenStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = false };
    public static string FilePath => Path.Combine(GoogleCalendarConfigurationStore.DirectoryPath, "token.cache");

    public static bool Exists => File.Exists(FilePath);

    public static GoogleOAuthToken? Load()
    {
        if (!File.Exists(FilePath)) return null;
        var protectedBytes = File.ReadAllBytes(FilePath);
        var json = Encoding.UTF8.GetString(WindowsProtectedData.Unprotect(protectedBytes));
        return JsonSerializer.Deserialize<GoogleOAuthToken>(json, Options);
    }

    public static void Save(GoogleOAuthToken token)
    {
        Directory.CreateDirectory(GoogleCalendarConfigurationStore.DirectoryPath);
        var json = JsonSerializer.Serialize(token, Options);
        File.WriteAllBytes(FilePath, WindowsProtectedData.Protect(Encoding.UTF8.GetBytes(json)));
    }

    public static void Delete()
    {
        if (File.Exists(FilePath)) File.Delete(FilePath);
    }
}
