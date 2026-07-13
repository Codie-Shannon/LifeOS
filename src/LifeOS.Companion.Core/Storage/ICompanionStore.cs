using LifeOS.Companion.Core.Models;

namespace LifeOS.Companion.Core.Storage;

public interface ICompanionStore
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task<DeviceProfile> GetOrCreateDeviceAsync(string defaultLabel, CancellationToken cancellationToken = default);
    Task UpdateDeviceLabelAsync(string deviceLabel, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<QuickCapture>> GetCapturesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<QuickCapture>> GetOutboxAsync(CancellationToken cancellationToken = default);
    Task SavePendingCaptureAsync(QuickCapture capture, CancellationToken cancellationToken = default);
    Task DeleteDraftAsync(string captureId, CancellationToken cancellationToken = default);
}
