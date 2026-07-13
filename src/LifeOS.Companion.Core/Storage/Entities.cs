using SQLite;

namespace LifeOS.Companion.Core.Storage;

[Table("schema_revisions")]
internal sealed class SchemaRevisionEntity
{
    [PrimaryKey] public int Revision { get; set; }
    public string AppliedAtUtc { get; set; } = string.Empty;
}

[Table("app_state")]
internal sealed class AppStateEntity
{
    [PrimaryKey] public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

[Table("captures")]
internal sealed class CaptureEntity
{
    [PrimaryKey] public string CaptureId { get; set; } = string.Empty;
    public byte[] TitleProtected { get; set; } = Array.Empty<byte>();
    public byte[] BodyProtected { get; set; } = Array.Empty<byte>();
    public byte[] CategoryProtected { get; set; } = Array.Empty<byte>();
    public string CreatedAtUtc { get; set; } = string.Empty;
    public string UpdatedAtUtc { get; set; } = string.Empty;
    public int DeliveryState { get; set; }
    public int SchemaVersion { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string ContentHash { get; set; } = string.Empty;
}

[Table("outbox")]
internal sealed class OutboxEntity
{
    [PrimaryKey] public string CaptureId { get; set; } = string.Empty;
    public long QueueOrdinal { get; set; }
    public int DeliveryState { get; set; }
    public string EnqueuedAtUtc { get; set; } = string.Empty;
}
