namespace LifeOS.Core.IntegrationInbox;

public sealed class IntegrationPreviewDraft
{
    public IntegrationSourceKind SourceKind { get; init; }
    public string SourceLabel { get; init; } = string.Empty;
    public string ExternalReference { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public DateTime? OccurredAt { get; init; }
    public decimal? Amount { get; init; }
    public string Currency { get; init; } = "NZD";
    public IntegrationTargetKind SuggestedTarget { get; init; } = IntegrationTargetKind.None;
    public string SuggestedAction { get; init; } = string.Empty;
    public string SourceEvidence { get; init; } = string.Empty;
    public string DuplicateKey { get; init; } = string.Empty;
}
