using System.Security.Cryptography;
using System.Text;

namespace LifeOS.Core.MicrosoftProvider;

public sealed record MicrosoftPkcePair(
    string Verifier,
    string Challenge);

public static class MicrosoftOAuthProtocol
{
    public static MicrosoftPkcePair CreatePkce()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(64);
        string verifier = Base64Url(bytes);
        byte[] challengeBytes = SHA256.HashData(
            Encoding.ASCII.GetBytes(verifier));
        return new MicrosoftPkcePair(
            verifier,
            Base64Url(challengeBytes));
    }

    public static string CreateState() =>
        Base64Url(RandomNumberGenerator.GetBytes(32));

    public static Uri BuildAuthorizationUri(
        MicrosoftProviderConfiguration configuration,
        MicrosoftPkcePair pkce,
        string state)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(pkce);

        configuration.Normalize();

        Dictionary<string, string> query = new()
        {
            ["client_id"] = configuration.ClientId,
            ["response_type"] = "code",
            ["redirect_uri"] = configuration.RedirectUri,
            ["response_mode"] = "query",
            ["scope"] = string.Join(
                " ",
                MicrosoftProviderConfiguration.Group48RequestedScopes),
            ["code_challenge"] = pkce.Challenge,
            ["code_challenge_method"] = "S256",
            ["state"] = state,
            ["prompt"] = "select_account"
        };

        string encoded = string.Join(
            "&",
            query.Select(pair =>
                $"{Uri.EscapeDataString(pair.Key)}=" +
                $"{Uri.EscapeDataString(pair.Value)}"));

        return new Uri(
            $"https://login.microsoftonline.com/" +
            $"{Uri.EscapeDataString(configuration.Tenant)}/" +
            $"oauth2/v2.0/authorize?{encoded}");
    }

    public static Uri BuildTokenUri(
        MicrosoftProviderConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        configuration.Normalize();

        return new Uri(
            $"https://login.microsoftonline.com/" +
            $"{Uri.EscapeDataString(configuration.Tenant)}/" +
            "oauth2/v2.0/token");
    }

    private static string Base64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
