namespace LifeOS.Core.EvidenceVault;

public sealed class EvidenceVaultLink
{
    public Guid EvidenceId { get; init; }

    public string TargetType { get; init; } = string.Empty;

    public string TargetId { get; init; } = string.Empty;

    public string Relationship { get; init; } = string.Empty;
}
