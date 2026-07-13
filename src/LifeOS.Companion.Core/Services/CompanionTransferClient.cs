using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using LifeOS.Companion.Core.Models;
using LifeOS.Companion.Core.Pairing;

namespace LifeOS.Companion.Core.Services;

public sealed class CompanionTransferClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    public CompanionTransferClient(HttpClient? http = null)
    {
        _http = http ?? new HttpClient { Timeout = TimeSpan.FromSeconds(12) };
    }

    public async Task<PairChallenge> BeginPairingAsync(string endpoint, string code, DeviceProfile device, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync(Normalize(endpoint, "/pair/request"),
            new PairRequest(CompanionProtocol.Version, code.Trim(), device.DeviceId, device.DeviceLabel), Json, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PairChallenge>(Json, ct))!;
    }

    public async Task<PairConfirmed> ConfirmPairingAsync(string endpoint, PairChallenge challenge, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync(Normalize(endpoint, "/pair/confirm"),
            new PairConfirm(CompanionProtocol.Version, challenge.PairingId, challenge.Verification), Json, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PairConfirmed>(Json, ct))!;
    }

    public async Task<TransferAcknowledgement> SendAsync(PairingCredential credential, QuickCapture capture, CancellationToken ct = default)
    {
        var transferId = Guid.NewGuid().ToString("N");
        var payload = new CaptureTransfer(CompanionProtocol.Version, credential.PairingId, capture.DeviceId,
            transferId, capture.CaptureId, capture.SchemaVersion, capture.Title, capture.Body, capture.Category,
            capture.CreatedAtUtc, capture.UpdatedAtUtc, capture.ContentHash, $"capture:{capture.CaptureId}:{capture.ContentHash}");
        var body = JsonSerializer.Serialize(payload, Json);
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var nonce = Guid.NewGuid().ToString("N");
        using var request = new HttpRequestMessage(HttpMethod.Post, Normalize(credential.Endpoint, "/transfer"));
        request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        request.Headers.Add("X-LifeOS-Timestamp", timestamp);
        request.Headers.Add("X-LifeOS-Nonce", nonce);
        request.Headers.Add("X-LifeOS-Signature", CompanionAuthenticator.CreateSignature(credential.SecretBase64, timestamp, nonce, body));
        var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TransferAcknowledgement>(Json, ct))!;
    }

    private static string Normalize(string endpoint, string path) => endpoint.TrimEnd('/') + path;
}
