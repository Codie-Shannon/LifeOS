using LifeOS.Companion.Core.Models;
namespace LifeOS.Companion.Core.Storage;
public interface ICompanionStore
{
 Task InitializeAsync(CancellationToken cancellationToken=default);
 Task<DeviceProfile> GetOrCreateDeviceAsync(string defaultLabel,CancellationToken cancellationToken=default);
 Task UpdateDeviceLabelAsync(string deviceLabel,CancellationToken cancellationToken=default);
 Task<IReadOnlyList<QuickCapture>> GetCapturesAsync(CancellationToken cancellationToken=default);
 Task<IReadOnlyList<QuickCapture>> GetOutboxAsync(CancellationToken cancellationToken=default);
 Task SavePendingCaptureAsync(QuickCapture capture,CancellationToken cancellationToken=default);
 Task UpdateDeliveryStateAsync(string captureId,DeliveryState state,CancellationToken cancellationToken=default);
 Task NormalizeStaleSendingAsync(CancellationToken cancellationToken=default);
 Task DeleteDraftAsync(string captureId,CancellationToken cancellationToken=default);
 Task SaveGlanceAsync(GlanceSnapshot snapshot,CancellationToken cancellationToken=default);
 Task<IReadOnlyList<GlanceSnapshot>> GetGlanceAsync(CancellationToken cancellationToken=default);
 Task ClearGlanceCacheAsync(CancellationToken cancellationToken=default);
 Task SaveConflictAsync(CaptureConflict conflict,CancellationToken cancellationToken=default);
 Task<IReadOnlyList<CaptureConflict>> GetConflictsAsync(CancellationToken cancellationToken=default);
 Task ResolveConflictAsync(string conflictId,ConflictResolutionChoice choice,CancellationToken cancellationToken=default);
 Task<LocalDataSummary> GetLocalDataSummaryAsync(bool paired,CancellationToken cancellationToken=default);
 Task SetStateValueAsync(string key,string value,CancellationToken cancellationToken=default);
 Task<string?> GetStateValueAsync(string key,CancellationToken cancellationToken=default);
}
