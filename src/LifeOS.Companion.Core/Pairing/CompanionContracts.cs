using System.Text.Json.Serialization;

namespace LifeOS.Companion.Core.Pairing;

public static class CompanionProtocol
{
    public const string Version = "1";
    public const int Port = 43133;
}

public sealed record PairRequest(string ProtocolVersion, string Code, string DeviceId, string DeviceLabel);
public sealed record PairChallenge(string ProtocolVersion, string PairingId, string DesktopLabel, string Verification, DateTimeOffset ExpiresAtUtc);
public sealed record PairConfirm(string ProtocolVersion, string PairingId, string Verification);
public sealed record PairConfirmed(string ProtocolVersion, string PairingId, string DesktopLabel, string SecretBase64, DateTimeOffset ConfirmedAtUtc);
public sealed record PairingCredential(string PairingId, string DesktopLabel, string Endpoint, string SecretBase64, DateTimeOffset CreatedAtUtc);

public sealed record CaptureTransfer(
    string ProtocolVersion,
    string PairingId,
    string DeviceId,
    string TransferId,
    string CaptureId,
    int SchemaVersion,
    string? Title,
    string Body,
    string? Category,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    string ContentHash,
    string IdempotencyKey);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransferResult { AcceptedForReview, Duplicate, RejectedByPolicy, Invalid }

public sealed record TransferAcknowledgement(
    string ProtocolVersion,
    string TransferId,
    string CaptureId,
    string DesktopIntakeId,
    DateTimeOffset PersistedAtUtc,
    TransferResult Result);
