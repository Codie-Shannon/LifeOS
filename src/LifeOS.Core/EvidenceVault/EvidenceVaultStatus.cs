namespace LifeOS.Core.EvidenceVault;

public enum EvidenceVaultStatus
{
    Captured = 0,
    NeedsReview = 10,
    Reviewed = 20,
    Linked = 30,
    Exported = 40,
    Archived = 999
}
