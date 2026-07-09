using LifeOS.Core.Money;
using LifeOS.Core.ProofTracker;
using LifeOS.Core.WorkPipeline;
using LifeOS.Core.WorkSessions;

namespace LifeOS.Core.PaidWorkMoneyProof;

public static class PaidWorkMoneyProofCalculator
{
    public static PaidWorkMoneyProofSummary Calculate(
        IEnumerable<WorkSession> workSessions,
        IEnumerable<ProofItem> proofItems,
        MoneyPressureSummary money,
        WorkPipelineSummary pipeline,
        DateOnly today)
    {
        var sessions = workSessions.ToList();
        var proof = proofItems.ToList();

        var billableSessions = sessions
            .Where(session => session.IsBillable && session.Status is not WorkSessionStatus.Cancelled and not WorkSessionStatus.NonBillable)
            .ToList();

        var paidSessions = billableSessions
            .Where(session => session.Status == WorkSessionStatus.Paid)
            .ToList();

        var unpaidSessions = billableSessions
            .Where(session => session.Status is WorkSessionStatus.Planned or WorkSessionStatus.InProgress or WorkSessionStatus.Completed or WorkSessionStatus.Invoiced)
            .ToList();

        var invoiceReadySessions = billableSessions
            .Where(session => session.Status is WorkSessionStatus.Completed or WorkSessionStatus.Invoiced)
            .OrderBy(session => session.ClientOrProject)
            .ThenBy(session => session.Date)
            .ToList();

        var readyProof = proof
            .Where(item => item.Status == ProofStatus.Ready)
            .OrderByDescending(item => item.Date)
            .ToList();

        var clientProof = proof
            .Where(item => item.Type is ProofType.ClientReply or ProofType.PaidInvoice or ProofType.CaseStudy)
            .OrderByDescending(item => item.Date)
            .ToList();

        var totalBillableValue = billableSessions.Sum(session => session.BillableValue);
        var paidValue = paidSessions.Sum(session => session.BillableValue);
        var unpaidBillableValue = unpaidSessions.Sum(session => session.BillableValue);
        var invoiceReadyValue = invoiceReadySessions.Sum(session => session.BillableValue);
        var pendingPipelineValue = pipeline.ExpectedValueTotal;
        var pendingManualIncome = money.PendingIncome;
        var moneyAtRisk = unpaidBillableValue + pendingPipelineValue + pendingManualIncome;
        var adminActionCount = invoiceReadySessions.Count + readyProof.Count + pipeline.TimesheetsNeeded + pipeline.InvoicesNeeded + pipeline.PaymentsExpected;

        var focusItems = new List<PaidWorkMoneyProofFocusItem>();

        if (pipeline.TimesheetsNeeded > 0)
        {
            focusItems.Add(new PaidWorkMoneyProofFocusItem
            {
                Title = "Timesheets need evidence",
                Source = "Work Pipeline",
                Priority = "High",
                Value = pipeline.TimesheetsNeeded.ToString(),
                NextAction = "Convert billable work into client-safe timesheet descriptions before details fade."
            });
        }

        if (pipeline.InvoicesNeeded > 0)
        {
            focusItems.Add(new PaidWorkMoneyProofFocusItem
            {
                Title = "Invoices need preparation",
                Source = "Work Pipeline",
                Priority = "High",
                Value = pipeline.InvoicesNeeded.ToString(),
                NextAction = "Check proof and timesheet readiness before treating expected value as safe money."
            });
        }

        if (invoiceReadySessions.Count > 0)
        {
            focusItems.Add(new PaidWorkMoneyProofFocusItem
            {
                Title = "Invoice-ready work sessions",
                Source = "Work Sessions",
                Priority = "High",
                Value = FormatMoney(invoiceReadyValue),
                NextAction = "Review the copy-ready client update and decide what can be invoiced or followed up."
            });
        }

        if (readyProof.Count > 0)
        {
            focusItems.Add(new PaidWorkMoneyProofFocusItem
            {
                Title = "Proof ready to use",
                Source = "Proof Tracker",
                Priority = "Normal",
                Value = readyProof.Count.ToString(),
                NextAction = "Attach or reference the strongest proof before sending a client update or case-study note."
            });
        }

        if (pipeline.PaymentsExpected > 0)
        {
            focusItems.Add(new PaidWorkMoneyProofFocusItem
            {
                Title = "Expected payments are visible",
                Source = "Work Pipeline",
                Priority = "Normal",
                Value = pipeline.PaymentsExpected.ToString(),
                NextAction = "Keep expected payments separate from safe money until they are confirmed paid."
            });
        }

        if (pendingManualIncome > 0m)
        {
            focusItems.Add(new PaidWorkMoneyProofFocusItem
            {
                Title = "Pending manual income",
                Source = "Money Pressure",
                Priority = "Watch",
                Value = FormatMoney(pendingManualIncome),
                NextAction = "Do not spend this as safe money until it lands."
            });
        }

        var reasons = new List<string>();

        if (invoiceReadyValue > 0m) reasons.Add($"{FormatMoney(invoiceReadyValue)} of work is invoice-ready or invoice-review ready.");
        if (unpaidBillableValue > 0m) reasons.Add($"{FormatMoney(unpaidBillableValue)} of billable work is not marked paid yet.");
        if (pendingPipelineValue > 0m) reasons.Add($"{FormatMoney(pendingPipelineValue)} of pipeline value is expected but not safe money.");
        if (pendingManualIncome > 0m) reasons.Add($"{FormatMoney(pendingManualIncome)} of manual income is pending and must stay separate from safe-to-spend.");
        if (readyProof.Count > 0) reasons.Add($"{readyProof.Count} proof item(s) are ready to support a client update, case study, or timesheet trail.");
        if (clientProof.Count > 0) reasons.Add($"{clientProof.Count} proof item(s) are client/payment/case-study relevant.");
        if (adminActionCount == 0) reasons.Add("No paid-work admin action is urgent right now. Keep proof attached as work moves.");

        var health = GetHealth(moneyAtRisk, invoiceReadyValue, adminActionCount, readyProof.Count, pipeline.TimesheetsNeeded, pipeline.InvoicesNeeded);

        return new PaidWorkMoneyProofSummary
        {
            Health = health,
            TotalBillableValue = totalBillableValue,
            PaidValue = paidValue,
            UnpaidBillableValue = unpaidBillableValue,
            InvoiceReadyValue = invoiceReadyValue,
            PendingPipelineValue = pendingPipelineValue,
            PendingManualIncome = pendingManualIncome,
            MoneyAtRisk = moneyAtRisk,
            InvoiceReadySessionCount = invoiceReadySessions.Count,
            ReadyProofCount = readyProof.Count,
            ClientProofCount = clientProof.Count,
            TimesheetsNeeded = pipeline.TimesheetsNeeded,
            InvoicesNeeded = pipeline.InvoicesNeeded,
            PaymentsExpected = pipeline.PaymentsExpected,
            AdminActionCount = adminActionCount,
            FocusItems = focusItems,
            Reasons = reasons,
            CopyReadyClientUpdate = BuildCopyReadyClientUpdate(invoiceReadySessions, readyProof, money, today)
        };
    }

