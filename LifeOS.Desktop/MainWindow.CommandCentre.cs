using LifeOS.Core.Money;
using LifeOS.Shared.Money;
using LifeOS.Shared.Shell;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LifeOS.Core.FollowUps;
using LifeOS.Shared.FollowUps;
using LifeOS.Shared.CommandCentre;
using LifeOS.Core.WorkPipeline;
using LifeOS.Shared.WorkPipeline;
using LifeOS.Core.DailyState;
using LifeOS.Shared.DailyState;
using LifeOS.Core.DailyOperatingFlow;
using LifeOS.Shared.DailyOperatingFlow;
using LifeOS.Core.TimesheetEvidence;
using LifeOS.Shared.TimesheetEvidence;
using LifeOS.Core.EvidenceVault;
using LifeOS.Shared.EvidenceVault;
using LifeOS.Core.RelationshipRadar;
using LifeOS.Core.PaidWorkMoneyProof;
using LifeOS.Shared.RelationshipRadar;
using LifeOS.Shared.PaidWorkMoneyProof;
using LifeOS.Core.SettingsSafety;
using LifeOS.Shared.SettingsSafety;
using LifeOS.Core.DesktopRelease;
using LifeOS.Shared.DesktopRelease;
using LifeOS.Core.UniversalSpine;
using LifeOS.Shared.UniversalSpine;
using LifeOS.Core.OsNavigation;
using LifeOS.Shared.OsNavigation;
using LifeOS.Core.SearchKnowledge;
using LifeOS.Shared.SearchKnowledge;
using LifeOS.Core.FinalOfflineOs;
using LifeOS.Shared.FinalOfflineOs;
using LifeOS.Core.LifeOsSpine;
using LifeOS.Shared.LifeOsSpine;
using LifeOS.Core.ItemState;
using LifeOS.Shared.ItemState;
using LifeOS.Core.MoneyObligations;
using LifeOS.Shared.MoneyObligations;
using LifeOS.Core.MoneyProfile;
using LifeOS.Shared.MoneyProfile;
using LifeOS.Core.PaymentCalendar;
using LifeOS.Shared.PaymentCalendar;
using LifeOS.Core.ReceiptEvidence;
using LifeOS.Shared.ReceiptEvidence;
using LifeOS.Core.CommandCentrePressure;
using LifeOS.Shared.CommandCentrePressure;
using LifeOS.Core.IntegrationInbox;
using LifeOS.Shared.IntegrationInbox;

using LifeOS.Core.Agenda;
using LifeOS.Core.PayLater;
using LifeOS.Core.WeeklyCloseOut;
using LifeOS.Shared.Agenda;
using LifeOS.Shared.PayLater;
using LifeOS.Shared.WeeklyCloseOut;
using LifeOS.Core.WorkSessions;
namespace LifeOS.Desktop;

