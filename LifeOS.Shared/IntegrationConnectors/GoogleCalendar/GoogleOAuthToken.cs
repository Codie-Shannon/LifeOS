namespace LifeOS.Shared.IntegrationConnectors.GoogleCalendar;

public sealed class GoogleOAuthToken
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; init; }
    public string Scope { get; init; } = string.Empty;
    public string TokenType { get; init; } = "Bearer";

    public bool IsUsable(DateTimeOffset now) =>
        !string.IsNullOrWhiteSpace(AccessToken) && ExpiresAt > now.AddMinutes(2);
}
