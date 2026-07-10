namespace LifeOS.Core.IntegrationInbox;

public static class IntegrationInboxCalculator
{
    public static IntegrationInboxSummary Calculate(IEnumerable<IntegrationPreviewItem> items)
    {
        var list = items.ToList();
        var review = list.Count(x => x.Status is IntegrationPreviewStatus.New
            or IntegrationPreviewStatus.NeedsReview
            or IntegrationPreviewStatus.DuplicateSuspected);

        return new IntegrationInboxSummary
        {
            Total = list.Count,
            NewItems = list.Count(x => x.Status == IntegrationPreviewStatus.New),
            NeedsReview = review,
            DuplicateSuspected = list.Count(x => x.Status == IntegrationPreviewStatus.DuplicateSuspected),
            Deferred = list.Count(x => x.Status == IntegrationPreviewStatus.Deferred),
            Accepted = list.Count(x => x.Status == IntegrationPreviewStatus.Accepted),
            Rejected = list.Count(x => x.Status == IntegrationPreviewStatus.Rejected),
            Linked = list.Count(x => x.Status == IntegrationPreviewStatus.Linked),
            Untrusted = list.Count(x => x.TrustState == IntegrationTrustState.Untrusted),
            SourceBacked = list.Count(x => x.TrustState is IntegrationTrustState.SourceBacked
                or IntegrationTrustState.Reviewed
                or IntegrationTrustState.Trusted),
            ReadyForHandoff = list.Count(x => x.Status == IntegrationPreviewStatus.Accepted
                && x.TrustState is IntegrationTrustState.Reviewed or IntegrationTrustState.Trusted),
            PreviewMoney = list.Where(x => x.Amount.HasValue).Sum(x => x.Amount!.Value),
            EmailPreviews = list.Count(x => x.SourceKind == IntegrationSourceKind.Email),
            CalendarPreviews = list.Count(x => x.SourceKind == IntegrationSourceKind.Calendar),
            AccountingPreviews = list.Count(x => x.SourceKind == IntegrationSourceKind.Accounting),
            FilePreviews = list.Count(x => x.SourceKind == IntegrationSourceKind.CloudFile),
            OcrPreviews = list.Count(x => x.SourceKind == IntegrationSourceKind.ReceiptOcr),
            BankingPreviews = list.Count(x => x.SourceKind == IntegrationSourceKind.BankingPreview),
            PressureLabel = review >= 4 ? "Critical" : review >= 2 ? "High" : review == 1 ? "Watch" : "Calm"
        };
    }
}
