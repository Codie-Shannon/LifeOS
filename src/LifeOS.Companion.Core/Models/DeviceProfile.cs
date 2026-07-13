namespace LifeOS.Companion.Core.Models;

public sealed record DeviceProfile(
    string DeviceId,
    string DeviceLabel,
    ConnectionState ConnectionState,
    bool FirstRunComplete,
    int SchemaVersion);
