namespace LifeOS.Companion.Core.Models;

public sealed record QuickCapture
{
    public required string CaptureId { get; init; }
    public string? Title { get; init; }
    public required string Body { get; init; }
    public string? Category { get; init; }
    public required DateTimeOffset CreatedAtUtc { get; init; }
    public required DateTimeOffset UpdatedAtUtc { get; init; }
    public required DeliveryState DeliveryState { get; init; }
    public required int SchemaVersion { get; init; }
    public required string DeviceId { get; init; }
    public required string ContentHash { get; init; }
}
