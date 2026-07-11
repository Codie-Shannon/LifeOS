using System.Text.Json;
using System.Text.RegularExpressions;

namespace LifeOS.Shared.IntegrationConnectors.GoogleCalendar;

public static class GoogleCalendarLifecycleStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    public static string FilePath => Path.Combine(GoogleCalendarConfigurationStore.DirectoryPath, "lifecycle.json");

    public static GoogleCalendarLifecycleSnapshot Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return DeriveCurrentState(new GoogleCalendarLifecycleSnapshot());
            var value = JsonSerializer.Deserialize<GoogleCalendarLifecycleSnapshot>(File.ReadAllText(FilePath), Options)
                ?? new GoogleCalendarLifecycleSnapshot();
            return DeriveCurrentState(value);
        }
        catch
        {
            return DeriveCurrentState(new GoogleCalendarLifecycleSnapshot
            {
                State = GoogleCalendarLifecycleState.RefreshFailed,
                LastResultSummary = "Local connector lifecycle metadata could not be read.",
                LastSanitizedError = "Lifecycle metadata is malformed or inaccessible. Clear the local connector cache and reconnect."
            });
        }
    }

    public static void Save(GoogleCalendarLifecycleSnapshot snapshot)
    {
        Directory.CreateDirectory(GoogleCalendarConfigurationStore.DirectoryPath);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(snapshot, Options));
    }

    public static bool ConfigurationPresent()
    {
        try
        {
            if (!File.Exists(GoogleCalendarConfigurationStore.FilePath)) return false;
            GoogleCalendarConfigurationStore.Load().Validate();
            return true;
        }
        catch { return false; }
    }

    public static GoogleCalendarLifecycleSnapshot DeriveCurrentState(GoogleCalendarLifecycleSnapshot snapshot)
    {
        if (!ConfigurationPresent())
        {
            snapshot.State = GoogleCalendarLifecycleState.ConfigurationMissing;
            return snapshot;
        }

        if (!GoogleOAuthTokenStore.Exists && snapshot.State is (GoogleCalendarLifecycleState.ConnectedLocally or GoogleCalendarLifecycleState.RefreshInProgress))
        {
            snapshot.State = GoogleCalendarLifecycleState.TokenCacheMissing;
            snapshot.LastResultSummary = "Configuration exists, but the local authorization cache is missing. Reconnect explicitly.";
        }
        else if (GoogleOAuthTokenStore.Exists && snapshot.State is (GoogleCalendarLifecycleState.ConfigurationMissing or GoogleCalendarLifecycleState.ConfiguredDisconnected or GoogleCalendarLifecycleState.TokenCacheMissing or GoogleCalendarLifecycleState.Disconnected or GoogleCalendarLifecycleState.LocalCacheCleared))
        {
            snapshot.State = GoogleCalendarLifecycleState.ConnectedLocally;
        }
        else if (!GoogleOAuthTokenStore.Exists && snapshot.State == GoogleCalendarLifecycleState.ConfigurationMissing)
        {
            snapshot.State = GoogleCalendarLifecycleState.ConfiguredDisconnected;
        }

        return snapshot;
    }

    public static string SanitizeError(Exception exception)
    {
        var text = exception.Message ?? "Connector action failed.";
        text = Regex.Replace(text, @"(?i)(access_token|refresh_token|client_secret|authorization_code|code)=?[^\s,&]+", "$1=[redacted]");
        text = Regex.Replace(text, @"https?://[^\s]+", "[provider endpoint redacted]");
        text = Regex.Replace(text, @"(?i)ya29\.[A-Za-z0-9._-]+", "[access token redacted]");
        text = text.Replace("\r", " ").Replace("\n", " ").Trim();
        return text.Length <= 320 ? text : text[..320] + "…";
    }

    public static GoogleCalendarLifecycleState ClassifyFailure(Exception exception)
    {
        var text = (exception.Message ?? string.Empty).ToLowerInvariant();
        if (text.Contains("revoked") || text.Contains("invalid_grant") || text.Contains("forbidden") || text.Contains("403")) return GoogleCalendarLifecycleState.AccessRevoked;
        if (text.Contains("expired") || text.Contains("unauthorized") || text.Contains("401") || text.Contains("not connected")) return GoogleCalendarLifecycleState.AuthenticationExpired;
        return GoogleCalendarLifecycleState.RefreshFailed;
    }

    public static void ClearLocalCache()
    {
        GoogleOAuthTokenStore.Delete();
        if (File.Exists(FilePath)) File.Delete(FilePath);
        Save(new GoogleCalendarLifecycleSnapshot
        {
            State = GoogleCalendarLifecycleState.LocalCacheCleared,
            LastCacheClearAt = DateTimeOffset.Now,
            LastResultSummary = "Local connector token and lifecycle cache were cleared. Imported Integration Inbox evidence was retained."
        });
    }
}