public partial class MainWindow
{
    private void ResetCommandCentrePressurePolicyButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmRiskyAction(
                "Reset v4.8 pressure policy?",
                "This restores the conservative local scoring and suppression defaults. It does not change source module data."))
        {
            return;
        }

        CommandCentrePressurePolicyStorage.Reset();
        _commandCentrePressurePolicy = CommandCentrePressurePolicyStorage.Load();
        ShowCommandCentre();
    }

    private List<PressureSignal> CreateCommandCentrePressureSignals(
        CommandCentreSummary summary,
        DailyOperatingFlowSummary dailyFlowSummary,
        MoneyObligationSummary moneyObligationSummary,
        MoneyProfileSummary moneyProfileSummary,
        PaymentCalendarSummary paymentCalendarSummary,
        WorkPipelineOperatingSummary workPipelineSummary,
        ReceiptEvidenceSummary receiptEvidenceSummary,
        WeeklyCloseOutOperatingSummary weeklyCloseOutSummary)
    {
        var signals = new List<PressureSignal>();

        signals.Add(new PressureSignal
        {
            Key = "safe-to-spend",
            Module = "Money Profile",
            Title = "Final safe-to-spend",
            Detail = $"Final safe-to-spend is {FormatMoney(moneyProfileSummary.SafeToSpendFinal)}.",
            NextAction = "Review visible obligations, hidden deductions, and required buffers before discretionary spending.",
            Severity = moneyProfileSummary.SafeToSpendFinal < 0 ? PressureSeverity.Critical : PressureSeverity.Normal,
            Lane = moneyProfileSummary.SafeToSpendFinal < 0 ? PressureLane.ActNow : PressureLane.Review,
            MoneyAmount = Math.Abs(Math.Min(0m, moneyProfileSummary.SafeToSpendFinal)),
            IsDueNow = moneyProfileSummary.SafeToSpendFinal < 0,
            IsTrusted = true
        });

        signals.Add(new PressureSignal
        {
            Key = "money-due-today",
            Module = "Bills / Payments",
            Title = "Money due today",
            Detail = $"{moneyObligationSummary.DueTodayItems} obligation(s) due today totalling {FormatMoney(moneyObligationSummary.AmountDueToday)}.",
            NextAction = "Confirm what is genuinely due today and pay or record evidence only when verified.",
            Severity = moneyObligationSummary.DueTodayItems > 0 ? PressureSeverity.Critical : PressureSeverity.Low,
            Lane = moneyObligationSummary.DueTodayItems > 0 ? PressureLane.ActNow : PressureLane.Protected,
            MoneyAmount = moneyObligationSummary.AmountDueToday,
            IsDueNow = moneyObligationSummary.DueTodayItems > 0,
            IsTrusted = true
        });

        signals.Add(new PressureSignal
        {
            Key = "follow-ups",
            Module = "Follow-Ups",
            Title = "Open follow-ups",
            Detail = $"{summary.FollowUps.TotalOpen} follow-up(s) remain open.",
            NextAction = "Only chase follow-ups that are due now; leave protected waiting items alone.",
            Severity = summary.FollowUps.TotalOpen > 0 ? PressureSeverity.High : PressureSeverity.Low,
            Lane = summary.FollowUps.TotalOpen > 0 ? PressureLane.Review : PressureLane.Protected,
            IsTrusted = true
        });

        signals.Add(new PressureSignal
        {
            Key = "daily-flow",
            Module = "Daily Operating Flow",
            Title = "Today flow",
            Detail = $"{dailyFlowSummary.TodayOpenCount} item(s) remain in today's operating flow with {dailyFlowSummary.StopPointCount} stop point(s).",
            NextAction = "Choose the highest-value action that can actually move today.",
            Severity = dailyFlowSummary.StopPointCount > 0 ? PressureSeverity.High : PressureSeverity.Normal,
            Lane = dailyFlowSummary.TodayOpenCount > 0 ? PressureLane.ActNow : PressureLane.Protected,
            IsDueNow = dailyFlowSummary.TodayOpenCount > 0,
            IsTrusted = true
        });

        signals.Add(new PressureSignal
        {
            Key = "work-blocked",
            Module = "Work Pipeline",
            Title = "Blocked work",
            Detail = $"{workPipelineSummary.BlockedItems} work item(s) are blocked.",
            NextAction = "Keep blocked work visible, record the blocker, and do not consume deep sprint time until it clears.",
            Severity = workPipelineSummary.BlockedItems > 0 ? PressureSeverity.High : PressureSeverity.Low,
            Lane = PressureLane.Waiting,
            IsBlocked = workPipelineSummary.BlockedItems > 0,
            IsWaitingOnOthers = workPipelineSummary.WaitingOnOthersItems > 0,
            IsTrusted = true
        });

        signals.Add(new PressureSignal
        {
            Key = "work-review",
            Module = "Work Pipeline",
            Title = "Work review needed",
            Detail = $"{workPipelineSummary.ReviewNeededItems} work item(s) need date, proof, payment, or next-action review.",
            NextAction = "Resolve the smallest review gap that unlocks paid work or removes uncertainty.",
            Severity = workPipelineSummary.ReviewNeededItems > 0 ? PressureSeverity.High : PressureSeverity.Low,
            Lane = workPipelineSummary.ReviewNeededItems > 0 ? PressureLane.Review : PressureLane.Protected,
            MoneyAmount = workPipelineSummary.ExpectedValueExcludedFromSafe,
            IsTrusted = false
        });

        signals.Add(new PressureSignal
        {
            Key = "receipt-review",
            Module = "Receipt OCR / Evidence",
            Title = "Receipt evidence review",
            Detail = $"{receiptEvidenceSummary.ReviewCount} receipt candidate(s) need review and {receiptEvidenceSummary.MissingSourceCount} are missing source evidence.",
            NextAction = "Attach or compare source evidence before accepting any receipt candidate.",
            Severity = receiptEvidenceSummary.MissingSourceCount > 0 ? PressureSeverity.High : PressureSeverity.Normal,
            Lane = receiptEvidenceSummary.ReviewCount > 0 ? PressureLane.Review : PressureLane.Protected,
            MoneyAmount = receiptEvidenceSummary.CandidateValue,
            IsTrusted = receiptEvidenceSummary.ReviewCount == 0
        });

        signals.Add(new PressureSignal
        {
            Key = "weekly-close-out",
            Module = "Weekly Close-Out",
            Title = "Weekly close-out",
            Detail = $"{weeklyCloseOutSummary.ReadyToCloseItems} can close and {weeklyCloseOutSummary.RollForwardItems} should roll forward.",
            NextAction = "Close trusted completed items, then deliberately assign next actions to roll-forward items.",
            Severity = weeklyCloseOutSummary.PressureLabel == "Critical"
                ? PressureSeverity.Critical
                : weeklyCloseOutSummary.PressureLabel == "High"
                    ? PressureSeverity.High
                    : PressureSeverity.Normal,
            Lane = weeklyCloseOutSummary.ReadyToCloseItems > 0 ? PressureLane.ActNow : PressureLane.Review,
            MoneyAmount = weeklyCloseOutSummary.MoneyStillUnderReview,
            IsDueNow = weeklyCloseOutSummary.ReadyToCloseItems > 0,
            IsTrusted = weeklyCloseOutSummary.UntrustedItems == 0
        });

        signals.Add(new PressureSignal
        {
            Key = "payment-calendar",
            Module = "Payment Calendar",
            Title = "Payment and agenda dates",
            Detail = $"{paymentCalendarSummary.TodayItems} item(s) land today and {paymentCalendarSummary.ReviewQueueItems} date item(s) need review.",
            NextAction = "Protect fixed commitments and verify payment dates without treating expected income as safe.",
            Severity = paymentCalendarSummary.TodayItems > 0 ? PressureSeverity.High : PressureSeverity.Normal,
            Lane = paymentCalendarSummary.TodayItems > 0 ? PressureLane.ActNow : PressureLane.Review,
            MoneyAmount = paymentCalendarSummary.AmountDueToday,
            IsDueNow = paymentCalendarSummary.TodayItems > 0,
            IsTrusted = paymentCalendarSummary.UntrustedItems == 0
        });

        signals.Add(new PressureSignal
        {
            Key = "expected-money",
            Module = "Money / Work Pipeline",
            Title = "Expected money excluded",
            Detail = $"{FormatMoney(moneyProfileSummary.ExpectedExcludedFromSafe)} remains expected rather than paid.",
            NextAction = "Keep expected money visible for planning but do not spend it until paid and cleared.",
            Severity = moneyProfileSummary.ExpectedExcludedFromSafe > 0 ? PressureSeverity.Normal : PressureSeverity.Low,
            Lane = PressureLane.Protected,
            MoneyAmount = moneyProfileSummary.ExpectedExcludedFromSafe,
            IsWaitingOnOthers = true,
            IsTrusted = true,
            SuppressionReason = "Expected money is visible for planning but protected from safe money."
        });

        return signals;
    }

    private Border CreatePressureSignalLanePanel(
        string title,
        string description,
        IEnumerable<PressureSignal> signals)
    {
        var panel = CreatePanel();
        panel.Margin = new Thickness(0, 16, 0, 0);

        var root = new StackPanel();
        root.Children.Add(new TextBlock
        {
            Text = title,
            Foreground = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        root.Children.Add(new TextBlock
        {
            Text = description,
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 4)
        });

        var list = signals.ToList();
        if (list.Count == 0)
        {
            root.Children.Add(CreateEmptyTextBlock("No pressure signals in this lane."));
            panel.Child = root;
            return panel;
        }

        foreach (var signal in list)
        {
            var moneyText = signal.MoneyAmount > 0 ? $" • Money: {FormatMoney(signal.MoneyAmount)}" : string.Empty;
            var body =
                $"{signal.Module} • {signal.Severity} • {signal.Lane}{moneyText}{Environment.NewLine}" +
                $"{signal.Detail}{Environment.NewLine}" +
                $"Next: {signal.NextAction}{Environment.NewLine}" +
                $"Trusted: {(signal.IsTrusted ? "Yes" : "No")} • Due now: {(signal.IsDueNow ? "Yes" : "No")}";

            var note = signal.IsSuppressed
                ? $"Suppressed: {signal.SuppressionReason}"
                : signal.SuppressionReason;

            root.Children.Add(CreateSimpleItemCard(signal.Title, body, note));
        }

        panel.Child = root;
        return panel;
    }

    private void ShowCommandCentre()
    {
        var summary = CommandCentreSummaryService.Create();
        var dailyFlowSummary = DailyOperatingFlowCalculator.Calculate(DailyOperatingFlowStorage.Load(), DateOnly.FromDateTime(DateTime.Today));
        var paidWorkProofSummary = PaidWorkMoneyProofSnapshotService.Create();
        var settingsSafetySummary = SettingsSafetyThemeCalculator.Calculate(SettingsSafetyThemeStorage.Load());
        var desktopReleaseSummary = DesktopReleaseReadinessCalculator.Calculate(DesktopReleaseStorage.Load());
        var universalSpineSummary = UniversalSpineCalculator.Calculate(UniversalSpineStorage.Load());
        var osNavigationSummary = OsNavigationCalculator.Calculate(OsNavigationStorage.Load());
        var searchKnowledgeSummary = SearchKnowledgeCalculator.Calculate(SearchKnowledgeStorage.Load());
        var finalOfflineOsSummary = FinalOfflineOsCalculator.Calculate(FinalOfflineOsStorage.Load());
        var lifeOsSpineSummary = LifeOsSpineCalculator.Calculate(LifeOsSpineStorage.Load());
        var itemStateSummary = LifeOsItemStateCalculator.Calculate(LifeOsItemStateStorage.Load());
        var moneyObligationSummary = MoneyObligationCalculator.Calculate(MoneyObligationStorage.Load());
        var moneyProfileSummary = MoneyProfileCalculator.Calculate(MoneyProfileStorage.Load());
        var paymentCalendarSummary = PaymentCalendarCalculator.Calculate(PaymentCalendarStorage.Load(), DateOnly.FromDateTime(DateTime.Today));
        var workPipelineOperatingSummary = WorkPipelineOperatingCalculator.Calculate(_workPipelineItems, DateOnly.FromDateTime(DateTime.Today));
        var receiptEvidenceSummary = ReceiptEvidenceCalculator.Calculate(_receiptEvidenceItems);
        var weeklyCloseOutOperatingSummary = WeeklyCloseOutOperatingCalculator.Calculate(_weeklyCloseOutReviewItems);
        var integrationInboxItems = IntegrationInboxStorage.Load();
        var integrationInboxSummary = IntegrationInboxCalculator.Calculate(integrationInboxItems);
        var pressureSignals = CreateCommandCentrePressureSignals(
            summary,
            dailyFlowSummary,
            moneyObligationSummary,
            moneyProfileSummary,
            paymentCalendarSummary,
            workPipelineOperatingSummary,
            receiptEvidenceSummary,
            weeklyCloseOutOperatingSummary);
        pressureSignals.Add(new PressureSignal
        {
            Key = "integration-inbox-review",
            Module = "Integration Inbox",
            Title = "Integration previews need review",
            Detail = $"{integrationInboxSummary.NeedsReview} preview(s) require review and {integrationInboxSummary.DuplicateSuspected} duplicate conflict(s) remain.",
            NextAction = "Review provenance and duplicates before accepting or linking any external preview.",
            Severity = integrationInboxSummary.NeedsReview >= 4
                ? PressureSeverity.Critical
                : integrationInboxSummary.NeedsReview > 0
                    ? PressureSeverity.High
                    : PressureSeverity.Low,
            Lane = integrationInboxSummary.NeedsReview > 0 ? PressureLane.Review : PressureLane.Protected,
            MoneyAmount = integrationInboxSummary.PreviewMoney,
            IsTrusted = integrationInboxSummary.NeedsReview == 0,
            IsDueNow = integrationInboxSummary.NeedsReview > 0
        });

        var pressureEngineSummary = CommandCentrePressureCalculator.Calculate(
            pressureSignals,
            _commandCentrePressurePolicy);

        SetHeader("Command Centre", "Unified Command Centre • v5.0.0-beta.1 • Integration checkpoint");

        var root = new StackPanel();

        root.Children.Add(CreateHeroPanel(
            "LifeOS Command Centre",
            "v5.0.0-beta.1 unifies local imports, Google Calendar, Gmail, Integration Inbox, and Email Radar into one manual, read-only, review-first integration checkpoint without allowing automatic external or LifeOS operational mutation."));

        var metricsPanel = new WrapPanel
        {
            Margin = new Thickness(0, 22, 0, 0)
        };

        metricsPanel.Children.Add(CreateDashboardCard("Overall pressure", summary.OverallPressureLabel, "This week"));
        metricsPanel.Children.Add(CreateDashboardCard("Safe to spend", FormatMoney(summary.MoneyPressure.SafeToSpend), summary.MoneyPressure.PressureLabel));
        metricsPanel.Children.Add(CreateDashboardCard("Agenda open", summary.Agenda.TotalOpen.ToString(), "Week"));
        metricsPanel.Children.Add(CreateDashboardCard("Pay later open", FormatMoney(summary.PayLater.TotalAmountOpen), "Deferred"));
        metricsPanel.Children.Add(CreateDashboardCard("Open follow-ups", summary.FollowUps.TotalOpen.ToString(), "Waiting-on"));
        metricsPanel.Children.Add(CreateDashboardCard("Daily flow", dailyFlowSummary.TodayOpenCount.ToString(), "Today"));
        metricsPanel.Children.Add(CreateDashboardCard("Stop points", dailyFlowSummary.StopPointCount.ToString(), "Control"));

        metricsPanel.Children.Add(CreateDashboardCard("Pipeline open", summary.WorkPipeline.OpenItems.ToString(), "Work Pipeline"));
        metricsPanel.Children.Add(CreateDashboardCard("Pipeline blocked", summary.WorkPipeline.BlockedItems.ToString(), "Blocked"));
        metricsPanel.Children.Add(CreateDashboardCard("Pipeline follow-ups", (summary.WorkPipeline.FollowUpsOverdue + summary.WorkPipeline.FollowUpsDueToday).ToString(), "Now"));
        metricsPanel.Children.Add(CreateDashboardCard("Expected pipeline", FormatMoney(summary.WorkPipeline.ExpectedValueTotal), "Not safe money"));
        metricsPanel.Children.Add(CreateDashboardCard("Work lane", workPipelineOperatingSummary.PressureLabel, "v4.5"));
        metricsPanel.Children.Add(CreateDashboardCard("Work review", workPipelineOperatingSummary.ReviewNeededItems.ToString(), "Pipeline"));
        metricsPanel.Children.Add(CreateDashboardCard("Waiting me", workPipelineOperatingSummary.WaitingOnMeItems.ToString(), "Owner"));
        metricsPanel.Children.Add(CreateDashboardCard("Invoice ready", (workPipelineOperatingSummary.InvoiceReadyItems + workPipelineOperatingSummary.TimesheetReadyItems).ToString(), "Admin"));
        metricsPanel.Children.Add(CreateDashboardCard("Payment expected", workPipelineOperatingSummary.PaymentExpectedItems.ToString(), FormatMoney(workPipelineOperatingSummary.ExpectedValueExcludedFromSafe)));

        metricsPanel.Children.Add(CreateDashboardCard("Receipt evidence", receiptEvidenceSummary.TotalItems.ToString(), "v4.6"));
        metricsPanel.Children.Add(CreateDashboardCard("Receipt review", receiptEvidenceSummary.ReviewCount.ToString(), receiptEvidenceSummary.PressureLabel));
        metricsPanel.Children.Add(CreateDashboardCard("Missing source", receiptEvidenceSummary.MissingSourceCount.ToString(), "Evidence"));
        metricsPanel.Children.Add(CreateDashboardCard("Accepted receipts", receiptEvidenceSummary.AcceptedCount.ToString(), "Trusted"));
        metricsPanel.Children.Add(CreateDashboardCard("Receipt value", FormatMoney(receiptEvidenceSummary.CandidateValue), "Not safe"));
        metricsPanel.Children.Add(CreateDashboardCard("Weekly close-out", weeklyCloseOutOperatingSummary.PressureLabel, "v4.7"));
        metricsPanel.Children.Add(CreateDashboardCard("Close now", weeklyCloseOutOperatingSummary.ReadyToCloseItems.ToString(), "This week"));
        metricsPanel.Children.Add(CreateDashboardCard("Roll forward", weeklyCloseOutOperatingSummary.RollForwardItems.ToString(), "Next week"));
        metricsPanel.Children.Add(CreateDashboardCard("Close-out blocked", weeklyCloseOutOperatingSummary.BlockedItems.ToString(), "Visible"));
        metricsPanel.Children.Add(CreateDashboardCard("Close-out money", FormatMoney(weeklyCloseOutOperatingSummary.MoneyStillUnderReview), "Not safe"));
        metricsPanel.Children.Add(CreateDashboardCard("Pressure engine", pressureEngineSummary.PressureLabel, "v4.8"));
        metricsPanel.Children.Add(CreateDashboardCard("Pressure score", pressureEngineSummary.PressureScore.ToString(), "Ranked"));
        metricsPanel.Children.Add(CreateDashboardCard("Critical signals", pressureEngineSummary.CriticalSignals.ToString(), "Act first"));
        metricsPanel.Children.Add(CreateDashboardCard("High signals", pressureEngineSummary.HighSignals.ToString(), "Visible"));
        metricsPanel.Children.Add(CreateDashboardCard("Act now", pressureEngineSummary.ActNowSignals.ToString(), "Today"));
        metricsPanel.Children.Add(CreateDashboardCard("Review first", pressureEngineSummary.ReviewSignals.ToString(), "Manual"));
        metricsPanel.Children.Add(CreateDashboardCard("Waiting", pressureEngineSummary.WaitingSignals.ToString(), "Do not chase"));
        metricsPanel.Children.Add(CreateDashboardCard("Protected", pressureEngineSummary.ProtectedSignals.ToString(), "Contained"));
        metricsPanel.Children.Add(CreateDashboardCard("Pressure money", FormatMoney(pressureEngineSummary.MoneyUnderPressure), "Not safe"));
        metricsPanel.Children.Add(CreateDashboardCard("Suppressed", pressureEngineSummary.SuppressedSignals.ToString(), "Safety"));
        metricsPanel.Children.Add(CreateDashboardCard("Integration previews", integrationInboxSummary.Total.ToString(), "v5.0.0-alpha.4"));
        metricsPanel.Children.Add(CreateDashboardCard("Integration review", integrationInboxSummary.NeedsReview.ToString(), "Manual gate"));
        metricsPanel.Children.Add(CreateDashboardCard("Preview conflicts", integrationInboxSummary.DuplicateSuspected.ToString(), "Duplicate"));
        metricsPanel.Children.Add(CreateDashboardCard("Imported value", FormatMoney(integrationInboxSummary.PreviewMoney), "Not safe"));




        metricsPanel.Children.Add(CreateDashboardCard("Billable value", FormatMoney(summary.WorkSessions.BillableValue), "Work"));
        metricsPanel.Children.Add(CreateDashboardCard("Unpaid work", FormatMoney(summary.WorkSessions.UnpaidBillableValue), "Income"));
        metricsPanel.Children.Add(CreateDashboardCard("Proof items", summary.ProofTracker.TotalProofItems.ToString(), "Proof"));
        metricsPanel.Children.Add(CreateDashboardCard("Proof ready", summary.ProofTracker.ReadyCount.ToString(), "Shareable"));
        metricsPanel.Children.Add(CreateDashboardCard("Paid-work risk", FormatMoney(paidWorkProofSummary.MoneyAtRisk), FormatPaidWorkMoneyProofHealth(paidWorkProofSummary.Health)));
        metricsPanel.Children.Add(CreateDashboardCard("Admin actions", paidWorkProofSummary.AdminActionCount.ToString(), "Money/proof"));
        metricsPanel.Children.Add(CreateDashboardCard("Safety mode", settingsSafetySummary.SafetyLabel, "Settings"));
        metricsPanel.Children.Add(CreateDashboardCard("Guardrails", settingsSafetySummary.EnabledGuardrails.ToString(), "Safety"));
        metricsPanel.Children.Add(CreateDashboardCard("Release state", desktopReleaseSummary.ReleaseStateLabel, "v2.0"));
        metricsPanel.Children.Add(CreateDashboardCard("Release readiness", $"{desktopReleaseSummary.ScorePercent}%", "Desktop"));
        metricsPanel.Children.Add(CreateDashboardCard("Spine items", universalSpineSummary.TotalItems.ToString(), "v2.1"));
        metricsPanel.Children.Add(CreateDashboardCard("Spine links", universalSpineSummary.LinkCount.ToString(), "Cross-module"));
        metricsPanel.Children.Add(CreateDashboardCard("Spine review", universalSpineSummary.ReviewNeededItems.ToString(), "Manual"));
        metricsPanel.Children.Add(CreateDashboardCard("Visible modules", osNavigationSummary.VisibleModules.ToString(), "v3.0"));
        metricsPanel.Children.Add(CreateDashboardCard("Workspace groups", osNavigationSummary.GroupCount.ToString(), "OS map"));
        metricsPanel.Children.Add(CreateDashboardCard("Core modules", osNavigationSummary.CoreModules.ToString(), "Protected"));
        metricsPanel.Children.Add(CreateDashboardCard("Knowledge items", searchKnowledgeSummary.TotalItems.ToString(), "v3.5"));
        metricsPanel.Children.Add(CreateDashboardCard("Search profiles", searchKnowledgeSummary.ProfileCount.ToString(), "Scoped"));
        metricsPanel.Children.Add(CreateDashboardCard("Knowledge review", searchKnowledgeSummary.ReviewNeededItems.ToString(), "Manual"));
        metricsPanel.Children.Add(CreateDashboardCard("Offline OS", finalOfflineOsSummary.LocalFirstComplete ? "Complete" : "Review", "v3.9"));
        metricsPanel.Children.Add(CreateDashboardCard("v4 landing zones", finalOfflineOsSummary.LandingZones.ToString(), "Ready map"));
        metricsPanel.Children.Add(CreateDashboardCard("v4 ready", finalOfflineOsSummary.ReadyForV4Integrations ? "Yes" : "No", "Gate"));
        metricsPanel.Children.Add(CreateDashboardCard("Spine modules", lifeOsSpineSummary.ModuleCount.ToString(), "v4 map"));
        metricsPanel.Children.Add(CreateDashboardCard("Item rules", lifeOsSpineSummary.ItemRuleCount.ToString(), "Stateful"));
        metricsPanel.Children.Add(CreateDashboardCard("Pressure sources", lifeOsSpineSummary.PressureSourceCount.ToString(), "Weekly"));
        metricsPanel.Children.Add(CreateDashboardCard("State items", itemStateSummary.TotalItems.ToString(), "v4.1"));
        metricsPanel.Children.Add(CreateDashboardCard("Needs review", itemStateSummary.NeedsReviewItems.ToString(), "Items"));
        metricsPanel.Children.Add(CreateDashboardCard("Untrusted", itemStateSummary.UntrustedItems.ToString(), "Items"));
        metricsPanel.Children.Add(CreateDashboardCard("Money obligations", moneyObligationSummary.TotalItems.ToString(), "v4.2"));
        metricsPanel.Children.Add(CreateDashboardCard("Due today", moneyObligationSummary.DueTodayItems.ToString(), FormatMoney(moneyObligationSummary.AmountDueToday)));
        metricsPanel.Children.Add(CreateDashboardCard("Pay later load", FormatMoney(moneyObligationSummary.PayLaterBalance), "BNPL"));
        metricsPanel.Children.Add(CreateDashboardCard("Hidden deductions", moneyObligationSummary.HiddenDeductionItems.ToString(), FormatMoney(moneyObligationSummary.HiddenDeductionMonthlyEstimate)));
        metricsPanel.Children.Add(CreateDashboardCard("Safe drag", FormatMoney(moneyObligationSummary.SafeToSpendDrag), "v4.2"));
        metricsPanel.Children.Add(CreateDashboardCard("Money profile", moneyProfileSummary.ConfidenceLabel, "v4.3"));
        metricsPanel.Children.Add(CreateDashboardCard("Final safe", FormatMoney(moneyProfileSummary.SafeToSpendFinal), "v4.3"));
        metricsPanel.Children.Add(CreateDashboardCard("Expected excluded", FormatMoney(moneyProfileSummary.ExpectedExcludedFromSafe), "Not safe"));
        metricsPanel.Children.Add(CreateDashboardCard("Hidden weekly", FormatMoney(moneyProfileSummary.HiddenDeductionWeeklyReserve), "Reserve"));
        metricsPanel.Children.Add(CreateDashboardCard("Calendar lane", paymentCalendarSummary.PressureLabel, "v4.4"));
        metricsPanel.Children.Add(CreateDashboardCard("Calendar items", paymentCalendarSummary.TotalItems.ToString(), "Dates"));
        metricsPanel.Children.Add(CreateDashboardCard("Payment dates", paymentCalendarSummary.PaymentDateItems.ToString(), FormatMoney(paymentCalendarSummary.AmountDueThisWeek)));
        metricsPanel.Children.Add(CreateDashboardCard("Agenda dates", paymentCalendarSummary.AgendaCommitments.ToString(), "Time"));
        metricsPanel.Children.Add(CreateDashboardCard("Review windows", paymentCalendarSummary.ReviewQueueItems.ToString(), "Manual"));
        metricsPanel.Children.Add(CreateDashboardCard("Expected dates", FormatMoney(paymentCalendarSummary.ExpectedMoneyExcluded), "Not safe"));

        root.Children.Add(metricsPanel);

        var actionPanel = CreateInfoPanel(
            "Next safest action",
            pressureEngineSummary.NextSafestAction);

        actionPanel.Margin = new Thickness(0, 8, 0, 0);
        root.Children.Add(actionPanel);


        var pressureRulePanel = CreateInfoPanel(
            "v4.8 Command Centre Pressure Engine rule",
            FormatReasons(new[]
            {
                "Rank reviewed local signals instead of dumping every module card into Today.",
                "Critical and due-now items rise first.",
                "Untrusted items go to review before action.",
                "Waiting-on-others items are protected until due unless critical.",
                "Expected money and pressure money remain excluded from safe money.",
                "Blocked work stays visible without consuming active sprint time.",
                $"Pressure score: {pressureEngineSummary.PressureScore}.",
                $"Visible signals: {pressureEngineSummary.TopSignals.Count} top / {pressureEngineSummary.TotalSignals} total."
            }));

        pressureRulePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(pressureRulePanel);

        root.Children.Add(CreatePressureSignalLanePanel(
            "Top ranked pressure signals",
            "The highest-ranked visible signals after trust, waiting, due-now, and suppression rules.",
            pressureEngineSummary.TopSignals));

        root.Children.Add(CreatePressureSignalLanePanel(
            "Act now",
            "Trusted or time-critical signals that can move today.",
            pressureEngineSummary.ActNow));

        root.Children.Add(CreatePressureSignalLanePanel(
            "Review before action",
            "Untrusted, imported, estimated, or incomplete signals that require a human gate.",
            pressureEngineSummary.Review));

        root.Children.Add(CreatePressureSignalLanePanel(
            "Waiting / do not chase",
            "Blocked or externally owned pressure that remains visible without hijacking active work.",
            pressureEngineSummary.Waiting));

        root.Children.Add(CreatePressureSignalLanePanel(
            "Protected / suppressed",
            "Expected money, parked work, fixed commitments, and waiting-on-others signals contained by safety rules.",
            pressureEngineSummary.Protected));

        var pressureControlsPanel = CreatePanel();
        pressureControlsPanel.Margin = new Thickness(0, 16, 0, 0);
        var pressureControlsRoot = new StackPanel();
        pressureControlsRoot.Children.Add(new TextBlock
        {
            Text = "Pressure engine controls",
            Foreground = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });
        pressureControlsRoot.Children.Add(new TextBlock
        {
            Text = "The local policy controls score weights, top-signal limits, waiting-on suppression, and untrusted review gates.",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 12)
        });
        pressureControlsRoot.Children.Add(new TextBlock
        {
            Text = $"Weights L/N/H/C: {_commandCentrePressurePolicy.LowWeight}/{_commandCentrePressurePolicy.NormalWeight}/{_commandCentrePressurePolicy.HighWeight}/{_commandCentrePressurePolicy.CriticalWeight} • " +
                   $"Thresholds N/H/C: {_commandCentrePressurePolicy.NormalScore}/{_commandCentrePressurePolicy.HighScore}/{_commandCentrePressurePolicy.CriticalScore} • " +
                   $"Top signals: {_commandCentrePressurePolicy.MaximumTopSignals}",
            Foreground = new SolidColorBrush(Color.FromRgb(203, 213, 225)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 12)
        });
        var resetPressurePolicyButton = CreateSmallActionButton("Reset v4.8 pressure policy");
        resetPressurePolicyButton.Click += ResetCommandCentrePressurePolicyButton_Click;
        pressureControlsRoot.Children.Add(resetPressurePolicyButton);
        pressureControlsPanel.Child = pressureControlsRoot;
        root.Children.Add(pressureControlsPanel);

        var pressureBridgePanel = CreateInfoPanel(
            "v4.8 module pressure bridge",
            FormatReasons(new[]
            {
                "Money Profile and Bills supply safe-money and due-now pressure.",
                "Payment Calendar supplies time and payment-date pressure.",
                "Work Pipeline supplies active, blocked, waiting, invoice-ready, and payment-expected pressure.",
                "Receipt Evidence supplies untrusted and missing-source review pressure.",
                "Weekly Close-Out supplies close-now and deliberate roll-forward pressure.",
                "Daily Operating Flow supplies what can actually move today.",
                "The engine ranks signals; it does not replace the source modules."
            }));
        pressureBridgePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(pressureBridgePanel);

        var unifiedPanel = CreateInfoPanel(
            "What matters now",
            FormatReasons(summary.Snapshot.TodayActions.Select(action => $"{action.Priority} • {action.Title}: {action.Action}")));

        unifiedPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(unifiedPanel);

        var hiddenPanel = CreateInfoPanel(
            "Hidden from Today",
            summary.Snapshot.HiddenSignals.Count == 0
                ? "No parked/passive signals hidden right now."
                : FormatReasons(summary.Snapshot.HiddenSignals.Select(signal => $"{signal.Title}: {signal.NextAction}")));

        hiddenPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(hiddenPanel);

        var reasonsPanel = CreateInfoPanel(
            "Why this week has pressure",
            FormatReasons(summary.Reasons));

        reasonsPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(reasonsPanel);

        var workPipelinePanel = CreateInfoPanel(
            "Work Pipeline pressure",
            FormatReasons(summary.WorkPipeline.Reasons));

        workPipelinePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(workPipelinePanel);

        var pipelineSignalsPanel = CreateInfoPanel(
            "Pipeline command signals",
            FormatReasons(summary.WorkPipeline.CommandCentreSignals.Select(signal => $"{signal.Label}: {signal.Value} - {signal.Detail}")));

        pipelineSignalsPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(pipelineSignalsPanel);

        var pipelineOperatingPanel = CreateInfoPanel(
            "v4.5 Work Pipeline operating lane",
            FormatReasons(workPipelineOperatingSummary.Reasons));

        pipelineOperatingPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(pipelineOperatingPanel);

        var pipelineOperatingSignalsPanel = CreateInfoPanel(
            "v4.5 Work Pipeline signals",
            FormatReasons(workPipelineOperatingSummary.CommandCentreSignals.Select(signal => $"{signal.Label}: {signal.Value} - {signal.Detail}")));

        pipelineOperatingSignalsPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(pipelineOperatingSignalsPanel);

        var pipelineIntegrityPanel = CreateInfoPanel(
            "v4.5 Pipeline integrity warnings",
            workPipelineOperatingSummary.IntegrityWarnings.Count == 0
                ? "No v4.5 pipeline integrity warnings."
                : FormatReasons(workPipelineOperatingSummary.IntegrityWarnings));

        pipelineIntegrityPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(pipelineIntegrityPanel);

        var receiptEvidencePanel = CreateInfoPanel(
            "v4.6 Receipt OCR / Evidence-to-Item",
            FormatReasons(receiptEvidenceSummary.Reasons));

        receiptEvidencePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(receiptEvidencePanel);

        var receiptEvidenceSignalsPanel = CreateInfoPanel(
            "v4.6 receipt evidence signals",
            FormatReasons(new[]
            {
                $"Candidates: {receiptEvidenceSummary.TotalItems}",
                $"Needs review: {receiptEvidenceSummary.ReviewCount}",
                $"Missing source: {receiptEvidenceSummary.MissingSourceCount}",
                $"Accepted and trusted: {receiptEvidenceSummary.AcceptedCount}",
                $"Candidate value excluded from safe money: {FormatMoney(receiptEvidenceSummary.CandidateValue)}"
            }));

        receiptEvidenceSignalsPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(receiptEvidenceSignalsPanel);

        var weeklyCloseOutPanel = CreateInfoPanel(
            "v4.7 Weekly Close-Out",
            FormatReasons(weeklyCloseOutOperatingSummary.Reasons));

        weeklyCloseOutPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(weeklyCloseOutPanel);

        var weeklyCloseOutSignalsPanel = CreateInfoPanel(
            "v4.7 weekly close-out signals",
            FormatReasons(new[]
            {
                $"Close now: {weeklyCloseOutOperatingSummary.ReadyToCloseItems}",
                $"Roll forward: {weeklyCloseOutOperatingSummary.RollForwardItems}",
                $"Waiting: {weeklyCloseOutOperatingSummary.WaitingItems}",
                $"Blocked: {weeklyCloseOutOperatingSummary.BlockedItems}",
                $"Money under review: {FormatMoney(weeklyCloseOutOperatingSummary.MoneyStillUnderReview)}",
                $"Proof checks: {weeklyCloseOutOperatingSummary.ProofReviewItems}",
                $"Receipt checks: {weeklyCloseOutOperatingSummary.ReceiptReviewItems}",
                $"Work checks: {weeklyCloseOutOperatingSummary.WorkReviewItems}"
            }));

        weeklyCloseOutSignalsPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(weeklyCloseOutSignalsPanel);

        var pressureBoundaryPanel = CreateInfoPanel(
            "v4.8 boundary",
            "Local ranking and suppression only. v4.8 does not send messages, pay bills, move money, close projects, create invoices, accept OCR, change external calendars, sync external systems, or run AI actions.");
        pressureBoundaryPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(pressureBoundaryPanel);

        var pressureNextPanel = CreateInfoPanel(
            "Connector foundation",
            "v5.0.0-alpha.4 can rank manually reviewed local CSV, JSON, and ICS previews while confirmation gates, duplicate detection, and audit history keep external data contained.");
        pressureNextPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(pressureNextPanel);

        var pressureFilePanel = CreateInfoPanel(
            "Local pressure policy file",
            CommandCentrePressurePolicyStorage.FilePath);
        pressureFilePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(pressureFilePanel);

        var dailyFlowPanel = CreateInfoPanel(
            "Daily Operating Flow",
            FormatReasons(dailyFlowSummary.TodayBlocks.Take(6).Select(block => $"{block.Pressure} • {block.Kind} • {block.Title}: {block.NextAction}")));

        dailyFlowPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(dailyFlowPanel);

        var paidWorkProofPanel = CreateInfoPanel(
            "Paid Work / Money / Proof",
            FormatReasons(paidWorkProofSummary.Reasons));

        paidWorkProofPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(paidWorkProofPanel);

        var safetyPanel = CreateInfoPanel(
            "Settings / Safety / Theme",
            FormatReasons(settingsSafetySummary.GuardrailReasons));

        safetyPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(safetyPanel);

        var releasePanel = CreateInfoPanel(
            "Desktop Release Centre",
            FormatReasons(desktopReleaseSummary.Reasons));

        releasePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(releasePanel);

        var spinePanel = CreateInfoPanel(
            "Universal Spine",
            FormatReasons(universalSpineSummary.Reasons));

        spinePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(spinePanel);

        var navigationPanel = CreateInfoPanel(
            "OS Navigation Centre",
            FormatReasons(osNavigationSummary.Reasons));

        navigationPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(navigationPanel);

        var knowledgePanel = CreateInfoPanel(
            "Search / Knowledge Centre",
            FormatReasons(searchKnowledgeSummary.Reasons));

        knowledgePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(knowledgePanel);

        var finalOsPanel = CreateInfoPanel(
            "Final Offline OS",
            FormatReasons(finalOfflineOsSummary.Reasons));

        finalOsPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(finalOsPanel);

        var lifeOsSpineMapPanel = CreateInfoPanel(
            "LifeOS Spine Map",
            FormatReasons(lifeOsSpineSummary.Reasons));

        lifeOsSpineMapPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(lifeOsSpineMapPanel);

        var itemStatePanel = CreateInfoPanel(
            "Item State Engine",
            FormatReasons(itemStateSummary.Reasons));

        itemStatePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(itemStatePanel);

        var moneyObligationsPanel = CreateInfoPanel(
            "Bills / Upcoming Payments / Pay Later",
            FormatReasons(moneyObligationSummary.Reasons));

        moneyObligationsPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(moneyObligationsPanel);

        var moneyProfilePanel = CreateInfoPanel(
            "Money Profile / Hidden Deductions / Safe-to-Spend",
            FormatReasons(moneyProfileSummary.Reasons));

        moneyProfilePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(moneyProfilePanel);

        var paymentCalendarPanel = CreateInfoPanel(
            "Agenda + Payment Calendar",
            FormatReasons(paymentCalendarSummary.Reasons));

        paymentCalendarPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(paymentCalendarPanel);

        var workPanel = CreateInfoPanel(
            "v4.5 operating rule",
            "LifeOS now makes Work Pipeline the operating lane for active work, leads, blocked projects, follow-ups, invoice readiness, payment expected state, and proof gaps before v5 integrations.");

        workPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(workPanel);

        var guardrailPanel = CreateInfoPanel(
            "v4.5 scope",
            "v4.5 models local Work Pipeline operating state only. It does not send messages, sync email/calendar/accounting/bank data, create invoices, create calendar events, run AI actions, replace a CRM, or perform the major workspace redesign.");

        guardrailPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(guardrailPanel);

        MainContentControl.Content = root;
    }
}
