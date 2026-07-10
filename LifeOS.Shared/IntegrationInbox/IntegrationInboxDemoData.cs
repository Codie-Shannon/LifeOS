using LifeOS.Core.IntegrationInbox;

namespace LifeOS.Shared.IntegrationInbox;

public static class IntegrationInboxDemoData
{
    public static List<IntegrationPreviewItem> Create()
    {
        return
        [
            new()
            {
                SourceKind = IntegrationSourceKind.Email,
                SourceLabel = "Demo Mailbox",
                ExternalReference = "mail-demo-001",
                Title = "Client reply may unblock portal work",
                Summary = "A fictional reply mentions access confirmation and a possible next step.",
                OccurredAt = DateTime.Today.AddHours(9),
                Status = IntegrationPreviewStatus.NeedsReview,
                TrustState = IntegrationTrustState.SourceBacked,
                SuggestedTarget = IntegrationTargetKind.WorkPipeline,
                SuggestedAction = "Review the thread, confirm the match, then link it to the blocked work item.",
                SourceEvidence = "Local demo email metadata and body preview.",
                DuplicateKey = "portal-access-demo",
                ReviewNote = "Do not change waiting state until manually confirmed."
            },
            new()
            {
                SourceKind = IntegrationSourceKind.Calendar,
                SourceLabel = "Demo Calendar",
                ExternalReference = "calendar-demo-001",
                Title = "Fixed caregiving window",
                Summary = "A fictional calendar event should protect a fixed time window.",
                OccurredAt = DateTime.Today.AddDays(1).AddHours(15),
                Status = IntegrationPreviewStatus.New,
                TrustState = IntegrationTrustState.SourceBacked,
                SuggestedTarget = IntegrationTargetKind.PaymentCalendar,
                SuggestedAction = "Confirm the date and protect the time window.",
                SourceEvidence = "Local demo calendar event preview.",
                DuplicateKey = "caregiving-window-demo"
            },
            new()
            {
                SourceKind = IntegrationSourceKind.Accounting,
                SourceLabel = "Demo Accounting",
                ExternalReference = "invoice-demo-001",
                Title = "Invoice payment status candidate",
                Summary = "A fictional invoice preview suggests payment may be expected but is not cleared.",
                Amount = 420m,
                OccurredAt = DateTime.Today.AddDays(3),
                Status = IntegrationPreviewStatus.DuplicateSuspected,
                TrustState = IntegrationTrustState.Untrusted,
                SuggestedTarget = IntegrationTargetKind.BillsPayments,
                SuggestedAction = "Compare invoice number, amount, and client before accepting.",
                SourceEvidence = "Local demo accounting preview only.",
                DuplicateKey = "invoice-420-demo",
                ReviewNote = "Possible duplicate of an existing expected-income item."
            },
            new()
            {
                SourceKind = IntegrationSourceKind.CloudFile,
                SourceLabel = "Demo Drive",
                ExternalReference = "file-demo-001",
                Title = "Proof screenshot candidate",
                Summary = "A fictional file preview may belong to a proof package.",
                Status = IntegrationPreviewStatus.Deferred,
                TrustState = IntegrationTrustState.SourceBacked,
                SuggestedTarget = IntegrationTargetKind.EvidenceVault,
                SuggestedAction = "Open the source file later and confirm the correct project link.",
                SourceEvidence = "Local demo file metadata.",
                DuplicateKey = "proof-shot-demo"
            },
            new()
            {
                SourceKind = IntegrationSourceKind.ReceiptOcr,
                SourceLabel = "Demo OCR",
                ExternalReference = "ocr-demo-001",
                Title = "Fuel receipt preview",
                Summary = "OCR candidate fields must be compared to the source image.",
                Amount = 42.70m,
                OccurredAt = DateTime.Today,
                Status = IntegrationPreviewStatus.NeedsReview,
                TrustState = IntegrationTrustState.Untrusted,
                SuggestedTarget = IntegrationTargetKind.EvidenceVault,
                SuggestedAction = "Compare merchant, amount, and date against the receipt image.",
                SourceEvidence = "Local demo OCR output.",
                DuplicateKey = "fuel-4270-demo"
            },
            new()
            {
                SourceKind = IntegrationSourceKind.BankingPreview,
                SourceLabel = "Future Bank Preview",
                ExternalReference = "bank-demo-001",
                Title = "Bank transaction preview disabled",
                Summary = "A future bank record is represented only as a local readiness example.",
                Amount = 62m,
                Status = IntegrationPreviewStatus.Rejected,
                TrustState = IntegrationTrustState.Untrusted,
                SuggestedTarget = IntegrationTargetKind.BillsPayments,
                SuggestedAction = "No action. Real bank integration is not active.",
                SourceEvidence = "Fictional readiness record.",
                DuplicateKey = "bank-disabled-demo",
                ReviewNote = "Rejected because no real bank connector is active."
            },
            new()
            {
                SourceKind = IntegrationSourceKind.ManualImport,
                SourceLabel = "Manual CSV Preview",
                ExternalReference = "manual-demo-001",
                Title = "Follow-up candidate",
                Summary = "A manually imported row may represent a follow-up.",
                Status = IntegrationPreviewStatus.Accepted,
                TrustState = IntegrationTrustState.Reviewed,
                SuggestedTarget = IntegrationTargetKind.FollowUps,
                SuggestedAction = "Create or link a follow-up only after explicit handoff.",
                SourceEvidence = "Local demo CSV row and review note.",
                DuplicateKey = "followup-demo",
                ReviewNote = "Accepted preview; handoff remains manual."
            }
        ];
    }
}
