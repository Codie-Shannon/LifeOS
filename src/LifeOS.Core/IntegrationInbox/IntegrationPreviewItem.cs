namespace LifeOS.Core.IntegrationInbox;

public sealed class IntegrationPreviewItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public IntegrationSourceKind SourceKind { get; set; }
    public string SourceLabel { get; set; } = "";
    public string ConnectorKey { get; set; } = "";
    public string ProviderAccountLabel { get; set; } = "";
    public string ProviderContainerId { get; set; } = "";
    public string ExternalReference { get; set; } = "";
    public string Title { get; set; } = "";
    public string Summary { get; set; } = "";
    public DateTime? OccurredAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public DateTime? FetchedAt { get; set; }
    public decimal? Amount { get; set; }
    public string Currency { get; set; } = "NZD";
    public IntegrationPreviewStatus Status { get; set; } = IntegrationPreviewStatus.New;
    public IntegrationTrustState TrustState { get; set; } = IntegrationTrustState.Untrusted;
    public IntegrationTargetKind SuggestedTarget { get; set; }
    public string SuggestedAction { get; set; } = "";
    public string SourceEvidence { get; set; } = "";
    public string DuplicateKey { get; set; } = "";
    public string ReviewNote { get; set; } = "";
    public string LinkReference { get; set; } = "";
    public bool IsReadOnlyPreview { get; set; } = true;
    public bool RequiresHumanReview { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
