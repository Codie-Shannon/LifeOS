namespace LifeOS.Companion.Core.Models;

public enum ConnectionState
{
    NotPaired = 0,
    PairingRequested = 1,
    Paired = 2,
    Offline = 3,
    Available = 4,
    Error = 5,
    Revoked = 6
}