    private static PaidWorkMoneyProofHealth GetHealth(decimal moneyAtRisk, decimal invoiceReadyValue, int adminActionCount, int readyProofCount, int timesheetsNeeded, int invoicesNeeded)
    {
        if (timesheetsNeeded > 0 || invoicesNeeded > 0 || invoiceReadyValue >= 250m) return PaidWorkMoneyProofHealth.HighPressure;
        if (moneyAtRisk > 0m && adminActionCount > 0) return PaidWorkMoneyProofHealth.NeedsReview;
        if (readyProofCount > 0 || moneyAtRisk > 0m) return PaidWorkMoneyProofHealth.Watch;
        return PaidWorkMoneyProofHealth.Calm;
    }

    private static string BuildCopyReadyClientUpdate(IReadOnlyCollection<WorkSession> invoiceReadySessions, IReadOnlyCollection<ProofItem> readyProof, MoneyPressureSummary money, DateOnly today)
    {
        var lines = new List<string>
        {
            $"Paid work / proof check — {today:yyyy-MM-dd}",
            string.Empty
        };

        if (invoiceReadySessions.Count == 0)
        {
            lines.Add("No invoice-ready work sessions are currently marked completed or invoiced.");
        }
        else
        {
            lines.Add("Invoice-ready work:");

            foreach (var group in invoiceReadySessions.GroupBy(session => session.ClientOrProject).OrderBy(group => group.Key))
            {
                var totalHours = group.Sum(session => session.Hours);
                var totalValue = group.Sum(session => session.BillableValue);
                lines.Add($"- {SafeText(group.Key, "Unassigned work")}: {totalHours:0.##}h / {FormatMoney(totalValue)}");

                foreach (var session in group.OrderBy(session => session.Date))
                {
                    lines.Add($"  • {session.Date:yyyy-MM-dd}: {session.Hours:0.##}h — {session.Description}");
                }
            }
        }

        lines.Add(string.Empty);

        if (readyProof.Count == 0)
        {
            lines.Add("Proof: no proof items are currently marked Ready.");
        }
        else
        {
            lines.Add("Proof ready to reference:");
            foreach (var item in readyProof.Take(5))
            {
                lines.Add($"- {item.Date:yyyy-MM-dd}: {item.Title} ({item.Type})");
            }
        }

        lines.Add(string.Empty);
        lines.Add($"Money safety: pending income is {FormatMoney(money.PendingIncome)} and must not be treated as safe-to-spend until paid.");
        lines.Add("Boundary: this is a copy-ready admin summary, not accounting automation, tax advice, or automatic sending.");

        return string.Join(Environment.NewLine, lines);
    }

    private static string FormatMoney(decimal value) => value.ToString("C");

    private static string SafeText(string value, string fallback) => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
}
