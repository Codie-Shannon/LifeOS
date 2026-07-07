namespace LifeOS.Core.EvidenceVault;

public sealed class EvidenceVaultItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public EvidenceVaultItemType Type { get; set; } = EvidenceVaultItemType.Screenshot;

    public EvidenceVaultStatus Status { get; set; } = EvidenceVaultStatus.Captured;

    public DateOnly EvidenceDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public string ProjectOrClient { get; set; } = string.Empty;

    public string WorkPipelineItemId { get; set; } = string.Empty;

    public string SourcePathOrReference { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public bool NeedsReview { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public bool IsOpen => Status is not EvidenceVaultStatus.Archived and not EvidenceVaultStatus.Exported;
}
