using System.Security.Cryptography;
using System.Text;
using LifeOS.Mobile.Core.Foundation;
using LifeOS.Mobile.Core.Storage;

namespace LifeOS.Mobile.Core.Services;

public sealed class MobileFoundationService
{
    private readonly IMobileLocalStore _store;
    public MobileFoundationService(IMobileLocalStore store) => _store = store;

    public async Task InitializeDemoAsync(CancellationToken cancellationToken = default)
    {
        await _store.InitializeAsync(cancellationToken);
        var outbox = await _store.LoadOutboxAsync(cancellationToken);
        if (outbox.Count == 0)
        {
            await _store.QueueAsync(new MobileOutboxItem(
                DeterministicId("group-52-demo-command"), "Local demo update", Sha256("fictional-demo-payload"),
                DateTimeOffset.UtcNow.AddMinutes(-2), "Pending"), cancellationToken);
        }
    }

    public async Task ActivateEmergencyStopAsync(CancellationToken cancellationToken = default)
    {
        var current = await _store.LoadSyncAsync(cancellationToken);
        await _store.SaveSyncAsync(current with { State = MobileSyncState.Stopped, SafeError = null }, cancellationToken);
    }

    public async Task ClearEmergencyStopAsync(CancellationToken cancellationToken = default)
    {
        var current = await _store.LoadSyncAsync(cancellationToken);
        await _store.SaveSyncAsync(current with { State = current.PendingCount > 0 ? MobileSyncState.Pending : MobileSyncState.Idle }, cancellationToken);
    }

    public async Task<IReadOnlyList<MobileDiagnosticItem>> GetSafeDiagnosticsAsync(CancellationToken cancellationToken = default)
    {
        var sync = await _store.LoadSyncAsync(cancellationToken);
        var session = await _store.LoadSessionAsync(cancellationToken);
        return [
            new("Release", "LifeOS v10 Full Mobile — Group 52"),
            new("Device", Redaction.Safe(session.DeviceLabel)),
            new("Local encryption", "AES-256-GCM"),
            new("Schema", "1"),
            new("Sync state", sync.State.ToString()),
            new("Pending queue", sync.PendingCount.ToString()),
            new("Provider writes", "Disabled"),
            new("Sensitive values", "Redacted")
        ];
    }

    public static string DeterministicId(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant()[..24];
    public static string Sha256(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
}
