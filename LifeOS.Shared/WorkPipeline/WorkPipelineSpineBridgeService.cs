using LifeOS.Core.WorkPipeline;

namespace LifeOS.Shared.WorkPipeline;

public static class WorkPipelineSpineBridgeService
{
    public static WorkPipelineOperatingSnapshot Create(DateOnly today)
    {
        var items = WorkPipelineStorage.Load();
        var summary = WorkPipelineOperatingCalculator.Calculate(items, today);

        return new WorkPipelineOperatingSnapshot
        {
            Date = today,
            StorageFilePath = WorkPipelineStorage.FilePath,
            Items = items,
            Summary = summary,
            SpineBridgeNotes = BuildSpineBridgeNotes(summary).ToList(),
            PaymentCalendarBridgeNotes = BuildPaymentCalendarBridgeNotes(summary).ToList(),
            ProofBridgeNotes = BuildProofBridgeNotes(summary).ToList()
        };
    }

    public static IReadOnlyList<string> BuildSpineBridgeNotes(WorkPipelineOperatingSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);

        return BuildSpineBridgeNotesIterator(summary).ToList();
    }

    public static IReadOnlyList<string> BuildPaymentCalendarBridgeNotes(WorkPipelineOperatingSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);

        return BuildPaymentCalendarBridgeNotesIterator(summary).ToList();
    }

    public static IReadOnlyList<string> BuildProofBridgeNotes(WorkPipelineOperatingSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);

        return BuildProofBridgeNotesIterator(summary).ToList();
    }

    private static IEnumerable<string> BuildSpineBridgeNotesIterator(WorkPipelineOperatingSummary summary)
    {
        yield return "Work Pipeline is the v4.5 operating lane for active work, warm leads, blocked work, invoice readiness, payment expected state, and proof gaps.";
        yield return "Each work item should map to the item/state spine before integrations pull in email, calendar, accounting, or document data.";
        yield return $"{summary.OpenItems} open pipeline item(s) currently feed Command Centre pressure.";
        yield return $"{summary.ReviewNeededItems} pipeline item(s) need date/proof/payment/next-action review.";

        if (summary.BlockedItems > 0)
        {
            yield return $"{summary.BlockedItems} blocked item(s) should stay visible but should not consume deep sprint time until unblocked.";
        }

        if (summary.WaitingOnMeItems > 0 || summary.WaitingOnOthersItems > 0)
        {
            yield return $"Waiting split: {summary.WaitingOnMeItems} waiting on me, {summary.WaitingOnOthersItems} waiting on others.";
        }
    }

    private static IEnumerable<string> BuildPaymentCalendarBridgeNotesIterator(WorkPipelineOperatingSummary summary)
    {
        yield return "Follow-up dates, keep-warm dates, payment expected dates, and invoice dates should land in the v4.4 Payment Calendar lane.";
        yield return "Dates create visibility only; they do not mark work complete, invoices sent, or payments paid.";
        yield return $"{summary.FollowUpsNow + summary.FollowUpsDueSoon} follow-up date(s) should be visible in the date lane.";
        yield return $"{summary.PaymentExpectedItems} payment expected item(s) should remain excluded from safe money until paid.";
    }

    private static IEnumerable<string> BuildProofBridgeNotesIterator(WorkPipelineOperatingSummary summary)
    {
        yield return "Invoice readiness should be linked to proof/evidence before a client-safe summary or invoice is treated as clean.";
        yield return $"{summary.InvoiceReadyItems + summary.TimesheetReadyItems} item(s) need invoice/timesheet handling.";
        yield return $"{summary.ProofNeededItems} item(s) need proof/evidence notes before clean handoff.";
        yield return "v4.5 does not auto-create invoices, timesheets, emails, proof files, or calendar events.";
    }
}
