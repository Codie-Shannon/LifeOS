using LifeOS.Mobile.Core.Foundation;

namespace LifeOS.Mobile.Core.Storage;

public interface IMobileLocalStore
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task<MobilePreferences> LoadPreferencesAsync(CancellationToken cancellationToken = default);
    Task SavePreferencesAsync(MobilePreferences preferences, CancellationToken cancellationToken = default);
    Task<MobileSessionState> LoadSessionAsync(CancellationToken cancellationToken = default);
    Task SaveSessionAsync(MobileSessionState state, CancellationToken cancellationToken = default);
    Task<MobileSyncSnapshot> LoadSyncAsync(CancellationToken cancellationToken = default);
    Task SaveSyncAsync(MobileSyncSnapshot state, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MobileOutboxItem>> LoadOutboxAsync(CancellationToken cancellationToken = default);
    Task QueueAsync(MobileOutboxItem item, CancellationToken cancellationToken = default);
    Task ClearDeviceDataAsync(CancellationToken cancellationToken = default);
}
