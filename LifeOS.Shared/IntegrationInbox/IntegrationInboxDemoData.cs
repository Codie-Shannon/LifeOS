using LifeOS.Core.IntegrationInbox;

namespace LifeOS.Shared.IntegrationInbox;

public static class IntegrationInboxDemoData
{
    public static List<IntegrationPreviewItem> Create()
    {
        return
        [
            DemoPreview(
                IntegrationSourceKind.Email,
                "Demo Mailbox",
                "mail-demo-001",
                "Client reply may unblock portal work",
                "A fictional reply mentions access confirmation and a possible next step.",
                IntegrationTargetKind.WorkPipeline,
                "Review the thread, confirm the match, then link it to the blocked work item.",
                "Local demo email metadata and body preview.",
                "portal-access-demo",
                IntegrationPreviewStatus.NeedsReview,
                IntegrationTrustState.SourceBacked,
                DateTime.Today.AddHours(9),
                reviewNote: "Do not change waiting state until manually confirmed."),

            DemoPreview(
                IntegrationSourceKind.Calendar,
                "Demo Calendar",
                "calendar-demo-001",
                "Fixed caregiving window",
                "A fictional calendar event should protect a fixed time window.",
                IntegrationTargetKind.PaymentCalendar,
                "Confirm the date and protect the time window.",
                "Local demo calendar event preview.",
                "caregiving-window-demo",
                IntegrationPreviewStatus.New,
                IntegrationTrustState.SourceBacked,
                DateTime.Today.AddDays(1).AddHours(15)),

            DemoPreview(
                IntegrationSourceKind.Accounting,
                "Demo Accounting",
                "invoice-demo-001",
                "Invoice payment status candidate",
                "A fictional invoice preview suggests payment may be expected but is not cleared.",
                IntegrationTargetKind.BillsPayments,
                "Compare invoice number, amount, and client before accepting.",
                "Local demo accounting preview only.",
                "invoice-420-demo",
                IntegrationPreviewStatus.DuplicateSuspected,
                IntegrationTrustState.Untrusted,
                DateTime.Today.AddDays(3),
                420m,
                "Possible duplicate of an existing expected-income item."),

            DemoPreview(
                IntegrationSourceKind.CloudFile,
                "Demo Drive",
                "file-demo-001",
                "Proof screenshot candidate",
                "A fictional file preview may belong to a proof package.",
                IntegrationTargetKind.EvidenceVault,
                "Open the source file later and confirm the correct project link.",
                "Local demo file metadata.",
                "proof-shot-demo",
                IntegrationPreviewStatus.Deferred,
                IntegrationTrustState.SourceBacked),

            DemoPreview(
                IntegrationSourceKind.ReceiptOcr,
                "Demo OCR",
                "ocr-demo-001",
                "Fuel receipt preview",
                "OCR candidate fields must be compared to the source image.",
                IntegrationTargetKind.EvidenceVault,
                "Compare merchant, amount, and date against the receipt image.",
                "Local demo OCR output.",
                "fuel-4270-demo",
                IntegrationPreviewStatus.NeedsReview,
                IntegrationTrustState.Untrusted,
                DateTime.Today,
                42.70m),

            DemoPreview(
                IntegrationSourceKind.BankingPreview,
                "Future Bank Preview",
                "bank-demo-001",
                "Bank transaction preview disabled",
                "A future bank record is represented only as a local readiness example.",
                IntegrationTargetKind.BillsPayments,
                "No action. Real bank integration is not active.",
                "Fictional readiness record.",
                "bank-disabled-demo",
                IntegrationPreviewStatus.Rejected,
                IntegrationTrustState.Untrusted,
                amount: 62m,
                reviewNote: "Rejected because no real bank connector is active."),

            DemoPreview(
                IntegrationSourceKind.ManualImport,
                "Manual CSV Preview",
                "manual-demo-001",
                "Follow-up candidate",
                "A manually imported row may represent a follow-up.",
                IntegrationTargetKind.FollowUps,
                "Create or link a follow-up only after explicit handoff.",
                "Local demo CSV row and review note.",
                "followup-demo",
                IntegrationPreviewStatus.Accepted,
                IntegrationTrustState.Reviewed,
                reviewNote: "Accepted preview; handoff remains manual.")
        ];
    }

    private static IntegrationPreviewItem DemoPreview(
        IntegrationSourceKind sourceKind,
        string sourceLabel,
        string externalReference,
        string title,
        string summary,
        IntegrationTargetKind suggestedTarget,
        string suggestedAction,
        string sourceEvidence,
        string duplicateKey,
        IntegrationPreviewStatus status,
        IntegrationTrustState trustState,
        DateTime? occurredAt = null,
        decimal? amount = null,
        string reviewNote = "")
    {
        var preview = IntegrationPreviewIntake.CreatePreview(new IntegrationPreviewDraft
        {
            SourceKind = sourceKind,
            SourceLabel = sourceLabel,
            ExternalReference = externalReference,
            Title = title,
            Summary = summary,
            OccurredAt = occurredAt,
            Amount = amount,
            SuggestedTarget = suggestedTarget,
            SuggestedAction = suggestedAction,
            SourceEvidence = sourceEvidence,
            DuplicateKey = duplicateKey
        });

        preview.Status = status;
        preview.TrustState = trustState;
        preview.ReviewNote = reviewNote;

        return preview;
    }
}
