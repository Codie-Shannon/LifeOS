namespace LifeOS.Core.IntegrationInbox;

public static class IntegrationPreviewIntake
{
    public static IntegrationPreviewItem CreatePreview(IntegrationPreviewDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);

        if (string.IsNullOrWhiteSpace(draft.SourceLabel))
        {
            throw new ArgumentException("Source label is required for every integration preview.", nameof(draft));
        }

        if (string.IsNullOrWhiteSpace(draft.ExternalReference))
        {
            throw new ArgumentException("External reference is required for every integration preview.", nameof(draft));
        }

        if (string.IsNullOrWhiteSpace(draft.Title))
        {
            throw new ArgumentException("Title is required for every integration preview.", nameof(draft));
        }

        var duplicateKey = string.IsNullOrWhiteSpace(draft.DuplicateKey)
            ? BuildDuplicateKey(draft)
            : draft.DuplicateKey.Trim();

        return new IntegrationPreviewItem
        {
            SourceKind = draft.SourceKind,
            SourceLabel = draft.SourceLabel.Trim(),
            ExternalReference = draft.ExternalReference.Trim(),
            Title = draft.Title.Trim(),
            Summary = draft.Summary.Trim(),
            OccurredAt = draft.OccurredAt,
            Amount = draft.Amount,
            Currency = string.IsNullOrWhiteSpace(draft.Currency) ? "NZD" : draft.Currency.Trim().ToUpperInvariant(),
            Status = IntegrationPreviewStatus.New,
            TrustState = IntegrationTrustState.Untrusted,
            SuggestedTarget = draft.SuggestedTarget,
            SuggestedAction = draft.SuggestedAction.Trim(),
            SourceEvidence = draft.SourceEvidence.Trim(),
            DuplicateKey = duplicateKey,
            IsReadOnlyPreview = true,
            RequiresHumanReview = true,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
    }

    private static string BuildDuplicateKey(IntegrationPreviewDraft draft)
    {
        var datePart = draft.OccurredAt?.ToString("yyyyMMdd") ?? "nodate";
        var amountPart = draft.Amount?.ToString("0.00") ?? "noamount";

        return string.Join(
            ":",
            draft.SourceKind.ToString().ToLowerInvariant(),
            draft.SourceLabel.Trim().ToLowerInvariant(),
            draft.ExternalReference.Trim().ToLowerInvariant(),
            datePart,
            amountPart);
    }
}
