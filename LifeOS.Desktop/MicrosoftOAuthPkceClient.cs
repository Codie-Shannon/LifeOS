using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using LifeOS.Core.MicrosoftProvider;

namespace LifeOS.Desktop;

public sealed record MicrosoftAuthorizationResult(
    MicrosoftGraphIdentity Identity,
    MicrosoftTokenRecord Token);

public sealed class MicrosoftOAuthPkceClient
{
    private readonly HttpClient _httpClient;

    public MicrosoftOAuthPkceClient(HttpClient httpClient)
    {
        _httpClient = httpClient ??
            throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<MicrosoftAuthorizationResult> ConnectAsync(
        MicrosoftProviderConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        IReadOnlyList<string> validation = configuration.Validate();
        if (validation.Count > 0)
        {
            throw new InvalidOperationException(
                string.Join(" ", validation));
        }

        MicrosoftPkcePair pkce =
            MicrosoftOAuthProtocol.CreatePkce();
        string state = MicrosoftOAuthProtocol.CreateState();
        Uri authorizationUri =
            MicrosoftOAuthProtocol.BuildAuthorizationUri(
                configuration,
                pkce,
                state);

        using HttpListener listener = new();
        listener.Prefixes.Add(configuration.RedirectUri);
        listener.Start();

        Process.Start(new ProcessStartInfo
        {
            FileName = authorizationUri.ToString(),
            UseShellExecute = true
        });

        HttpListenerContext context = await WaitForCallbackAsync(
            listener,
            TimeSpan.FromMinutes(5),
            cancellationToken);

        string? returnedState =
            context.Request.QueryString["state"];
        string? code =
            context.Request.QueryString["code"];
        string? error =
            context.Request.QueryString["error"];
        string? errorDescription =
            context.Request.QueryString["error_description"];

        await WriteBrowserResponseAsync(
            context.Response,
            string.IsNullOrWhiteSpace(error) &&
            string.Equals(
                returnedState,
                state,
                StringComparison.Ordinal));

        if (!string.IsNullOrWhiteSpace(error))
        {
            throw new InvalidOperationException(
                $"Microsoft consent was not completed: " +
                $"{Sanitize(errorDescription ?? error)}");
        }

        if (!string.Equals(
                returnedState,
                state,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Microsoft sign-in state validation failed.");
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new InvalidOperationException(
                "Microsoft sign-in did not return an authorization code.");
        }

        MicrosoftTokenRecord token = await ExchangeCodeAsync(
            configuration,
            code,
            pkce.Verifier,
            cancellationToken);

        using HttpClient graphHttp = new();
        MicrosoftGraphIdentity identity =
            await new MicrosoftGraphClient(
                graphHttp,
                token.AccessToken)
            .GetIdentityAsync(cancellationToken);

        token.AccountId = $"microsoft-{identity.Id}";
        return new MicrosoftAuthorizationResult(
            identity,
            token);
    }

    public async Task<MicrosoftTokenRecord> RefreshAsync(
        MicrosoftProviderConfiguration configuration,
        MicrosoftTokenRecord existing,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(existing);

        if (string.IsNullOrWhiteSpace(existing.RefreshToken))
        {
            throw new InvalidOperationException(
                "No Microsoft refresh token is available. Reconnect is required.");
        }

        Dictionary<string, string> form = new()
        {
            ["client_id"] = configuration.ClientId,
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = existing.RefreshToken,
            ["redirect_uri"] = configuration.RedirectUri,
            ["scope"] = string.Join(
                " ",
                MicrosoftProviderConfiguration.Group49RequestedScopes)
        };

        using HttpResponseMessage response =
            await _httpClient.PostAsync(
                MicrosoftOAuthProtocol.BuildTokenUri(configuration),
                new FormUrlEncodedContent(form),
                cancellationToken);

        string json = await response.Content.ReadAsStringAsync(
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Microsoft token refresh failed with status " +
                $"{(int)response.StatusCode}.");
        }

        MicrosoftTokenRecord refreshed =
            ParseToken(json);
        refreshed.AccountId = existing.AccountId;

        if (string.IsNullOrWhiteSpace(refreshed.RefreshToken))
        {
            refreshed.RefreshToken = existing.RefreshToken;
        }

        return refreshed;
    }

    private async Task<MicrosoftTokenRecord> ExchangeCodeAsync(
        MicrosoftProviderConfiguration configuration,
        string code,
        string verifier,
        CancellationToken cancellationToken)
    {
        Dictionary<string, string> form = new()
        {
            ["client_id"] = configuration.ClientId,
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = configuration.RedirectUri,
            ["code_verifier"] = verifier,
            ["scope"] = string.Join(
                " ",
                MicrosoftProviderConfiguration.Group49RequestedScopes)
        };

        using HttpResponseMessage response =
            await _httpClient.PostAsync(
                MicrosoftOAuthProtocol.BuildTokenUri(configuration),
                new FormUrlEncodedContent(form),
                cancellationToken);

        string json = await response.Content.ReadAsStringAsync(
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Microsoft token exchange failed with status " +
                $"{(int)response.StatusCode}.");
        }

        return ParseToken(json);
    }

