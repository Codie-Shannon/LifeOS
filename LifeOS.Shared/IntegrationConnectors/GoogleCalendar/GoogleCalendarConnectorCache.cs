namespace LifeOS.Shared.IntegrationConnectors.GoogleCalendar;

public static class GoogleCalendarConnectorCache
{
    public static string DirectoryPath => GoogleCalendarConfigurationStore.DirectoryPath;

    public static void Disconnect() => GoogleOAuthTokenStore.Delete();

    public static void DeleteLocalCache()
    {
        GoogleOAuthTokenStore.Delete();
    }
}
