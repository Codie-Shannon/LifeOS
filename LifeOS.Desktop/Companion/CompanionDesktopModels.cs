using System.Text.Json.Serialization;
namespace LifeOS.Desktop.Companion;
public sealed record DesktopPairing(string PairingId,string DeviceId,string DeviceLabel,string SecretBase64,DateTimeOffset CreatedAtUtc,DateTimeOffset LastSeenUtc,bool Revoked);
[JsonConverter(typeof(JsonStringEnumConverter))] public enum IntakeReviewState { NeedsReview, Confirmed, Rejected }
public sealed record MobileCompanionIntake(string IntakeId,string CaptureId,string DeviceLabel,string TransferId,string IdempotencyKey,string ContentHash,string? Title,string Body,string? Category,DateTimeOffset CreatedAtUtc,DateTimeOffset ReceivedAtUtc,IntakeReviewState ReviewState,string Provenance);
