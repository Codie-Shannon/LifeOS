namespace LifeOS.Shared.IntegrationConnectors.Gmail;
public sealed class GmailConnectorConfiguration
{
 public string ClientId {get;init;}=""; public string ClientSecret {get;init;}=""; public string RedirectUri {get;init;}="http://127.0.0.1:53683/";
 public void Validate(){if(string.IsNullOrWhiteSpace(ClientId)||ClientId.StartsWith("REPLACE_",StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Gmail client ID is not configured."); if(string.IsNullOrWhiteSpace(ClientSecret)||ClientSecret.StartsWith("REPLACE_",StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Gmail client secret is not configured."); if(!Uri.TryCreate(RedirectUri,UriKind.Absolute,out var u)||u.Scheme!=Uri.UriSchemeHttp||!u.IsLoopback) throw new InvalidOperationException("Gmail redirect URI must be a local HTTP loopback address.");}
}
