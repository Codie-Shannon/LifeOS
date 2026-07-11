namespace LifeOS.Shared.IntegrationConnectors.GoogleCalendar;

public static class GoogleCalendarConnectorCache
{
    public static void Disconnect() => GoogleOAuthTokenStore.Delete();
    public static void ClearLocalConnectorCache() => GoogleCalendarLifecycleStore.ClearLocalCache();
}
