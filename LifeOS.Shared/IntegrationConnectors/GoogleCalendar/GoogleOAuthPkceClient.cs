using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace LifeOS.Shared.IntegrationConnectors.GoogleCalendar;

public sealed class GoogleOAuthPkceClient(HttpClient httpClient, GoogleCalendarConnectorConfiguration configuration)
{
    public const string ReadOnlyScope = "https://www.googleapis.com/auth/calendar.readonly";
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly GoogleCalendarConnectorConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        _configuration.Validate();
        var verifier = Base64Url(RandomNumberGenerator.GetBytes(64));
        var challenge = Base64Url(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)));
        var state = Base64Url(RandomNumberGenerator.GetBytes(32));
        using var listener = new HttpListener();
        listener.Prefixes.Add(_configuration.RedirectUri);
        listener.Start();

        var authorizeUrl = "https://accounts.google.com/o/oauth2/v2/auth" +
            $"?client_id={Uri.EscapeDataString(_configuration.ClientId)}" +
            $"&redirect_uri={Uri.EscapeDataString(_configuration.RedirectUri)}" +
            "&response_type=code" +
            $"&scope={Uri.EscapeDataString(ReadOnlyScope)}" +
            "&access_type=offline&prompt=consent" +
            $"&state={Uri.EscapeDataString(state)}" +
            $"&code_challenge={Uri.EscapeDataString(challenge)}&code_challenge_method=S256";

        Process.Start(new ProcessStartInfo(authorizeUrl) { UseShellExecute = true });
        var contextTask = listener.GetContextAsync();
        var completed = await Task.WhenAny(contextTask, Task.Delay(TimeSpan.FromMinutes(5), cancellationToken)).ConfigureAwait(false);
        if (completed != contextTask) throw new TimeoutException("Google Calendar connection timed out.");
        var context = await contextTask.ConfigureAwait(false);
        var query = context.Request.QueryString;
        var returnedState = query["state"];
        var code = query["code"];
        var error = query["error"];
        var responseText = string.IsNullOrWhiteSpace(error) ? "LifeOS received the Google Calendar response. You can close this tab." : "Google Calendar connection was not completed. You can close this tab.";
        var responseBytes = Encoding.UTF8.GetBytes($"<html><body><h2>{WebUtility.HtmlEncode(responseText)}</h2></body></html>");
        context.Response.ContentType = "text/html; charset=utf-8";
        context.Response.ContentLength64 = responseBytes.Length;
        await context.Response.OutputStream.WriteAsync(responseBytes, cancellationToken).ConfigureAwait(false);
        context.Response.Close();
        listener.Stop();

        if (!string.IsNullOrWhiteSpace(error)) throw new InvalidOperationException($"Google authorization failed: {error}.");
        if (!string.Equals(state, returnedState, StringComparison.Ordinal)) throw new InvalidOperationException("Google authorization state check failed.");
        if (string.IsNullOrWhiteSpace(code)) throw new InvalidOperationException("Google authorization code was not returned.");

        var token = await ExchangeCodeAsync(code, verifier, cancellationToken).ConfigureAwait(false);
        GoogleOAuthTokenStore.Save(token);
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var token = GoogleOAuthTokenStore.Load() ?? throw new InvalidOperationException("Google Calendar is not connected.");
        if (token.IsUsable(DateTimeOffset.Now)) return token.AccessToken;
        if (string.IsNullOrWhiteSpace(token.RefreshToken)) throw new InvalidOperationException("Google Calendar authorization has expired. Disconnect and connect again.");

        using var response = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = _configuration.ClientId,
            ["refresh_token"] = token.RefreshToken,
            ["grant_type"] = "refresh_token"
        }), cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException($"Google token refresh failed ({(int)response.StatusCode}).");
        var refreshed = ParseToken(body, token.RefreshToken);
        GoogleOAuthTokenStore.Save(refreshed);
        return refreshed.AccessToken;
    }

    private async Task<GoogleOAuthToken> ExchangeCodeAsync(string code, string verifier, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = _configuration.ClientId,
            ["code"] = code,
            ["code_verifier"] = verifier,
            ["grant_type"] = "authorization_code",
            ["redirect_uri"] = _configuration.RedirectUri
        }), cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException($"Google token exchange failed ({(int)response.StatusCode}).");
        return ParseToken(body, string.Empty);
    }

    private static GoogleOAuthToken ParseToken(string body, string existingRefreshToken)
    {
        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        var accessToken = root.GetProperty("access_token").GetString() ?? string.Empty;
        var refreshToken = root.TryGetProperty("refresh_token", out var refresh) ? refresh.GetString() ?? existingRefreshToken : existingRefreshToken;
        var expiresIn = root.TryGetProperty("expires_in", out var expires) ? expires.GetInt32() : 3600;
        var scope = root.TryGetProperty("scope", out var scopeValue) ? scopeValue.GetString() ?? ReadOnlyScope : ReadOnlyScope;
        var tokenType = root.TryGetProperty("token_type", out var type) ? type.GetString() ?? "Bearer" : "Bearer";
        return new GoogleOAuthToken { AccessToken = accessToken, RefreshToken = refreshToken, ExpiresAt = DateTimeOffset.Now.AddSeconds(expiresIn), Scope = scope, TokenType = tokenType };
    }

    private static string Base64Url(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
