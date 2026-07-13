using System.Text.Json.Serialization;
namespace LifeOS.Companion.Core.Models;
public enum GlanceKind { Agenda, WaitingOnWork }
public sealed record GlanceItem(string Id,string Heading,string? Detail,DateTimeOffset? DueAtUtc,string Source);
public sealed record GlanceSnapshot(GlanceKind Kind,string Source,DateTimeOffset UpdatedAtUtc,DateTimeOffset StaleAfterUtc,IReadOnlyList<GlanceItem> Items)
{
    public bool IsStale(DateTimeOffset nowUtc)=>nowUtc>=StaleAfterUtc;
}
public enum NotificationCategory { ReviewAvailable, TransferFailed, PairingRevoked, GlanceUpdated }
public sealed record NotificationPreferences(bool Enabled,bool PrivacySafeLockScreen);
public sealed record LocalDataSummary(int Captures,int Pending,int Failed,int Delivered,bool IsPaired,int CachedGlanceSnapshots,int Conflicts);
public enum ConflictResolutionChoice { KeepPhoneDraft,KeepDesktopVersion,CreateSeparateReviewItem,Cancel }
public sealed record CaptureConflict(string ConflictId,string CaptureId,string PhoneContentHash,string DesktopContentHash,DateTimeOffset DetectedAtUtc,bool Resolved,ConflictResolutionChoice? Resolution,DateTimeOffset? ResolvedAtUtc);
public sealed record ConflictAudit(string AuditId,string ConflictId,ConflictResolutionChoice Choice,DateTimeOffset AtUtc,string Detail);
public sealed record BetaReadiness(string Version,string DeviceLabel,string PairingState,DateTimeOffset? LastAcknowledgementUtc,int Pending,int Failed,int Delivered,DateTimeOffset? LastGlanceUpdateUtc,bool GlanceStale,bool NotificationsEnabled,bool PrivacySafeNotifications,int Conflicts,int SchemaVersion,string StorageState,bool HasCloudAccount,bool HasBackgroundSync);