    private static MicrosoftTokenRecord ParseToken(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;

        string accessToken = GetString(root, "access_token");
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new InvalidOperationException(
                "Microsoft token response did not contain an access token.");
        }

        int expiresIn =
            root.TryGetProperty(
                "expires_in",
                out JsonElement expires) &&
            expires.TryGetInt32(out int parsed)
                ? parsed
                : 3600;

        return new MicrosoftTokenRecord
        {
            AccessToken = accessToken,
            RefreshToken = GetString(root, "refresh_token"),
            TokenType = GetString(root, "token_type"),
            Scope = GetString(root, "scope"),
            ExpiresUtc = DateTimeOffset.UtcNow
                .AddSeconds(Math.Max(60, expiresIn - 60))
        };
    }

    private static async Task<HttpListenerContext> WaitForCallbackAsync(
        HttpListener listener,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        Task<HttpListenerContext> callbackTask =
            listener.GetContextAsync();
        Task delayTask = Task.Delay(
            timeout,
            cancellationToken);

        Task completed = await Task.WhenAny(
            callbackTask,
            delayTask);

        if (completed != callbackTask)
        {
            cancellationToken.ThrowIfCancellationRequested();
            throw new TimeoutException(
                "Microsoft sign-in timed out before the browser returned.");
        }

        return await callbackTask;
    }

    private static async Task WriteBrowserResponseAsync(
        HttpListenerResponse response,
        bool succeeded)
    {
        string title = succeeded
            ? "LifeOS Microsoft connection complete"
            : "LifeOS Microsoft connection stopped";
        string body = succeeded
            ? "You can close this browser tab and return to LifeOS."
            : "Return to LifeOS to review the connection error.";

        string html =
            "<!doctype html><html><head><meta charset=\"utf-8\">" +
            $"<title>{title}</title></head><body style=\"" +
            "font-family:Segoe UI,Arial;padding:32px;background:#11131d;" +
            "color:#f2f2f7\"><h1>" +
            WebUtility.HtmlEncode(title) +
            "</h1><p>" +
            WebUtility.HtmlEncode(body) +
            "</p></body></html>";

        byte[] bytes = Encoding.UTF8.GetBytes(html);
        response.StatusCode = 200;
        response.ContentType = "text/html; charset=utf-8";
        response.ContentLength64 = bytes.Length;
        await response.OutputStream.WriteAsync(bytes);
        response.Close();
    }

    private static string GetString(
        JsonElement element,
        string propertyName) =>
        element.TryGetProperty(
            propertyName,
            out JsonElement value) &&
        value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;

    private static string Sanitize(string value)
    {
        string trimmed = value.Trim();
        return trimmed.Length <= 240
            ? trimmed
            : trimmed[..240];
    }
}
