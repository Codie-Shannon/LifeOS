using System.Text.Json;

namespace LifeOS.Shared.IntegrationConnectors.GoogleCalendar;

public static class GoogleCalendarConfigurationStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string DirectoryPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LifeOS", "connectors", "google-calendar");

    public static string FilePath => Path.Combine(DirectoryPath, "configuration.json");

    public static GoogleCalendarConnectorConfiguration Load()
    {
        EnsureTemplate();
        var value = JsonSerializer.Deserialize<GoogleCalendarConnectorConfiguration>(File.ReadAllText(FilePath), Options)
            ?? new GoogleCalendarConnectorConfiguration();
        return value;
    }

    public static void EnsureTemplate()
    {
        Directory.CreateDirectory(DirectoryPath);
        if (!File.Exists(FilePath))
        {
            File.WriteAllText(FilePath, JsonSerializer.Serialize(new GoogleCalendarConnectorConfiguration
            {
                ClientId = "REPLACE_WITH_LOCAL_GOOGLE_OAUTH_CLIENT_ID",
                RedirectUri = "http://127.0.0.1:53682/",
                CalendarId = "primary"
            }, Options));
        }
    }
}
