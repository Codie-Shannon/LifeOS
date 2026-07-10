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

using LifeOS.Core.Agenda;
using LifeOS.Core.PayLater;
using LifeOS.Core.WeeklyCloseOut;
using LifeOS.Shared.Agenda;
using LifeOS.Shared.PayLater;
using LifeOS.Shared.WeeklyCloseOut;
using LifeOS.Core.WorkSessions;
using LifeOS.Core.ProofTracker;
using LifeOS.Shared.WorkSessions;
using LifeOS.Shared.ProofTracker;
namespace LifeOS.Desktop;

public partial class MainWindow : Window
{
    private MoneyPressureManualInput _moneyPressureInput = MoneyPressureStorage.Load();

    private TextBox? _currentBalanceTextBox;
    private TextBox? _paidIncomeTextBox;
    private TextBox? _pendingIncomeTextBox;
    private TextBox? _billsDueTextBox;
    private TextBox? _deductionsDueTextBox;
    private TextBox? _foodFuelBufferTextBox;
    private TextBox? _emergencyBufferTextBox;

    private List<FollowUpItem> _followUps = FollowUpStorage.Load();

    private TextBox? _followUpPersonTextBox;
    private TextBox? _followUpContextTextBox;
    private TextBox? _followUpNextActionTextBox;
    private TextBox? _followUpDateTextBox;
    private ComboBox? _followUpStatusComboBox;
    private ComboBox? _followUpPriorityComboBox;
    private CheckBox? _followUpMoneyLinkedCheckBox;
    private TextBox? _followUpNotesTextBox;


    private List<AgendaItem> _agendaItems = AgendaStorage.Load();

    private TextBox? _agendaTitleTextBox;
    private TextBox? _agendaDueDateTextBox;
    private TextBox? _agendaTimeTextBox;
    private ComboBox? _agendaTypeComboBox;
    private ComboBox? _agendaStatusComboBox;
    private ComboBox? _agendaPressureComboBox;
    private CheckBox? _agendaFixedCheckBox;
    private TextBox? _agendaNotesTextBox;

    private List<PayLaterItem> _payLaterItems = PayLaterStorage.Load();

    private TextBox? _payLaterNameTextBox;
    private TextBox? _payLaterPayeeTextBox;
    private TextBox? _payLaterAmountTextBox;
    private TextBox? _payLaterDueDateTextBox;
    private ComboBox? _payLaterStatusComboBox;
    private ComboBox? _payLaterPressureComboBox;
    private TextBox? _payLaterNotesTextBox;

    private List<WeeklyCloseOutEntry> _weeklyCloseOutEntries = WeeklyCloseOutStorage.Load();
    private List<WeeklyCloseOutReviewItem> _weeklyCloseOutReviewItems = WeeklyCloseOutReviewStorage.Load();

    private TextBox? _closeOutWeekStartTextBox;
    private TextBox? _closeOutDoneTextBox;
    private TextBox? _closeOutMovedTextBox;
    private TextBox? _closeOutWaitingTextBox;
    private TextBox? _closeOutNextWeekTextBox;
    private TextBox? _closeOutNotesTextBox;

    private List<WorkSession> _workSessions = WorkSessionStorage.Load();

    private TextBox? _workSessionClientTextBox;
    private TextBox? _workSessionDateTextBox;
    private TextBox? _workSessionHoursTextBox;
    private TextBox? _workSessionRateTextBox;
    private ComboBox? _workSessionStatusComboBox;
    private CheckBox? _workSessionBillableCheckBox;
    private TextBox? _workSessionDescriptionTextBox;
    private TextBox? _workSessionNotesTextBox;

    private List<ProofItem> _proofItems = ProofStorage.Load();

    private List<RelationshipRadarProfile> _relationshipProfiles = RelationshipRadarStorage.Load();

    private TextBox? _relationshipNameTextBox;
    private TextBox? _relationshipContextTextBox;
    private ComboBox? _relationshipStatusComboBox;
    private ComboBox? _relationshipWaitingOnComboBox;
    private TextBox? _relationshipLastContactTextBox;
    private TextBox? _relationshipNextFollowUpTextBox;
    private TextBox? _relationshipLinkedWorkTextBox;
    private TextBox? _relationshipNextActionTextBox;
    private TextBox? _relationshipNotesTextBox;
    private CheckBox? _relationshipDoNotChaseCheckBox;



    private SettingsSafetyThemeProfile _settingsSafetyProfile = SettingsSafetyThemeStorage.Load();

    private ComboBox? _settingsSafetyModeComboBox;
    private ComboBox? _settingsAppearanceComboBox;
    private ComboBox? _settingsAccentComboBox;
    private CheckBox? _settingsLocalOnlyCheckBox;
    private CheckBox? _settingsManualReviewCheckBox;
    private CheckBox? _settingsExpectedMoneyUnsafeCheckBox;
    private CheckBox? _settingsScreenshotSafetyCheckBox;
    private CheckBox? _settingsConfirmDestructiveCheckBox;
    private CheckBox? _settingsDemoSafeDataCheckBox;
    private CheckBox? _settingsExperimentalModulesCheckBox;
    private TextBox? _settingsBuildLaneTextBox;
    private TextBox? _settingsVersionNoteTextBox;

    private DesktopReleaseReadinessProfile _desktopReleaseProfile = DesktopReleaseStorage.Load();

    private UniversalSpineProfile _universalSpineProfile = UniversalSpineStorage.Load();

    private OsNavigationProfile _osNavigationProfile = OsNavigationStorage.Load();

    private SearchKnowledgeProfile _searchKnowledgeProfile = SearchKnowledgeStorage.Load();

    private FinalOfflineOsProfile _finalOfflineOsProfile = FinalOfflineOsStorage.Load();
    private LifeOsSpineProfile _lifeOsSpineProfile = LifeOsSpineStorage.Load();

    private LifeOsItemStateProfile _lifeOsItemStateProfile = LifeOsItemStateStorage.Load();

    private MoneyObligationProfile _moneyObligationProfile = MoneyObligationStorage.Load();

    private MoneyProfilePlan _moneyProfilePlan = MoneyProfileStorage.Load();

    private PaymentCalendarPlan _paymentCalendarPlan = PaymentCalendarStorage.Load();

    private List<ReceiptEvidenceItem> _receiptEvidenceItems = ReceiptEvidenceStorage.Load();


    private List<WorkPipelineItem> _workPipelineItems = WorkPipelineStorage.Load();
    private string _workPipelineFilter = "Active";

    private TextBox? _workPipelineTitleTextBox;
    private TextBox? _workPipelineContactTextBox;
    private TextBox? _workPipelineCompanyTextBox;
    private TextBox? _workPipelineCategoryTextBox;
    private TextBox? _workPipelineOpportunityTypeTextBox;
    private TextBox? _workPipelineWaitingOnTextBox;
    private ComboBox? _workPipelineStageComboBox;
    private ComboBox? _workPipelineStatusComboBox;
    private ComboBox? _workPipelinePriorityComboBox;
    private TextBox? _workPipelineNextActionTextBox;
    private TextBox? _workPipelineFollowUpDateTextBox;
    private TextBox? _workPipelineExpectedValueTextBox;
    private TextBox? _workPipelineExpectedValueNoteTextBox;
    private CheckBox? _workPipelineBillableCheckBox;
    private CheckBox? _workPipelineTimesheetCheckBox;
    private CheckBox? _workPipelineInvoiceCheckBox;
    private CheckBox? _workPipelinePaymentExpectedCheckBox;
    private TextBox? _workPipelineNotesTextBox;

    private TextBox? _proofProjectTextBox;
    private TextBox? _proofTitleTextBox;
    private ComboBox? _proofTypeComboBox;
    private ComboBox? _proofStatusComboBox;
    private TextBox? _proofDateTextBox;
    private TextBox? _proofDescriptionTextBox;
    private TextBox? _proofLinkOrPathTextBox;
    private TextBox? _proofNotesTextBox;

    public MainWindow()
    {
        InitializeComponent();
        ShowCommandCentre();
    }

    private void CommandCentreNavButton_Click(object sender, RoutedEventArgs e) => ShowCommandCentre();

    private void MoneyPressureNavButton_Click(object sender, RoutedEventArgs e) => ShowMoneyPressurePage();

    private void MoneyTimelineNavButton_Click(object sender, RoutedEventArgs e) => ShowMoneyTimelinePage();

    private void BillsPaymentsNavButton_Click(object sender, RoutedEventArgs e) => ShowBillsPaymentsPage();

    private void MoneyProfileNavButton_Click(object sender, RoutedEventArgs e) => ShowMoneyProfilePage();

    private void AgendaNavButton_Click(object sender, RoutedEventArgs e) => ShowAgendaPage();

    private void PaymentCalendarNavButton_Click(object sender, RoutedEventArgs e) => ShowPaymentCalendarPage();

    private void PayLaterNavButton_Click(object sender, RoutedEventArgs e) => ShowPayLaterPage();

    private void WeeklyCloseOutNavButton_Click(object sender, RoutedEventArgs e) => ShowWeeklyCloseOutPage();

    private void WorkSessionsNavButton_Click(object sender, RoutedEventArgs e) => ShowWorkSessionsPage();

    private void PaidWorkCentreNavButton_Click(object sender, RoutedEventArgs e) => ShowPaidWorkCentrePage();

    private void ProofTrackerNavButton_Click(object sender, RoutedEventArgs e) => ShowProofTrackerPage();

    private void FollowUpsNavButton_Click(object sender, RoutedEventArgs e) => ShowFollowUpsPage();

    private void WorkPipelineNavButton_Click(object sender, RoutedEventArgs e) => ShowWorkPipelinePage();

    private void ReceiptEvidenceNavButton_Click(object sender, RoutedEventArgs e) => ShowReceiptEvidencePage();

    private void DailyOperatingFlowNavButton_Click(object sender, RoutedEventArgs e) => ShowDailyOperatingFlowPage();

    private void DailyStateNavButton_Click(object sender, RoutedEventArgs e) => ShowDailyStatePage();

    private void TimesheetEvidenceNavButton_Click(object sender, RoutedEventArgs e) => ShowTimesheetEvidencePage();

    private void EvidenceVaultNavButton_Click(object sender, RoutedEventArgs e) => ShowEvidenceVaultPage();

    private void RelationshipRadarNavButton_Click(object sender, RoutedEventArgs e) => ShowRelationshipRadarPage();

    private void ProjectsNavButton_Click(object sender, RoutedEventArgs e) => ShowModulePage(LifeOSModuleKind.Projects);

    private void TimerAgentNavButton_Click(object sender, RoutedEventArgs e) => ShowModulePage(LifeOSModuleKind.TimerAgent);

        private void ItemStateEngineNavButton_Click(object sender, RoutedEventArgs e) => ShowItemStateEnginePage();

    private void LifeOsSpineNavButton_Click(object sender, RoutedEventArgs e) => ShowLifeOsSpinePage();

    private void FinalOfflineOsNavButton_Click(object sender, RoutedEventArgs e) => ShowFinalOfflineOsPage();

    private void SearchKnowledgeNavButton_Click(object sender, RoutedEventArgs e) => ShowSearchKnowledgePage();

    private void OsNavigationNavButton_Click(object sender, RoutedEventArgs e) => ShowOsNavigationPage();

    private void UniversalSpineNavButton_Click(object sender, RoutedEventArgs e) => ShowUniversalSpinePage();

    private void DesktopReleaseNavButton_Click(object sender, RoutedEventArgs e) => ShowDesktopReleasePage();

    private void SettingsNavButton_Click(object sender, RoutedEventArgs e) => ShowSettingsSafetyThemePage();

    private void RecalculateMoneyPressureButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateMoneyPressureInputFromTextBoxes();
        ShowMoneyPressurePage();
    }








    



    private void ResetMoneyProfileButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmRiskyAction("Reset money profile demo data?", "This will replace the local Money Profile with fictional/demo hidden deduction and safe-to-spend data."))
        {
            return;
        }

        MoneyProfileStorage.ResetToDemoData();
        _moneyProfilePlan = MoneyProfileStorage.Load();
        ShowMoneyProfilePage();
    }


    private void ResetPaymentCalendarButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmRiskyAction("Reset payment calendar demo data?", "This will replace the local Payment Calendar with fictional/demo agenda, bill, BNPL, expected income, and review-window dates."))
        {
            return;
        }

        PaymentCalendarStorage.ResetToDemoData();
        _paymentCalendarPlan = PaymentCalendarStorage.Load();
        ShowPaymentCalendarPage();
    }

    private void ShowPaymentCalendarPage()
    {
        _paymentCalendarPlan = PaymentCalendarStorage.Load();
        var today = DateOnly.FromDateTime(DateTime.Today);
        var summary = PaymentCalendarCalculator.Calculate(_paymentCalendarPlan, today);

        SetHeader("Payment Calendar", $"Payment Calendar • v4.4 • {summary.TotalItems} date items • {summary.ThisWeekItems} this week • {summary.ReviewQueueItems} review");

        var root = new StackPanel();

        root.Children.Add(CreateHeroPanel(
            "Agenda + Payment Calendar",
            "v4.4 puts agenda commitments, bills, subscriptions, BNPL instalments, expected-money dates, review windows, and weekly close-out points into one time-aware lane before v5 integrations."));

        var metricsPanel = new WrapPanel
        {
            Margin = new Thickness(0, 22, 0, 0)
        };

        metricsPanel.Children.Add(CreateDashboardCard("Engine", "Active", "v4.4"));
        metricsPanel.Children.Add(CreateDashboardCard("Pressure", summary.PressureLabel, "Time/money"));
        metricsPanel.Children.Add(CreateDashboardCard("Date items", summary.TotalItems.ToString(), "Tracked"));
        metricsPanel.Children.Add(CreateDashboardCard("This week", summary.ThisWeekItems.ToString(), "Window"));
        metricsPanel.Children.Add(CreateDashboardCard("Today", summary.TodayItems.ToString(), FormatMoney(summary.AmountDueToday)));
        metricsPanel.Children.Add(CreateDashboardCard("Due soon", summary.DueSoonItems.ToString(), "7 days"));
        metricsPanel.Children.Add(CreateDashboardCard("Overdue", summary.OverdueItems.ToString(), "Risk"));
        metricsPanel.Children.Add(CreateDashboardCard("Payment dates", summary.PaymentDateItems.ToString(), FormatMoney(summary.AmountDueThisWeek)));
        metricsPanel.Children.Add(CreateDashboardCard("Agenda dates", summary.AgendaCommitments.ToString(), "Time"));
        metricsPanel.Children.Add(CreateDashboardCard("Fixed", summary.FixedCommitments.ToString(), "Protected"));
        metricsPanel.Children.Add(CreateDashboardCard("Pay later dates", summary.PayLaterDates.ToString(), FormatMoney(summary.PayLaterDueThisWeek)));
        metricsPanel.Children.Add(CreateDashboardCard("Expected income", summary.ExpectedMoneyDates.ToString(), FormatMoney(summary.ExpectedMoneyExcluded)));
        metricsPanel.Children.Add(CreateDashboardCard("Review windows", summary.ReviewQueueItems.ToString(), "Manual"));
        metricsPanel.Children.Add(CreateDashboardCard("Untrusted", summary.UntrustedItems.ToString(), "Dates"));
        metricsPanel.Children.Add(CreateDashboardCard("Safe alerts", summary.SafeToSpendAlerts.ToString(), "Money"));
        metricsPanel.Children.Add(CreateDashboardCard("Calendar sync", summary.RealCalendarSyncEnabled ? "On" : "Off", "v5+"));

        root.Children.Add(metricsPanel);

        var rulePanel = CreateInfoPanel("Agenda/payment calendar rule", FormatReasons(summary.Reasons));
        rulePanel.Margin = new Thickness(0, 8, 0, 0);
        root.Children.Add(rulePanel);

        var controlsPanel = CreatePaymentCalendarControlsPanel();
        controlsPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(controlsPanel);

        var dateRulesPanel = CreateInfoPanel("Date trust / sync gates", FormatReasons(summary.DateRules));
        dateRulesPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(dateRulesPanel);

        var timelinePanel = CreatePaymentCalendarTimelinePanel(summary);
        timelinePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(timelinePanel);

        var todayPanel = CreatePaymentCalendarItemPanel("Today / next two days", summary.TodayAndNextActions);
        todayPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(todayPanel);

        var paymentPanel = CreatePaymentCalendarItemPanel("Payment dates / safe-to-spend lane", summary.PaymentItems);
        paymentPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(paymentPanel);

        var agendaPanel = CreatePaymentCalendarItemPanel("Agenda commitments / time lane", summary.AgendaItems);
        agendaPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(agendaPanel);

        var payLaterPanel = CreatePaymentCalendarItemPanel("Pay Later / Zip / Afterpay schedule", summary.PayLaterItems);
        payLaterPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(payLaterPanel);

        var expectedPanel = CreatePaymentCalendarItemPanel("Expected money dates excluded from safe money", summary.ExpectedMoneyItems);
        expectedPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(expectedPanel);

        var reviewPanel = CreatePaymentCalendarItemPanel("Review windows / untrusted date items", summary.ReviewQueue);
        reviewPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(reviewPanel);

        var safePanel = CreatePaymentCalendarItemPanel("Safe-to-spend alerts by date", summary.SafeToSpendItems);
        safePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(safePanel);

        var boundaryPanel = CreateInfoPanel(
            "v4.4 boundary",
            "v4.4 models agenda/payment dates locally. It does not connect Google Calendar, Outlook Calendar, Gmail/Outlook email, bank feeds, BNPL providers, accounting systems, open banking, OCR automation, OAuth, or AI actions.");

        boundaryPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(boundaryPanel);

        var nextPanel = CreateInfoPanel(
            "After v4.4",
            "Next lane: v4.5 Work Pipeline. v4.4 makes dates visible; v4.5 should push active work, leads, blocked projects, follow-ups, invoice readiness, and payment states through the same item/state spine.");

        nextPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(nextPanel);

        var storagePanel = CreateInfoPanel(
            "Local payment calendar file",
            $"Payment Calendar file: {PaymentCalendarStorage.FilePath}");

        storagePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(storagePanel);

        MainContentControl.Content = root;
    }

    private Border CreatePaymentCalendarControlsPanel()
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = "Payment calendar controls",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        root.Children.Add(new TextBlock
        {
            Text = "Use reset only for fictional/demo date items. v4.4 is local/manual date-state modelling; no real calendar, email, bank, BNPL, accounting, OCR, OAuth, or AI connector is active.",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 16)
        });

        var buttonRow = new StackPanel
        {
            Orientation = Orientation.Horizontal
        };

        var resetButton = CreateActionButton("Reset Payment Calendar Demo", Color.FromRgb(30, 41, 59), Color.FromRgb(226, 232, 240));
        resetButton.Click += ResetPaymentCalendarButton_Click;

        buttonRow.Children.Add(resetButton);
        root.Children.Add(buttonRow);

        panel.Child = root;
        return panel;
    }

    private Border CreatePaymentCalendarTimelinePanel(PaymentCalendarSummary summary)
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = "Time-aware lane",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        root.Children.Add(new TextBlock
        {
            Text = "Bills, BNPL instalments, agenda commitments, expected-money dates, and review windows are grouped by date. This is the bridge between the old paper bills workflow and future calendar integrations.",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 16)
        });

        if (summary.TimelineGroups.Count == 0)
        {
            root.Children.Add(CreateEmptyTextBlock("No dated payment/agenda items."));
            panel.Child = root;
            return panel;
        }

        foreach (var group in summary.TimelineGroups)
        {
            var groupCard = CreatePanel();
            groupCard.Margin = new Thickness(0, 8, 0, 0);

            var groupRoot = new StackPanel();

            groupRoot.Children.Add(new TextBlock
            {
                Text = $"{group.Label} • {group.Date:dd MMM yyyy} • {group.ItemCount} item(s) • {FormatMoney(group.AmountDue)} due",
                Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                FontSize = 18,
                FontWeight = FontWeights.Bold
            });

            groupRoot.Children.Add(new TextBlock
            {
                Text = $"{group.PaymentCount} payment date(s) • {group.AgendaCount} agenda/time item(s) • Review gate: {(group.HasReviewGate ? "Yes" : "No")}",
                Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 4, 0, 8)
            });

            foreach (var item in group.Items)
            {
                groupRoot.Children.Add(CreatePaymentCalendarCompactLine(item));
            }

            groupCard.Child = groupRoot;
            root.Children.Add(groupCard);
        }

        panel.Child = root;
        return panel;
    }

    private Border CreatePaymentCalendarItemPanel(string title, IEnumerable<PaymentCalendarItem> items)
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = title,
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        var list = items.ToList();

        if (list.Count == 0)
        {
            root.Children.Add(CreateEmptyTextBlock("No date items in this section."));
            panel.Child = root;
            return panel;
        }

        foreach (var item in list)
        {
            var itemCard = CreatePanel();
            itemCard.Margin = new Thickness(0, 10, 0, 0);

            var itemRoot = new StackPanel();

            var amountText = item.Amount > 0 ? $" • {FormatMoney(item.Amount)}" : string.Empty;
            var balanceText = item.Balance > 0 ? $" • Balance {FormatMoney(item.Balance)}" : string.Empty;
            var dueText = item.DueDate.HasValue ? $" • Due {item.DueDate.Value:dd MMM yyyy}" : string.Empty;
            var timeText = string.IsNullOrWhiteSpace(item.TimeText) ? string.Empty : $" • {item.TimeText}";

            itemRoot.Children.Add(new TextBlock
            {
                Text = item.Title,
                Foreground = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap
            });

            itemRoot.Children.Add(new TextBlock
            {
                Text = $"{PaymentCalendarCalculator.FormatKind(item.Kind)} • {PaymentCalendarCalculator.FormatState(item.State)} • {PaymentCalendarCalculator.FormatPressure(item.Pressure)} • {PaymentCalendarCalculator.FormatTrust(item.TrustState)}{amountText}{balanceText}{dueText}{timeText}",
                Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 6, 0, 0)
            });

            var details = new List<string>
            {
                $"Source: {item.Source}",
                $"Module: {item.OriginalModule}",
                $"Category: {item.Category}",
                $"Evidence: {item.EvidenceSummary}",
                $"Review: {item.ReviewRule}",
                $"Pressure: {item.PressureImpact}",
                $"Next: {item.NextAction}"
            };

            if (item.ExpectedMoneyExcludedFromSafe)
            {
                details.Add("Safe money rule: Expected money is visible by date but excluded from safe-to-spend.");
            }

            if (item.IsFixedCommitment)
            {
                details.Add("Time rule: Fixed commitment protects time before work is scheduled around it.");
            }

            itemRoot.Children.Add(new TextBlock
            {
                Text = string.Join(Environment.NewLine, details.Where(detail => !string.IsNullOrWhiteSpace(detail))),
                Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 8, 0, 0)
            });

            itemCard.Child = itemRoot;
            root.Children.Add(itemCard);
        }

        panel.Child = root;
        return panel;
    }

    private TextBlock CreatePaymentCalendarCompactLine(PaymentCalendarItem item)
    {
        var amountText = item.Amount > 0 ? $" • {FormatMoney(item.Amount)}" : string.Empty;
        var timeText = string.IsNullOrWhiteSpace(item.TimeText) ? string.Empty : $" • {item.TimeText}";

        return new TextBlock
        {
            Text = $"• {item.Title} — {PaymentCalendarCalculator.FormatKind(item.Kind)} • {PaymentCalendarCalculator.FormatState(item.State)} • {PaymentCalendarCalculator.FormatPressure(item.Pressure)}{amountText}{timeText} — {item.NextAction}",
            Foreground = new SolidColorBrush(Color.FromRgb(203, 213, 225)),
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 3, 0, 0)
        };
    }


    private void ShowMoneyProfilePage()
    {
        _moneyProfilePlan = MoneyProfileStorage.Load();
        var summary = MoneyProfileCalculator.Calculate(_moneyProfilePlan);

        SetHeader("Money Profile", $"Money Profile • v4.3 • {summary.HiddenDeductionCount} hidden deductions • {summary.ReviewNeededCount} review • {summary.ConfidenceLabel} confidence");

        var root = new StackPanel();

        root.Children.Add(CreateHeroPanel(
            "Money Profile / Hidden Deductions / Safe-to-Spend",
            "v4.3 strengthens the real money profile: confirmed cash, visible obligations, hidden deductions, expected-money exclusion, buffers, reserve rules, and safe-to-spend confidence before integrations."));

        var metricsPanel = new WrapPanel
        {
            Margin = new Thickness(0, 22, 0, 0)
        };

        metricsPanel.Children.Add(CreateDashboardCard("Engine", "Active", "v4.3"));
        metricsPanel.Children.Add(CreateDashboardCard("Confidence", summary.ConfidenceLabel, "Money"));
        metricsPanel.Children.Add(CreateDashboardCard("Visible balance", FormatMoney(summary.CurrentVisibleBalance), "Raw"));
        metricsPanel.Children.Add(CreateDashboardCard("Confirmed cash", FormatMoney(summary.ConfirmedCashOnHand), "Safe source"));
        metricsPanel.Children.Add(CreateDashboardCard("Known obligations", FormatMoney(summary.WeeklyKnownObligations), "This week"));
        metricsPanel.Children.Add(CreateDashboardCard("Hidden monthly", FormatMoney(summary.HiddenDeductionMonthlyEstimate), "Reserve"));
        metricsPanel.Children.Add(CreateDashboardCard("Hidden weekly", FormatMoney(summary.HiddenDeductionWeeklyReserve), "Drag"));
        metricsPanel.Children.Add(CreateDashboardCard("Minimum buffer", FormatMoney(summary.MinimumBuffer), "Protected"));
        metricsPanel.Children.Add(CreateDashboardCard("Emergency hold", FormatMoney(summary.EmergencyHold), "Protected"));
        metricsPanel.Children.Add(CreateDashboardCard("Safe before hidden", FormatMoney(summary.SafeToSpendBeforeHidden), "Pre-reserve"));
        metricsPanel.Children.Add(CreateDashboardCard("Safe after hidden", FormatMoney(summary.SafeToSpendAfterHidden), "Post-reserve"));
        metricsPanel.Children.Add(CreateDashboardCard("Final safe", FormatMoney(summary.SafeToSpendFinal), "Spendable"));
        metricsPanel.Children.Add(CreateDashboardCard("Expected money", FormatMoney(summary.ExpectedMoneyTotal), "Visible"));
        metricsPanel.Children.Add(CreateDashboardCard("Excluded expected", FormatMoney(summary.ExpectedExcludedFromSafe), "Not safe"));
        metricsPanel.Children.Add(CreateDashboardCard("Needs review", summary.ReviewNeededCount.ToString(), "Gate"));
        metricsPanel.Children.Add(CreateDashboardCard("Untrusted", summary.UntrustedCount.ToString(), "Deductions"));

        root.Children.Add(metricsPanel);

        var rulePanel = CreateInfoPanel("Money profile rule", FormatReasons(summary.Reasons));
        rulePanel.Margin = new Thickness(0, 8, 0, 0);
        root.Children.Add(rulePanel);

        var controlsPanel = CreateMoneyProfileControlsPanel();
        controlsPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(controlsPanel);

        var calculationPanel = CreateMoneyProfileCalculationPanel(summary);
        calculationPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(calculationPanel);

        var hiddenPanel = CreateMoneyProfileHiddenDeductionPanel("Hidden deductions / reserve rules", summary.HiddenDeductions);
        hiddenPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(hiddenPanel);

        var reviewPanel = CreateMoneyProfileHiddenDeductionPanel("Hidden deduction review gates", summary.ReviewDeductions);
        reviewPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(reviewPanel);

        var expectedPanel = CreateMoneyProfileExpectedMoneyPanel("Expected money excluded from safe money", summary.ExpectedMoneyItems);
        expectedPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(expectedPanel);

        var confidencePanel = CreateMoneyProfileConfidencePanel(summary);
        confidencePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(confidencePanel);

        var boundaryPanel = CreateInfoPanel(
            "v4.3 boundary",
            "v4.3 models hidden deductions, reserves, buffers, expected-money exclusion, and safe-to-spend confidence locally. It does not connect bank feeds, payslips, IRD/tax portals, accounting systems, open banking, email, OCR automation, OAuth, or AI actions.");

        boundaryPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(boundaryPanel);

        var nextPanel = CreateInfoPanel(
            "After v4.3",
            "Next lane: v4.4 Agenda + Payment Calendar. v4.3 defines safer money confidence; v4.4 puts due dates, payment dates, bills, BNPL instalments, and agenda commitments into one time-aware calendar lane.");

        nextPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(nextPanel);

        var storagePanel = CreateInfoPanel(
            "Local money profile file",
            $"Money Profile file: {MoneyProfileStorage.FilePath}");

        storagePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(storagePanel);

        MainContentControl.Content = root;
    }

    private Border CreateMoneyProfileControlsPanel()
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = "Money profile controls",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        root.Children.Add(new TextBlock
        {
            Text = "Use reset only for fictional/demo money-profile data. v4.3 is local/manual safe-to-spend modelling; no real bank, payslip, tax, accounting, open banking, email, or OCR connector is active.",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 16)
        });

        var buttonRow = new StackPanel
        {
            Orientation = Orientation.Horizontal
        };

        var resetButton = CreateActionButton("Reset Money Profile Demo", Color.FromRgb(30, 41, 59), Color.FromRgb(226, 232, 240));
        resetButton.Click += ResetMoneyProfileButton_Click;

        buttonRow.Children.Add(resetButton);
        root.Children.Add(buttonRow);

        panel.Child = root;
        return panel;
    }

    private Border CreateMoneyProfileCalculationPanel(MoneyProfileSummary summary)
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = "Safe-to-spend calculation",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        root.Children.Add(new TextBlock
        {
            Text = "This section proves the actual money rule: confirmed cash minus obligations, hidden deductions, minimum buffer, and emergency hold. Expected money is visible, but excluded.",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 12)
        });

        foreach (var line in summary.CalculationLines)
        {
            root.Children.Add(new TextBlock
            {
                Text = "• " + line,
                Foreground = new SolidColorBrush(Color.FromRgb(191, 219, 254)),
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 2, 0, 0)
            });
        }

        panel.Child = root;
        return panel;
    }

    private Border CreateMoneyProfileHiddenDeductionPanel(string title, IEnumerable<MoneyProfileHiddenDeduction> deductions)
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = title,
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        var list = deductions.ToList();

        if (list.Count == 0)
        {
            root.Children.Add(CreateEmptyTextBlock("No hidden deductions in this section."));
            panel.Child = root;
            return panel;
        }

        foreach (var item in list)
        {
            var trustedText = item.Trusted ? "Trusted" : "Untrusted";

            root.Children.Add(CreateSimpleItemCard(
                item.Title,
                $"{MoneyProfileCalculator.FormatKind(item.Kind)} • {LifeOsItemStateCalculator.FormatState(item.State)} • {trustedText} • Monthly {FormatMoney(item.MonthlyEstimate)} • Weekly {FormatMoney(item.WeeklyReserve)}",
                $"Source: {MoneyProfileCalculator.FormatSource(item.SourceKind)}\nRule: {item.RuleSummary}\nEvidence: {item.EvidenceSummary}\nReview: {item.ReviewGate}\nSafe-to-spend impact: {item.SafeToSpendImpact}\nNext: {item.NextAction}"));
        }

        panel.Child = root;
        return panel;
    }

    private Border CreateMoneyProfileExpectedMoneyPanel(string title, IEnumerable<MoneyProfileExpectedMoney> items)
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = title,
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        var list = items.ToList();

        if (list.Count == 0)
        {
            root.Children.Add(CreateEmptyTextBlock("No expected money items in this section."));
            panel.Child = root;
            return panel;
        }

        foreach (var item in list)
        {
            var trustedText = item.Trusted ? "Trusted" : "Untrusted";
            var safeText = item.CountsAsSafeMoney ? "Counts as safe" : "Excluded from safe";
            var dateText = item.ExpectedDate.HasValue ? $" • Expected {item.ExpectedDate.Value:dd MMM yyyy}" : string.Empty;

            root.Children.Add(CreateSimpleItemCard(
                item.Title,
                $"{LifeOsItemStateCalculator.FormatState(item.State)} • {trustedText} • {safeText} • {FormatMoney(item.Amount)}{dateText}",
                $"Source: {item.SourceSummary}\nEvidence: {item.EvidenceSummary}\nReview: {item.ReviewGate}\nSafe money rule: {item.SafeMoneyRule}"));
        }

        panel.Child = root;
        return panel;
    }

    private Border CreateMoneyProfileConfidencePanel(MoneyProfileSummary summary)
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = "Safe-to-spend confidence gates",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        root.Children.Add(new TextBlock
        {
            Text = $"Confidence: {summary.ConfidenceLabel}",
            Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248)),
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 6, 0, 8)
        });

        foreach (var line in summary.ConfidenceChecklist)
        {
            root.Children.Add(new TextBlock
            {
                Text = "• " + line,
                Foreground = new SolidColorBrush(Color.FromRgb(191, 219, 254)),
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 2, 0, 0)
            });
        }

        panel.Child = root;
        return panel;
    }


    private void ResetBillsPaymentsButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmRiskyAction("Reset bills/payments demo data?", "This will replace the local Bills / Payments profile with fictional/demo money obligation data."))
        {
            return;
        }

        MoneyObligationStorage.ResetToDemoData();
        _moneyObligationProfile = MoneyObligationStorage.Load();
        ShowBillsPaymentsPage();
    }

    private void ShowBillsPaymentsPage()
    {
        _moneyObligationProfile = MoneyObligationStorage.Load();
        var summary = MoneyObligationCalculator.Calculate(_moneyObligationProfile);

        SetHeader("Bills / Payments", $"Bills / Payments • v4.2 • {summary.TotalItems} obligations • {summary.RequiredThisWeekItems} this week • {summary.PayLaterItems} BNPL");

        var root = new StackPanel();

        root.Children.Add(CreateHeroPanel(
            "Bills / Upcoming Payments / Pay Later",
            "v4.2 turns bills, subscriptions, upcoming payments, Pay Later / Zip / Afterpay instalments, hidden deductions, and manual cashflow items into reviewed stateful money obligations before they affect safe-to-spend."));

        var metricsPanel = new WrapPanel
        {
            Margin = new Thickness(0, 22, 0, 0)
        };

        metricsPanel.Children.Add(CreateDashboardCard("Engine", "Active", "v4.2"));
        metricsPanel.Children.Add(CreateDashboardCard("Pressure", summary.PressureLabel, "Money"));
        metricsPanel.Children.Add(CreateDashboardCard("Obligations", summary.TotalItems.ToString(), "Tracked"));
        metricsPanel.Children.Add(CreateDashboardCard("This week", summary.RequiredThisWeekItems.ToString(), "Required"));
        metricsPanel.Children.Add(CreateDashboardCard("Due today", summary.DueTodayItems.ToString(), FormatMoney(summary.AmountDueToday)));
        metricsPanel.Children.Add(CreateDashboardCard("Due soon", summary.DueSoonItems.ToString(), "7 days"));
        metricsPanel.Children.Add(CreateDashboardCard("Overdue", summary.OverdueItems.ToString(), FormatMoney(summary.OverdueAmount)));
        metricsPanel.Children.Add(CreateDashboardCard("Needs review", summary.NeedsReviewItems.ToString(), "Gate"));
        metricsPanel.Children.Add(CreateDashboardCard("Untrusted", summary.UntrustedItems.ToString(), "Source"));
        metricsPanel.Children.Add(CreateDashboardCard("Pay later", summary.PayLaterItems.ToString(), FormatMoney(summary.PayLaterBalance)));
        metricsPanel.Children.Add(CreateDashboardCard("BNPL week", FormatMoney(summary.PayLaterDueThisWeek), "Load"));
        metricsPanel.Children.Add(CreateDashboardCard("Hidden", summary.HiddenDeductionItems.ToString(), FormatMoney(summary.HiddenDeductionMonthlyEstimate)));
        metricsPanel.Children.Add(CreateDashboardCard("Safe drag", FormatMoney(summary.SafeToSpendDrag), "This week"));
        metricsPanel.Children.Add(CreateDashboardCard("Projected safe", FormatMoney(summary.ProjectedSafeToSpendAfterObligations), "After obligations"));
        metricsPanel.Children.Add(CreateDashboardCard("Paid/closed", summary.PaidOrClosedItems.ToString(), "Evidence"));

        root.Children.Add(metricsPanel);

        var rulePanel = CreateInfoPanel("Bills/payments rule", FormatReasons(summary.Reasons));
        rulePanel.Margin = new Thickness(0, 8, 0, 0);
        root.Children.Add(rulePanel);

        var controlsPanel = CreateBillsPaymentsControlsPanel();
        controlsPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(controlsPanel);

        var duePanel = CreateMoneyObligationPanel("Due pressure / safe-to-spend impact", summary.DuePressureItems);
        duePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(duePanel);

        var billsPanel = CreateMoneyObligationPanel("Bills / subscriptions", summary.BillsAndSubscriptions);
        billsPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(billsPanel);

        var upcomingPanel = CreateMoneyObligationPanel("Upcoming payments / manual cashflow", summary.UpcomingPayments);
        upcomingPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(upcomingPanel);

        var payLaterPanel = CreateMoneyObligationPanel("Pay Later / Zip / Afterpay", summary.PayLaterItemsList);
        payLaterPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(payLaterPanel);

        var hiddenPanel = CreateMoneyObligationPanel("Money Profile / hidden deductions", summary.HiddenDeductionItemsList);
        hiddenPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(hiddenPanel);

        var reviewPanel = CreateMoneyObligationPanel("Review / evidence queue", summary.ReviewQueue);
        reviewPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(reviewPanel);

        var paidPanel = CreateMoneyObligationPanel("Paid / closed with evidence", summary.PaidOrClosedItemsList);
        paidPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(paidPanel);

        var boundaryPanel = CreateInfoPanel(
            "v4.2 boundary",
            "v4.2 models bills, upcoming payments, Pay Later / BNPL, hidden deductions, safe-to-spend drag, and payment evidence gates locally. It does not connect bank feeds, Pay Later providers, accounting systems, email, OCR automation, or OAuth yet.");

        boundaryPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(boundaryPanel);

        var nextPanel = CreateInfoPanel(
            "After v4.2",
            "Next lane: v4.3 Money Profile / Hidden Deductions / Safe-to-Spend. v4.2 defines the obligations; v4.3 turns hidden deductions and safe-to-spend calculation into a stronger money profile.");

        nextPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(nextPanel);

        var storagePanel = CreateInfoPanel(
            "Local bills/payments file",
            $"Bills / Payments file: {MoneyObligationStorage.FilePath}");

        storagePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(storagePanel);

        MainContentControl.Content = root;
    }

    private Border CreateBillsPaymentsControlsPanel()
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = "Bills / payments controls",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        root.Children.Add(new TextBlock
        {
            Text = "Use reset only for fictional/demo money obligations. v4.2 is local/manual money-state modelling; no real bank, Pay Later, accounting, email, or OCR connector is active.",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 16)
        });

        var buttonRow = new StackPanel
        {
            Orientation = Orientation.Horizontal
        };

        var resetButton = CreateActionButton("Reset Bills / Payments Demo", Color.FromRgb(30, 41, 59), Color.FromRgb(226, 232, 240));
        resetButton.Click += ResetBillsPaymentsButton_Click;

        buttonRow.Children.Add(resetButton);
        root.Children.Add(buttonRow);

        panel.Child = root;
        return panel;
    }

    private Border CreateMoneyObligationPanel(string title, IEnumerable<MoneyObligationItem> items)
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = title,
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        var list = items.ToList();

        if (list.Count == 0)
        {
            root.Children.Add(CreateEmptyTextBlock("No money obligations in this section."));
            panel.Child = root;
            return panel;
        }

        foreach (var item in list)
        {
            var dueText = item.DueDate.HasValue ? $" • Due {item.DueDate.Value:dd MMM yyyy}" : string.Empty;
            var amountText = item.AmountDue > 0 ? $" • Due {FormatMoney(item.AmountDue)}" : string.Empty;
            var balanceText = item.RemainingBalance > 0 ? $" • Balance {FormatMoney(item.RemainingBalance)}" : string.Empty;
            var monthlyText = item.MonthlyEstimate > 0 ? $" • Monthly {FormatMoney(item.MonthlyEstimate)}" : string.Empty;
            var trustedText = item.Trusted ? "Trusted" : "Untrusted";

            root.Children.Add(CreateSimpleItemCard(
                item.Title,
                $"{MoneyObligationCalculator.FormatKind(item.Kind)} • {LifeOsItemStateCalculator.FormatState(item.State)} • {MoneyObligationCalculator.FormatPressure(item.PressureLevel)} • {trustedText}{amountText}{balanceText}{monthlyText}{dueText}",
                $"Provider: {item.Provider}\nSource: {MoneyObligationCalculator.FormatSource(item.SourceKind)}\nCategory: {item.Category}\nSchedule: {item.ScheduleLabel}\nEvidence: {item.EvidenceSummary}\nEvidence state: {MoneyObligationCalculator.FormatEvidence(item.EvidenceState)}\nReview: {item.ReviewGate}\nPressure: {item.PressureSignal}\nNext: {item.SafeNextAction}"));
        }

        panel.Child = root;
        return panel;
    }


    private void ResetItemStateEngineButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmRiskyAction("Reset item/state demo data?", "This will replace the local Item State Engine profile with fictional/demo stateful items and transition rules."))
        {
            return;
        }

        LifeOsItemStateStorage.ResetToDemoData();
        _lifeOsItemStateProfile = LifeOsItemStateStorage.Load();
        ShowItemStateEnginePage();
    }

    private void ShowItemStateEnginePage()
    {
        _lifeOsItemStateProfile = LifeOsItemStateStorage.Load();
        var summary = LifeOsItemStateCalculator.Calculate(_lifeOsItemStateProfile);

        SetHeader("Item State Engine", $"Item State Engine • v4.1 • {summary.TotalItems} items • {summary.NeedsReviewItems} review • {summary.TransitionRules} transition rules");

        var root = new StackPanel();

        root.Children.Add(CreateHeroPanel(
            "Item Type / State Engine",
            "v4.1 turns the LifeOS spine map into a working item/state layer. Bills, payments, receipts, invoices, work, follow-ups, agenda items, proof, and future integrations all become reviewed items with state, evidence, pressure impact, and safe next action."));

        var metricsPanel = new WrapPanel
        {
            Margin = new Thickness(0, 22, 0, 0)
        };

        metricsPanel.Children.Add(CreateDashboardCard("Engine", summary.ItemStateEngineActive ? "Active" : "Off", "v4.1"));
        metricsPanel.Children.Add(CreateDashboardCard("Items", summary.TotalItems.ToString(), "Demo"));
        metricsPanel.Children.Add(CreateDashboardCard("Open", summary.OpenItems.ToString(), "State"));
        metricsPanel.Children.Add(CreateDashboardCard("Review", summary.NeedsReviewItems.ToString(), "Gate"));
        metricsPanel.Children.Add(CreateDashboardCard("Due", summary.DueSoonOrTodayItems.ToString(), "Soon/today"));
        metricsPanel.Children.Add(CreateDashboardCard("Overdue", summary.OverdueItems.ToString(), "Risk"));
        metricsPanel.Children.Add(CreateDashboardCard("Waiting", summary.WaitingItems.ToString(), "Do not chase"));
        metricsPanel.Children.Add(CreateDashboardCard("Untrusted", summary.UntrustedItems.ToString(), "Needs source"));
        metricsPanel.Children.Add(CreateDashboardCard("Money", summary.MoneyImpactItems.ToString(), "Impact"));
        metricsPanel.Children.Add(CreateDashboardCard("Safe spend", summary.SafeToSpendImpactItems.ToString(), "Affected"));
        metricsPanel.Children.Add(CreateDashboardCard("Agenda", summary.AgendaImpactItems.ToString(), "Affected"));
        metricsPanel.Children.Add(CreateDashboardCard("Rules", summary.TransitionRules.ToString(), "Transitions"));

        root.Children.Add(metricsPanel);

        var rulePanel = CreateInfoPanel("Item/state rule", FormatReasons(summary.Reasons));
        rulePanel.Margin = new Thickness(0, 8, 0, 0);
        root.Children.Add(rulePanel);

        var controlsPanel = CreateItemStateEngineControlsPanel();
        controlsPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(controlsPanel);

        var reviewPanel = CreateLifeOsItemPanel("Review queue / untrusted items", summary.ReviewQueue);
        reviewPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(reviewPanel);

        var pressurePanel = CreateLifeOsItemPanel("Command Centre pressure items", summary.PressureItems);
        pressurePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(pressurePanel);

        var moneyPanel = CreateLifeOsItemPanel("Money-impact items", summary.MoneyItems);
        moneyPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(moneyPanel);

        var workPanel = CreateLifeOsItemPanel("Work / people / proof items", summary.WorkItems);
        workPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(workPanel);

        var transitionPanel = CreateLifeOsTransitionRulePanel("State transition rules", summary.Rules);
        transitionPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(transitionPanel);

        var boundaryPanel = CreateInfoPanel(
            "v4.1 boundary",
            "v4.1 models item state and transition rules. It does not connect real APIs, request OAuth permissions, sync live email/calendar/accounting/bank data, run AI actions, or automatically change real-world systems.");

        boundaryPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(boundaryPanel);

        var nextPanel = CreateInfoPanel(
            "After v4.1",
            "Next lane: v4.2 Bills / Upcoming Payments / Pay Later. The item engine created here becomes the base for bill items, upcoming payment items, BNPL items, safe-to-spend impact, agenda impact, and weekly close-out pressure.");

        nextPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(nextPanel);

        var storagePanel = CreateInfoPanel(
            "Local item state file",
            $"Item State Engine file: {LifeOsItemStateStorage.FilePath}");

        storagePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(storagePanel);

        MainContentControl.Content = root;
    }

    private Border CreateItemStateEngineControlsPanel()
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = "Item engine controls",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        root.Children.Add(new TextBlock
        {
            Text = "Use reset only for fictional/demo item-state data. v4.1 is local state modelling; no real integrations or automatic actions are active.",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 16)
        });

        var buttonRow = new StackPanel
        {
            Orientation = Orientation.Horizontal
        };

        var resetButton = CreateActionButton("Reset Item State Demo", Color.FromRgb(30, 41, 59), Color.FromRgb(226, 232, 240));
        resetButton.Click += ResetItemStateEngineButton_Click;

        buttonRow.Children.Add(resetButton);
        root.Children.Add(buttonRow);

        panel.Child = root;
        return panel;
    }

    private Border CreateLifeOsItemPanel(string title, IEnumerable<LifeOsStatefulItem> items)
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = title,
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        var list = items.ToList();

        if (list.Count == 0)
        {
            root.Children.Add(CreateEmptyTextBlock("No items in this section."));
            panel.Child = root;
            return panel;
        }

        foreach (var item in list)
        {
            var dueText = item.DueDate.HasValue ? $" • Due {item.DueDate.Value:dd MMM yyyy}" : string.Empty;
            var amountText = item.Amount.HasValue ? $" • {item.Amount.Value:C}" : string.Empty;
            var trustedText = item.Trusted ? "Trusted" : "Untrusted";

            root.Children.Add(CreateSimpleItemCard(
                item.Title,
                $"{LifeOsItemStateCalculator.FormatType(item.Type)} • {LifeOsItemStateCalculator.FormatState(item.State)} • {LifeOsItemStateCalculator.FormatRisk(item.RiskLevel)} • {trustedText}{amountText}{dueText}",
                $"Source: {LifeOsItemStateCalculator.FormatSource(item.SourceKind)}\nImpacts: {LifeOsItemStateCalculator.FormatImpactAreas(item.ImpactAreas)}\nEvidence: {item.EvidenceSummary}\nReview: {item.ReviewGate}\nPressure: {item.PressureSignal}\nNext: {item.SafeNextAction}"));
        }

        panel.Child = root;
        return panel;
    }

    private Border CreateLifeOsTransitionRulePanel(string title, IEnumerable<LifeOsStateTransitionRule> rules)
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = title,
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        var list = rules.ToList();

        if (list.Count == 0)
        {
            root.Children.Add(CreateEmptyTextBlock("No transition rules mapped yet."));
            panel.Child = root;
            return panel;
        }

        foreach (var rule in list)
        {
            var flags = new List<string>();

            if (rule.RequiresEvidence)
            {
                flags.Add("evidence required");
            }

            if (rule.RequiresManualReview)
            {
                flags.Add("manual review");
            }

            if (rule.IsDestructive)
            {
                flags.Add("destructive");
            }

            var flagText = flags.Count == 0 ? "no special flags" : string.Join(", ", flags);

            root.Children.Add(CreateSimpleItemCard(
                rule.Label,
                $"{LifeOsItemStateCalculator.FormatType(rule.Type)} • {LifeOsItemStateCalculator.FormatState(rule.FromState)} → {LifeOsItemStateCalculator.FormatState(rule.ToState)} • {flagText}",
                $"Requirement: {rule.Requirement}\nPressure result: {rule.ResultingPressure}"));
        }

        panel.Child = root;
        return panel;
    }


    private void ResetLifeOsSpineButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmRiskyAction("Reset LifeOS spine demo data?", "This will replace the local LifeOS Spine Map with fictional/canon roadmap demo data."))
        {
            return;
        }

        LifeOsSpineStorage.ResetToDemoData();
        _lifeOsSpineProfile = LifeOsSpineStorage.Load();
        ShowLifeOsSpinePage();
    }

    private void ShowLifeOsSpinePage()
    {
        _lifeOsSpineProfile = LifeOsSpineStorage.Load();
        var summary = LifeOsSpineCalculator.Calculate(_lifeOsSpineProfile);

        SetHeader("LifeOS Spine Map", $"LifeOS Spine Map • v4.0 • {summary.ModuleCount} modules • {summary.ItemTypesCovered} item types • {summary.PressureSourceCount} pressure sources");

        var root = new StackPanel();

        root.Children.Add(CreateHeroPanel(
            "LifeOS Spine Recovery Map",
            "v4.0 restores the original LifeOS operating spine before integrations. Every important real-world input becomes a reviewed item with state, evidence, timeline/context, pressure impact, and a Command Centre signal."));

        var metricsPanel = new WrapPanel
        {
            Margin = new Thickness(0, 22, 0, 0)
        };

        metricsPanel.Children.Add(CreateDashboardCard("Road", "v4 spine", "Before v5"));
        metricsPanel.Children.Add(CreateDashboardCard("Modules", summary.ModuleCount.ToString(), "Mapped"));
        metricsPanel.Children.Add(CreateDashboardCard("Canon", summary.CanonModules.ToString(), "Original"));
        metricsPanel.Children.Add(CreateDashboardCard("Active", summary.ActiveModules.ToString(), "Existing"));
        metricsPanel.Children.Add(CreateDashboardCard("Needs model", summary.NeedsModelModules.ToString(), "v4 work"));
        metricsPanel.Children.Add(CreateDashboardCard("Required v4", summary.RequiredForV4Modules.ToString(), "Spine"));
        metricsPanel.Children.Add(CreateDashboardCard("Item rules", summary.ItemRuleCount.ToString(), "Stateful"));
        metricsPanel.Children.Add(CreateDashboardCard("Item types", summary.ItemTypesCovered.ToString(), "Covered"));
        metricsPanel.Children.Add(CreateDashboardCard("States", summary.StateCount.ToString(), "Common"));
        metricsPanel.Children.Add(CreateDashboardCard("Pressure", summary.PressureSourceCount.ToString(), "Sources"));
        metricsPanel.Children.Add(CreateDashboardCard("Integrations", summary.IntegrationsDeferredToV5 ? "v5" : "v4", "Deferred"));
        metricsPanel.Children.Add(CreateDashboardCard("Companion", summary.CompanionAppDeferredToV65 ? "v6.5" : "Earlier", "Mobile arm"));

        root.Children.Add(metricsPanel);

        var rulePanel = CreateInfoPanel("Master spine rule", FormatReasons(summary.Reasons));
        rulePanel.Margin = new Thickness(0, 8, 0, 0);
        root.Children.Add(rulePanel);

        var controlsPanel = CreateLifeOsSpineControlsPanel();
        controlsPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(controlsPanel);

        var modulesPanel = CreateLifeOsSpineModulePanel("LifeOS operating spine", summary.CoreModules);
        modulesPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(modulesPanel);

        var itemPanel = CreateLifeOsItemRulePanel("Item type / state model", summary.ItemRules);
        itemPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(itemPanel);

        var pressurePanel = CreateLifeOsPressureSourcePanel("Weekly pressure sources", summary.PressureSources);
        pressurePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(pressurePanel);

        var boundaryPanel = CreateInfoPanel(
            "v4.0 boundary",
            "v4.0 maps the LifeOS operating spine. It does not connect real APIs, request OAuth permissions, sync email/calendar/accounting/bank data, run AI actions, build the companion app, or do the major workspace redesign.");

        boundaryPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(boundaryPanel);

        var nextPanel = CreateInfoPanel(
            "v4 road",
            "v4.1 Item Type / State Engine → v4.2 Bills / Upcoming Payments / Pay Later → v4.3 Money Profile / Hidden Deductions / Safe-to-Spend → v4.4 Agenda + Payment Calendar → v4.5 Work Pipeline → v4.6 Receipt OCR / Evidence-to-Item → v4.7 Weekly Close-Out → v4.8 Command Centre Pressure Engine → v4.9 Integration Inbox + v5 readiness.");

        nextPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(nextPanel);

        var storagePanel = CreateInfoPanel(
            "Local spine map file",
            $"LifeOS Spine Map file: {LifeOsSpineStorage.FilePath}");

        storagePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(storagePanel);

        MainContentControl.Content = root;
    }

    private Border CreateLifeOsSpineControlsPanel()
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = "Spine controls",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        root.Children.Add(new TextBlock
        {
            Text = "Use reset only for fictional/canon roadmap demo data. v4.0 is a map, not a live integration layer.",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 16)
        });

        var buttonRow = new StackPanel
        {
            Orientation = Orientation.Horizontal
        };

        var resetButton = CreateActionButton("Reset Spine Map Demo", Color.FromRgb(30, 41, 59), Color.FromRgb(226, 232, 240));
        resetButton.Click += ResetLifeOsSpineButton_Click;

        buttonRow.Children.Add(resetButton);
        root.Children.Add(buttonRow);

        panel.Child = root;
        return panel;
    }

    private Border CreateLifeOsSpineModulePanel(string title, IEnumerable<LifeOsSpineModule> modules)
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = title,
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        var list = modules.ToList();

        if (list.Count == 0)
        {
            root.Children.Add(CreateEmptyTextBlock("No spine modules mapped yet."));
            panel.Child = root;
            return panel;
        }

        foreach (var module in list)
        {
            root.Children.Add(CreateSimpleItemCard(
                module.Name,
                $"{LifeOsSpineCalculator.FormatArea(module.Area)} • {LifeOsSpineCalculator.FormatStatus(module.Status)} • Priority {module.Priority} • {(module.RequiredForV4 ? "Required for v4" : "Optional")}",
                $"Purpose: {module.Purpose}\nConnects: {module.ConnectsTo}\nCommand signal: {module.CommandCentreSignal}\nBoundary: {module.Boundary}"));
        }

        panel.Child = root;
        return panel;
    }

    private Border CreateLifeOsItemRulePanel(string title, IEnumerable<LifeOsStatefulItemRule> rules)
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = title,
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        var list = rules.ToList();

        if (list.Count == 0)
        {
            root.Children.Add(CreateEmptyTextBlock("No item/state rules mapped yet."));
            panel.Child = root;
            return panel;
        }

        foreach (var rule in list)
        {
            root.Children.Add(CreateSimpleItemCard(
                rule.Name,
                $"{LifeOsSpineCalculator.FormatItemType(rule.ItemType)} • {(rule.RequiredForV4 ? "Required for v4" : "Optional")} • {LifeOsSpineCalculator.FormatStates(rule.AllowedStates.Take(6))}...",
                $"Sources: {rule.SourceExamples}\nEvidence: {rule.EvidenceRule}\nPressure: {rule.PressureRule}\nLanding: {rule.LandingRule}\nReview: {rule.ReviewGate}"));
        }

        panel.Child = root;
        return panel;
    }

    private Border CreateLifeOsPressureSourcePanel(string title, IEnumerable<LifeOsPressureSource> sources)
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = title,
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        var list = sources.ToList();

        if (list.Count == 0)
        {
            root.Children.Add(CreateEmptyTextBlock("No pressure sources mapped yet."));
            panel.Child = root;
            return panel;
        }

        foreach (var source in list)
        {
            root.Children.Add(CreateSimpleItemCard(
                source.Name,
                $"{LifeOsSpineCalculator.FormatArea(source.Area)} • {LifeOsSpineCalculator.FormatImpact(source.ImpactLevel)} pressure",
                $"Source items: {source.SourceItems}\nQuestion: {source.PressureQuestion}\nCommand signal: {source.CommandCentreSignal}\nSafe next action: {source.SafeNextAction}"));
        }

        panel.Child = root;
        return panel;
    }


    private void ResetFinalOfflineOsButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmRiskyAction("Reset final offline OS demo data?", "This will replace the local Final Offline OS profile with fictional checkpoint and integration landing-zone data."))
        {
            return;
        }

        FinalOfflineOsStorage.ResetToDemoData();
        _finalOfflineOsProfile = FinalOfflineOsStorage.Load();
        ShowFinalOfflineOsPage();
    }

    private void ShowFinalOfflineOsPage()
    {
        _finalOfflineOsProfile = FinalOfflineOsStorage.Load();
        var summary = FinalOfflineOsCalculator.Calculate(_finalOfflineOsProfile);

        SetHeader("Final Offline OS", $"Final Offline OS • v3.9 • {summary.ReadyCheckpoints}/{summary.TotalCheckpoints} checkpoints ready • {summary.LandingZones} v4 landing zones");

        var root = new StackPanel();

        root.Children.Add(CreateHeroPanel(
            "Final Offline OS",
            "Close the local-first foundation before v4 integrations. Confirm that modules, local storage, safety rules, proof boundaries, docs/screenshots, and integration landing zones are strong enough to accept real external data without ripping the app apart."));

        var metricsPanel = new WrapPanel
        {
            Margin = new Thickness(0, 22, 0, 0)
        };

        metricsPanel.Children.Add(CreateDashboardCard("Offline OS", summary.LocalFirstComplete ? "Complete" : "Review", "v3.9"));
        metricsPanel.Children.Add(CreateDashboardCard("v4 ready", summary.ReadyForV4Integrations ? "Yes" : "No", "Integrations"));
        metricsPanel.Children.Add(CreateDashboardCard("Checkpoints", summary.TotalCheckpoints.ToString(), "Total"));
        metricsPanel.Children.Add(CreateDashboardCard("Ready", summary.ReadyCheckpoints.ToString(), "Local"));
        metricsPanel.Children.Add(CreateDashboardCard("Needs review", summary.ReviewNeededCheckpoints.ToString(), "Before tag"));
        metricsPanel.Children.Add(CreateDashboardCard("Required for v4", summary.RequiredForV4.ToString(), "Gate"));
        metricsPanel.Children.Add(CreateDashboardCard("Areas", summary.AreaCount.ToString(), "Covered"));
        metricsPanel.Children.Add(CreateDashboardCard("Landing zones", summary.LandingZones.ToString(), "v4 map"));
        metricsPanel.Children.Add(CreateDashboardCard("Planned v4", summary.PlannedForV4.ToString(), "Not active"));
        metricsPanel.Children.Add(CreateDashboardCard("External sync", summary.ExternalIntegrationsEnabled ? "On" : "Off", "v4+"));
        metricsPanel.Children.Add(CreateDashboardCard("AI assistant", summary.AiAssistantEnabled ? "On" : "Off", "v7+"));
        metricsPanel.Children.Add(CreateDashboardCard("UI reshape", summary.MajorUiReshapeDeferred ? "Deferred" : "Now", "After proof"));

        root.Children.Add(metricsPanel);

        var rulePanel = CreateInfoPanel("Final offline OS rule", FormatReasons(summary.Reasons));
        rulePanel.Margin = new Thickness(0, 8, 0, 0);
        root.Children.Add(rulePanel);

        var controlsPanel = CreateFinalOfflineOsControlsPanel();
        controlsPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(controlsPanel);

        var requiredPanel = CreateOfflineOsCheckpointPanel("Required offline checkpoints", summary.RequiredCheckpoints);
        requiredPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(requiredPanel);

        var reviewPanel = CreateOfflineOsCheckpointPanel("Final review before v3.9 tag", summary.ReviewItems);
        reviewPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(reviewPanel);

        var landingPanel = CreateIntegrationLandingZonePanel("v4 integration landing zones", summary.IntegrationLandingZones);
        landingPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(landingPanel);

        var boundaryPanel = CreateInfoPanel(
            "v3.9 boundary",
            "v3.9 is the final offline/local-first OS foundation. It is not real Outlook/Gmail, calendar, SharePoint/Drive, accounting, AI assistant, automatic messaging, cloud sync, team/business scaling, or the major workspace redesign. Those begin after this checkpoint.");

        boundaryPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(boundaryPanel);

        var nextPanel = CreateInfoPanel(
            "After v3.9",
            "Next lane: v4.x integrations. Start by wiring one integration source into its landing module and the Universal Spine, keep manual review gates on, and do not bypass money/proof/safety boundaries.");

        nextPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(nextPanel);

        var storagePanel = CreateInfoPanel(
            "Local final OS file",
            $"Final Offline OS file: {FinalOfflineOsStorage.FilePath}");

        storagePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(storagePanel);

        MainContentControl.Content = root;
    }

    private Border CreateFinalOfflineOsControlsPanel()
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = "Final OS controls",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        root.Children.Add(new TextBlock
        {
            Text = "Use reset only for fictional/demo final OS data. v3.9 closes the offline foundation and prepares integration landing zones; it does not turn integrations or AI on.",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 16)
        });

        var buttonRow = new StackPanel
        {
            Orientation = Orientation.Horizontal
        };

        var resetButton = CreateActionButton("Reset Demo Final OS", Color.FromRgb(30, 41, 59), Color.FromRgb(226, 232, 240));
        resetButton.Click += ResetFinalOfflineOsButton_Click;

        buttonRow.Children.Add(resetButton);
        root.Children.Add(buttonRow);

        panel.Child = root;
        return panel;
    }

    private Border CreateOfflineOsCheckpointPanel(string title, IEnumerable<OfflineOsCheckpoint> checkpoints)
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = title,
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        var list = checkpoints.ToList();

        if (list.Count == 0)
        {
            root.Children.Add(CreateEmptyTextBlock("No checkpoints in this section."));
            panel.Child = root;
            return panel;
        }

        foreach (var checkpoint in list)
        {
            root.Children.Add(CreateSimpleItemCard(
                checkpoint.Title,
                $"{FinalOfflineOsCalculator.FormatArea(checkpoint.Area)} • {FinalOfflineOsCalculator.FormatStatus(checkpoint.Status)} • Priority {checkpoint.Priority} • {(checkpoint.RequiredForV4 ? "Required for v4" : "Optional")}",
                $"Evidence: {checkpoint.Evidence}\nNext: {checkpoint.NextAction}\nBoundary: {checkpoint.Boundary}"));
        }

        panel.Child = root;
        return panel;
    }

    private Border CreateIntegrationLandingZonePanel(string title, IEnumerable<IntegrationLandingZone> zones)
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = title,
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        var list = zones.ToList();

        if (list.Count == 0)
        {
            root.Children.Add(CreateEmptyTextBlock("No integration landing zones mapped yet."));
            panel.Child = root;
            return panel;
        }

        foreach (var zone in list)
        {
            root.Children.Add(CreateSimpleItemCard(
                zone.SourceName,
                $"{FinalOfflineOsCalculator.FormatLandingZoneType(zone.ZoneType)} • {FinalOfflineOsCalculator.FormatStatus(zone.Status)} • {zone.TargetModule}",
                $"Spine: {zone.SpineConnection}\nSafety: {zone.SafetyRule}\n{zone.Notes}"));
        }

        panel.Child = root;
        return panel;
    }


    private void ResetSearchKnowledgeButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmRiskyAction("Reset search/knowledge demo data?", "This will replace the local Search / Knowledge profile with fictional demo items and profiles."))
        {
            return;
        }

        SearchKnowledgeStorage.ResetToDemoData();
        _searchKnowledgeProfile = SearchKnowledgeStorage.Load();
        ShowSearchKnowledgePage();
    }

    private void ShowSearchKnowledgePage()
    {
        _searchKnowledgeProfile = SearchKnowledgeStorage.Load();
        var summary = SearchKnowledgeCalculator.Calculate(_searchKnowledgeProfile);

        SetHeader("Search / Knowledge Centre", $"Search / Knowledge Centre • v3.5 • {summary.TotalItems} local items • {summary.ProfileCount} search profiles");

        var root = new StackPanel();

        root.Children.Add(CreateHeroPanel(
            "Search / Knowledge Centre",
            "Build the local knowledge and search shape before integrations or AI reasoning. Capture what should be searchable, how search should be scoped, what must be reviewed, and which future sources can feed the app later."));

        var metricsPanel = new WrapPanel
        {
            Margin = new Thickness(0, 22, 0, 0)
        };

        metricsPanel.Children.Add(CreateDashboardCard("Knowledge items", summary.TotalItems.ToString(), "Local"));
        metricsPanel.Children.Add(CreateDashboardCard("Active", summary.ActiveItems.ToString(), "Usable"));
        metricsPanel.Children.Add(CreateDashboardCard("Needs review", summary.ReviewNeededItems.ToString(), "Manual"));
        metricsPanel.Children.Add(CreateDashboardCard("Planned", summary.PlannedItems.ToString(), "Future"));
        metricsPanel.Children.Add(CreateDashboardCard("Search profiles", summary.ProfileCount.ToString(), "Scoped"));
        metricsPanel.Children.Add(CreateDashboardCard("Pinned profiles", summary.PinnedProfiles.ToString(), "Fast"));
        metricsPanel.Children.Add(CreateDashboardCard("Manual profiles", summary.ManualProfiles.ToString(), "Review"));
        metricsPanel.Children.Add(CreateDashboardCard("Sources", summary.SourceCount.ToString(), "Types"));
        metricsPanel.Children.Add(CreateDashboardCard("Local index", summary.LocalIndexOnly ? "Only" : "Mixed", "v3.5"));
        metricsPanel.Children.Add(CreateDashboardCard("External search", summary.ExternalSearchEnabled ? "On" : "Off", "Later"));
        metricsPanel.Children.Add(CreateDashboardCard("AI reasoning", summary.AiReasoningEnabled ? "On" : "Off", "Later"));
        metricsPanel.Children.Add(CreateDashboardCard("Integrations", summary.IntegrationSourcesEnabled ? "On" : "Off", "v4+"));

        root.Children.Add(metricsPanel);

        var rulePanel = CreateInfoPanel("Search / Knowledge rule", FormatReasons(summary.Reasons));
        rulePanel.Margin = new Thickness(0, 8, 0, 0);
        root.Children.Add(rulePanel);

        var controlsPanel = CreateSearchKnowledgeControlsPanel();
        controlsPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(controlsPanel);

        var profilesPanel = CreateSearchProfilePanel("Pinned/local search profiles", summary.Profiles);
        profilesPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(profilesPanel);

        var priorityPanel = CreateKnowledgeItemPanel("Knowledge items needing review/planning", summary.PriorityItems);
        priorityPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(priorityPanel);

        var recentPanel = CreateKnowledgeItemPanel("Recent local knowledge items", summary.RecentItems);
        recentPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(recentPanel);

        var boundaryPanel = CreateInfoPanel(
            "v3.5 boundary",
            "v3.5 is local Search / Knowledge structure only. It is not external search, full-text indexing, cloud sync, connector search, inbox scanning, AI reasoning, automatic summarisation, or final knowledge automation.");

        boundaryPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(boundaryPanel);

        var storagePanel = CreateInfoPanel(
            "Local knowledge file",
            $"Search / Knowledge file: {SearchKnowledgeStorage.FilePath}");

        storagePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(storagePanel);

        MainContentControl.Content = root;
    }

    private Border CreateSearchKnowledgeControlsPanel()
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = "Search / Knowledge controls",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        root.Children.Add(new TextBlock
        {
            Text = "Use reset only for fictional/demo search and knowledge data. v3.5 proves local scoping and review rules before external search, integrations, or AI assistant reasoning.",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 16)
        });

        var buttonRow = new StackPanel
        {
            Orientation = Orientation.Horizontal
        };

        var resetButton = CreateActionButton("Reset Demo Knowledge", Color.FromRgb(30, 41, 59), Color.FromRgb(226, 232, 240));
        resetButton.Click += ResetSearchKnowledgeButton_Click;

        buttonRow.Children.Add(resetButton);
        root.Children.Add(buttonRow);

        panel.Child = root;
        return panel;
    }

    private Border CreateSearchProfilePanel(string title, IEnumerable<KnowledgeSearchProfile> profiles)
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = title,
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        var list = profiles.ToList();

        if (list.Count == 0)
        {
            root.Children.Add(CreateEmptyTextBlock("No local search profiles captured yet."));
            panel.Child = root;
            return panel;
        }

        foreach (var profile in list)
        {
            root.Children.Add(CreateSimpleItemCard(
                profile.Name,
                $"{(profile.IsPinned ? "Pinned" : "Standard")} • {(profile.IsManualOnly ? "Manual review" : "Automated")} • External index {(profile.UsesExternalIndex ? "on" : "off")}",
                $"Query hint: {profile.QueryHint}\nScope: {profile.Scope}\n{profile.Notes}"));
        }

        panel.Child = root;
        return panel;
    }

    private Border CreateKnowledgeItemPanel(string title, IEnumerable<KnowledgeItem> items)
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = title,
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        var list = items.ToList();

        if (list.Count == 0)
        {
            root.Children.Add(CreateEmptyTextBlock("No knowledge items in this section."));
            panel.Child = root;
            return panel;
        }

        foreach (var item in list)
        {
            root.Children.Add(CreateSimpleItemCard(
                item.Title,
                $"{SearchKnowledgeCalculator.FormatKind(item.Kind)} • {SearchKnowledgeCalculator.FormatStatus(item.Status)} • {SearchKnowledgeCalculator.FormatSourceType(item.SourceType)} • {item.SourceModule}",
                $"{item.Summary}\nNext: {item.NextAction}\nSearch terms: {item.SearchText}"));
        }

        panel.Child = root;
        return panel;
    }


    private void ResetOsNavigationButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmRiskyAction("Reset OS navigation demo data?", "This will replace the local OS navigation profile with fictional demo modules and groups."))
        {
            return;
        }

        OsNavigationStorage.ResetToDemoData();
        _osNavigationProfile = OsNavigationStorage.Load();
        ShowOsNavigationPage();
    }

    private void ShowOsNavigationPage()
    {
        _osNavigationProfile = OsNavigationStorage.Load();
        var summary = OsNavigationCalculator.Calculate(_osNavigationProfile);
        var moduleLookup = _osNavigationProfile.Modules.ToDictionary(module => module.Id);

        SetHeader("OS Navigation Centre", $"OS Navigation Centre • v3.0 • {summary.VisibleModules} visible modules • {summary.GroupCount} groups");

        var root = new StackPanel();

        root.Children.Add(CreateHeroPanel(
            "OS Navigation Centre",
            "Strengthen the offline OS shell without doing the later full workspace redesign yet. Keep the current WPF page model, protect core modules, group the growing navigation, and make the next operating areas obvious."));

        var metricsPanel = new WrapPanel
        {
            Margin = new Thickness(0, 22, 0, 0)
        };

        metricsPanel.Children.Add(CreateDashboardCard("Visible modules", summary.VisibleModules.ToString(), "Navigation"));
        metricsPanel.Children.Add(CreateDashboardCard("Core modules", summary.CoreModules.ToString(), "Protected"));
        metricsPanel.Children.Add(CreateDashboardCard("Pinned", summary.PinnedModules.ToString(), "Fast access"));
        metricsPanel.Children.Add(CreateDashboardCard("Groups", summary.GroupCount.ToString(), "Workspaces"));
        metricsPanel.Children.Add(CreateDashboardCard("Primary groups", summary.PrimaryWorkspaceCount.ToString(), "Operating"));
        metricsPanel.Children.Add(CreateDashboardCard("Needs review", summary.ReviewNeededModules.ToString(), "Modules"));
        metricsPanel.Children.Add(CreateDashboardCard("Planned", summary.PlannedModules.ToString(), "Future"));
        metricsPanel.Children.Add(CreateDashboardCard("Hidden", summary.HiddenModules.ToString(), "Not shown"));
        metricsPanel.Children.Add(CreateDashboardCard("Sidebar scroll", summary.SidebarScrollEnabled ? "On" : "Off", "Reachable"));
        metricsPanel.Children.Add(CreateDashboardCard("UI reshape", summary.MajorUiReshapeDeferred ? "Deferred" : "Now", "Roadmap"));
        metricsPanel.Children.Add(CreateDashboardCard("Sync", summary.ExternalIntegrationsEnabled ? "On" : "Off", "Local-first"));

        root.Children.Add(metricsPanel);

        var rulePanel = CreateInfoPanel("OS navigation rule", FormatReasons(summary.Reasons));
        rulePanel.Margin = new Thickness(0, 8, 0, 0);
        root.Children.Add(rulePanel);

        var controlsPanel = CreateOsNavigationControlsPanel();
        controlsPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(controlsPanel);

        var pinnedPanel = CreateOsNavigationModulePanel("Pinned/core operating modules", summary.Pinned);
        pinnedPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(pinnedPanel);

        var groupPanel = CreateOsNavigationGroupPanel("Workspace group map", summary.Groups, moduleLookup);
        groupPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(groupPanel);

        var reviewPanel = CreateOsNavigationModulePanel("Modules needing v3 review", summary.ReviewNeeded);
        reviewPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(reviewPanel);

        var plannedPanel = CreateOsNavigationModulePanel("Planned/future modules", summary.Planned);
        plannedPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(plannedPanel);

        var boundaryPanel = CreateInfoPanel(
            "v3.0 boundary",
            "v3.0 strengthens OS navigation and core modules. It is not the major workspace redesign, universal search, advanced knowledge layer, integrations, cloud sync, AI assistant execution, or automatic decision-making.");

        boundaryPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(boundaryPanel);

        var storagePanel = CreateInfoPanel(
            "Local navigation file",
            $"OS navigation file: {OsNavigationStorage.FilePath}");

        storagePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(storagePanel);

        MainContentControl.Content = root;
    }

    private Border CreateOsNavigationControlsPanel()
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = "Navigation controls",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        root.Children.Add(new TextBlock
        {
            Text = "Use reset only for fictional/demo navigation data. v3.0 keeps the existing page model and improves module organisation before later integrations and UI reshape.",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 16)
        });

        var buttonRow = new StackPanel
        {
            Orientation = Orientation.Horizontal
        };

        var resetButton = CreateActionButton("Reset Demo Navigation", Color.FromRgb(30, 41, 59), Color.FromRgb(226, 232, 240));
        resetButton.Click += ResetOsNavigationButton_Click;

        buttonRow.Children.Add(resetButton);
        root.Children.Add(buttonRow);

        panel.Child = root;
        return panel;
    }

    private Border CreateOsNavigationModulePanel(string title, IEnumerable<OsNavigationModule> modules)
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = title,
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        var list = modules.ToList();

        if (list.Count == 0)
        {
            root.Children.Add(CreateEmptyTextBlock("No modules in this section."));
            panel.Child = root;
            return panel;
        }

        foreach (var module in list)
        {
            root.Children.Add(CreateSimpleItemCard(
                module.Title,
                $"{OsNavigationCalculator.FormatCategory(module.Category)} • {OsNavigationCalculator.FormatStatus(module.Status)} • Sort {module.SortOrder}",
                $"{module.Purpose}\nNext: {module.NextAction}"));
        }

        panel.Child = root;
        return panel;
    }

    private Border CreateOsNavigationGroupPanel(string title, IEnumerable<OsNavigationGroup> groups, IReadOnlyDictionary<Guid, OsNavigationModule> moduleLookup)
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = title,
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        var list = groups.ToList();

        if (list.Count == 0)
        {
            root.Children.Add(CreateEmptyTextBlock("No workspace groups captured yet."));
            panel.Child = root;
            return panel;
        }

        foreach (var group in list)
        {
            var moduleNames = group.ModuleIds
                .Select(moduleId => moduleLookup.TryGetValue(moduleId, out var module) ? module.Title : "Unknown module")
                .ToList();

            root.Children.Add(CreateSimpleItemCard(
                group.Title,
                $"{OsNavigationCalculator.FormatCategory(group.Category)} • {(group.IsPrimaryWorkspace ? "Primary" : "Planned")} • {moduleNames.Count} module(s)",
                $"{group.Purpose}\nModules: {string.Join(", ", moduleNames)}"));
        }

        panel.Child = root;
        return panel;
    }


    private void ResetUniversalSpineButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmRiskyAction("Reset universal spine demo data?", "This will replace the local Universal Spine profile with fictional demo links and items."))
        {
            return;
        }

        UniversalSpineStorage.ResetToDemoData();
        _universalSpineProfile = UniversalSpineStorage.Load();
        ShowUniversalSpinePage();
    }

    private void ShowUniversalSpinePage()
    {
        _universalSpineProfile = UniversalSpineStorage.Load();
        var summary = UniversalSpineCalculator.Calculate(_universalSpineProfile);
        var itemLookup = _universalSpineProfile.Items.ToDictionary(item => item.Id);

        SetHeader("Universal Spine", $"Universal Spine • v2.1 • {summary.TotalItems} local items • {summary.LinkCount} links");

        var root = new StackPanel();

        root.Children.Add(CreateHeroPanel(
            "Universal Spine",
            "Connect the offline modules without pretending LifeOS has integrations yet. The spine records local links between work, money, proof, relationships, daily flow, evidence, release state, safety, and knowledge."));

        var metricsPanel = new WrapPanel
        {
            Margin = new Thickness(0, 22, 0, 0)
        };

        metricsPanel.Children.Add(CreateDashboardCard("Spine items", summary.TotalItems.ToString(), "Local"));
        metricsPanel.Children.Add(CreateDashboardCard("Active", summary.ActiveItems.ToString(), "Signals"));
        metricsPanel.Children.Add(CreateDashboardCard("Needs review", summary.ReviewNeededItems.ToString(), "Manual"));
        metricsPanel.Children.Add(CreateDashboardCard("Waiting", summary.WaitingItems.ToString(), "State"));
        metricsPanel.Children.Add(CreateDashboardCard("Blocked", summary.BlockedItems.ToString(), "Stop"));
        metricsPanel.Children.Add(CreateDashboardCard("Links", summary.LinkCount.ToString(), "Cross-module"));
        metricsPanel.Children.Add(CreateDashboardCard("Proof links", summary.NeedsProofLinks.ToString(), "Evidence"));
        metricsPanel.Children.Add(CreateDashboardCard("Money links", summary.MoneyLinks.ToString(), "Not safe"));
        metricsPanel.Children.Add(CreateDashboardCard("Follow-up links", summary.FollowUpLinks.ToString(), "Relationship"));
        metricsPanel.Children.Add(CreateDashboardCard("Modules", summary.ModuleCount.ToString(), "Connected"));
        metricsPanel.Children.Add(CreateDashboardCard("Sync", summary.ExternalSyncEnabled ? "On" : "Off", "Local-first"));

        root.Children.Add(metricsPanel);

        var spinePanel = CreateInfoPanel("Spine rule", FormatReasons(summary.Reasons));
        spinePanel.Margin = new Thickness(0, 8, 0, 0);
        root.Children.Add(spinePanel);

        var controlsPanel = CreateUniversalSpineControlsPanel();
        controlsPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(controlsPanel);

        var priorityPanel = CreateUniversalSpineItemPanel("Priority spine signals", summary.PriorityItems);
        priorityPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(priorityPanel);

        var linkPanel = CreateUniversalSpineLinkPanel("Cross-module links", summary.Links, itemLookup);
        linkPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(linkPanel);

        var recentPanel = CreateUniversalSpineItemPanel("Recent local spine items", summary.RecentItems);
        recentPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(recentPanel);

        var boundaryPanel = CreateInfoPanel(
            "v2.1 boundary",
            "Universal Spine is local context and link state only. It is not universal search, AI reasoning, inbox scanning, cloud sync, automatic decisions, or integrations. Those layers come later after the offline spine is stable.");

        boundaryPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(boundaryPanel);

        var storagePanel = CreateInfoPanel(
            "Local spine file",
            $"Universal spine file: {UniversalSpineStorage.FilePath}");

        storagePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(storagePanel);

        MainContentControl.Content = root;
    }

    private Border CreateUniversalSpineControlsPanel()
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = "Spine controls",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        root.Children.Add(new TextBlock
        {
            Text = "Use reset only for fictional/demo spine data. v2.1 proves the local cross-module link model before universal search, integrations, or AI assistant layers.",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 16)
        });

        var buttonRow = new StackPanel
        {
            Orientation = Orientation.Horizontal
        };

        var resetButton = CreateActionButton("Reset Demo Spine", Color.FromRgb(30, 41, 59), Color.FromRgb(226, 232, 240));
        resetButton.Click += ResetUniversalSpineButton_Click;

        buttonRow.Children.Add(resetButton);
        root.Children.Add(buttonRow);

        panel.Child = root;
        return panel;
    }

    private Border CreateUniversalSpineItemPanel(string title, IEnumerable<UniversalSpineItem> items)
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = title,
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        var list = items.ToList();

        if (list.Count == 0)
        {
            root.Children.Add(CreateEmptyTextBlock("No spine items in this section."));
            panel.Child = root;
            return panel;
        }

        foreach (var item in list)
        {
            root.Children.Add(CreateSimpleItemCard(
                item.Title,
                $"{UniversalSpineCalculator.FormatKind(item.Kind)} • {UniversalSpineCalculator.FormatStatus(item.Status)} • {item.SourceModule} • Priority {item.Priority}",
                $"{item.NextAction}\n{item.Notes}"));
        }

        panel.Child = root;
        return panel;
    }

    private Border CreateUniversalSpineLinkPanel(string title, IEnumerable<UniversalSpineLink> links, IReadOnlyDictionary<Guid, UniversalSpineItem> itemLookup)
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = title,
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        var list = links.ToList();

        if (list.Count == 0)
        {
            root.Children.Add(CreateEmptyTextBlock("No cross-module links captured yet."));
            panel.Child = root;
            return panel;
        }

        foreach (var link in list)
        {
            var fromTitle = itemLookup.TryGetValue(link.FromItemId, out var fromItem) ? fromItem.Title : "Unknown source";
            var toTitle = itemLookup.TryGetValue(link.ToItemId, out var toItem) ? toItem.Title : "Unknown target";

            root.Children.Add(CreateSimpleItemCard(
                link.Label,
                UniversalSpineCalculator.FormatLinkType(link.LinkType),
                $"{fromTitle} → {toTitle}\n{link.Notes}"));
        }

        panel.Child = root;
        return panel;
    }


    private void ResetDesktopReleaseReadinessButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmRiskyAction("Reset desktop release readiness?", "This will replace the local release readiness checklist with the fictional v2.0 default set."))
        {
            return;
        }

        DesktopReleaseStorage.ResetToDemoData();
        _desktopReleaseProfile = DesktopReleaseStorage.Load();
        ShowDesktopReleasePage();
    }

    private void ShowDesktopReleasePage()
    {
        _desktopReleaseProfile = DesktopReleaseStorage.Load();
        var summary = DesktopReleaseReadinessCalculator.Calculate(_desktopReleaseProfile);

        SetHeader("Desktop Release Centre", $"Desktop Release Centre • v2.0 • {summary.ReleaseStateLabel}");

        var root = new StackPanel();

        root.Children.Add(CreateHeroPanel(
            "Desktop Release Centre",
            "Turn the offline LifeOS foundation into a paid desktop release candidate: local-first data, proof-backed money control, manual review gates, demo-safe screenshots, docs, and release readiness checks."));

        var metricsPanel = new WrapPanel
        {
            Margin = new Thickness(0, 22, 0, 0)
        };

        metricsPanel.Children.Add(CreateDashboardCard("Release state", summary.ReleaseStateLabel, "v2.0"));
        metricsPanel.Children.Add(CreateDashboardCard("Readiness", $"{summary.ScorePercent}%", "Checks"));
        metricsPanel.Children.Add(CreateDashboardCard("Complete", summary.CompleteChecks.ToString(), "Done"));
        metricsPanel.Children.Add(CreateDashboardCard("Needs review", summary.ReviewNeededChecks.ToString(), "Before tag"));
        metricsPanel.Children.Add(CreateDashboardCard("Blocked", summary.BlockedChecks.ToString(), "Stop"));
        metricsPanel.Children.Add(CreateDashboardCard("Planned next", summary.PlannedNextChecks.ToString(), "Future"));
        metricsPanel.Children.Add(CreateDashboardCard("Local-only", summary.IsLocalOnly ? "On" : "Off", "Safety"));
        metricsPanel.Children.Add(CreateDashboardCard("Screenshot safe", summary.IsScreenshotSafe ? "On" : "Review", "Docs"));

        root.Children.Add(metricsPanel);

        var reasonsPanel = CreateInfoPanel("Release readiness", FormatReasons(summary.Reasons));
        reasonsPanel.Margin = new Thickness(0, 8, 0, 0);
        root.Children.Add(reasonsPanel);

        var controlsPanel = CreateDesktopReleaseControlsPanel();
        controlsPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(controlsPanel);

        var reviewPanel = CreateDesktopReleaseChecklistPanel("Review before v2.0 tag", summary.PriorityItems);
        reviewPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(reviewPanel);

        var completePanel = CreateDesktopReleaseChecklistPanel("Completed release foundation", summary.CompletedItems.Take(10));
        completePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(completePanel);

        var plannedPanel = CreateDesktopReleaseChecklistPanel("Planned after v2.0", summary.PlannedNextItems);
        plannedPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(plannedPanel);

        var boundaryPanel = CreateInfoPanel(
            "v2.0 boundary",
            "v2.0 is the paid desktop release readiness checkpoint for the offline/local-first app. It is not integrations, cloud sync, automatic messaging, accounting automation, mobile capture, AI assistant execution, or a production installer/signing system.");

        boundaryPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(boundaryPanel);

        var storagePanel = CreateInfoPanel(
            "Local release file",
            $"Desktop release readiness file: {DesktopReleaseStorage.FilePath}");

        storagePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(storagePanel);

        MainContentControl.Content = root;
    }

    private Border CreateDesktopReleaseControlsPanel()
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = "Release controls",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        root.Children.Add(new TextBlock
        {
            Text = "Use reset only for fictional/demo release readiness data. The final v2.0 tag should happen after Group 05 screenshots/docs/README are committed.",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 16)
        });

        var buttonRow = new StackPanel
        {
            Orientation = Orientation.Horizontal
        };

        var resetButton = CreateActionButton("Reset Demo Release State", Color.FromRgb(30, 41, 59), Color.FromRgb(226, 232, 240));
        resetButton.Click += ResetDesktopReleaseReadinessButton_Click;

        buttonRow.Children.Add(resetButton);

        root.Children.Add(buttonRow);

        panel.Child = root;
        return panel;
    }

    private Border CreateDesktopReleaseChecklistPanel(string title, IEnumerable<DesktopReleaseChecklistItem> items)
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = title,
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        var list = items.ToList();

        if (list.Count == 0)
        {
            root.Children.Add(CreateEmptyTextBlock("No release checks in this section."));
            panel.Child = root;
            return panel;
        }

        foreach (var item in list)
        {
            root.Children.Add(CreateSimpleItemCard(
                item.Title,
                $"{DesktopReleaseReadinessCalculator.FormatArea(item.Area)} • {DesktopReleaseReadinessCalculator.FormatStatus(item.Status)} • Priority {item.Priority}",
                item.Notes));
        }

        panel.Child = root;
        return panel;
    }


    private void SaveSettingsSafetyThemeButton_Click(object sender, RoutedEventArgs e)
    {
        _settingsSafetyProfile.SafetyMode = _settingsSafetyModeComboBox?.SelectedItem is LifeOSSafetyMode selectedSafetyMode
            ? selectedSafetyMode
            : LifeOSSafetyMode.Strict;

        _settingsSafetyProfile.Appearance = _settingsAppearanceComboBox?.SelectedItem is LifeOSAppearancePreference selectedAppearance
            ? selectedAppearance
            : LifeOSAppearancePreference.Dark;

        _settingsSafetyProfile.Accent = _settingsAccentComboBox?.SelectedItem is LifeOSAccentPreference selectedAccent
            ? selectedAccent
            : LifeOSAccentPreference.Cyan;

        _settingsSafetyProfile.LocalOnlyMode = _settingsLocalOnlyCheckBox?.IsChecked == true;
        _settingsSafetyProfile.RequireManualReviewBeforeSend = _settingsManualReviewCheckBox?.IsChecked == true;
        _settingsSafetyProfile.TreatExpectedMoneyAsUnsafe = _settingsExpectedMoneyUnsafeCheckBox?.IsChecked == true;
        _settingsSafetyProfile.HidePrivateDetailsInScreenshots = _settingsScreenshotSafetyCheckBox?.IsChecked == true;
        _settingsSafetyProfile.ConfirmDestructiveActions = _settingsConfirmDestructiveCheckBox?.IsChecked == true;
        _settingsSafetyProfile.DemoSafeDataMode = _settingsDemoSafeDataCheckBox?.IsChecked == true;
        _settingsSafetyProfile.EnableExperimentalModules = _settingsExperimentalModulesCheckBox?.IsChecked == true;
        _settingsSafetyProfile.ActiveBuildLane = ReadTextValue(_settingsBuildLaneTextBox, "Offline foundation");
        _settingsSafetyProfile.CurrentVersionNote = ReadTextValue(_settingsVersionNoteTextBox, "v1.8 settings, safety, and theme foundation");
        _settingsSafetyProfile.UpdatedAt = DateTime.Now;

        SettingsSafetyThemeStorage.Save(_settingsSafetyProfile);
        ShowSettingsSafetyThemePage();
    }

    private void ResetSettingsSafetyThemeButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmRiskyAction("Reset Settings / Safety / Theme defaults?", "This will replace local Settings / Safety / Theme preferences with the fictional v1.8 defaults."))
        {
            return;
        }

        SettingsSafetyThemeStorage.ResetToDemoData();
        _settingsSafetyProfile = SettingsSafetyThemeStorage.Load();
        ShowSettingsSafetyThemePage();
    }

    private void ShowSettingsSafetyThemePage()
    {
        _settingsSafetyProfile = SettingsSafetyThemeStorage.Load();
        var summary = SettingsSafetyThemeCalculator.Calculate(_settingsSafetyProfile);

        SetHeader("Settings / Safety", $"Settings / Safety / Theme • v1.8 • {summary.SafetyLabel}");

        var root = new StackPanel();

        root.Children.Add(CreateHeroPanel(
            "Settings / Safety / Theme",
            "Control LifeOS guardrails before integrations: local-only mode, manual review gates, expected-money safety, screenshot privacy, destructive-action confirmations, and theme preferences."));

        var metricsPanel = new WrapPanel
        {
            Margin = new Thickness(0, 22, 0, 0)
        };

        metricsPanel.Children.Add(CreateDashboardCard("Safety mode", summary.SafetyLabel, "Guardrails"));
        metricsPanel.Children.Add(CreateDashboardCard("Enabled guardrails", summary.EnabledGuardrails.ToString(), "Protected"));
        metricsPanel.Children.Add(CreateDashboardCard("Manual gates", summary.ManualReviewGates.ToString(), "Review"));
        metricsPanel.Children.Add(CreateDashboardCard("Privacy protections", summary.PrivacyProtections.ToString(), "Screenshots"));
        metricsPanel.Children.Add(CreateDashboardCard("Appearance", summary.AppearanceLabel, "Theme"));
        metricsPanel.Children.Add(CreateDashboardCard("Accent", summary.AccentLabel, "Visual"));
        metricsPanel.Children.Add(CreateDashboardCard("Local-first", summary.IsLocalFirst ? "On" : "Off", "Storage"));
        metricsPanel.Children.Add(CreateDashboardCard("Expected money", summary.IsExpectedMoneyProtected ? "Not safe" : "Unprotected", "Money rule"));

        root.Children.Add(metricsPanel);

        var controlsPanel = CreateSettingsSafetyThemeInputPanel(summary);
        controlsPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(controlsPanel);

        var guardrailsPanel = CreateInfoPanel(
            "Active guardrails",
            FormatReasons(summary.GuardrailReasons));
        guardrailsPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(guardrailsPanel);

        var themePanel = CreateInfoPanel(
            "Theme boundary",
            FormatReasons(summary.ThemeNotes));
        themePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(themePanel);

        var privacyPanel = CreateInfoPanel(
            "Screenshot privacy checklist",
            "Before screenshots/docs: use fictional names only; hide emails, phone numbers, tenant/app IDs, URLs, private client data, bank/payment details, and secrets. Real workflow shape is allowed; real private identity is not.");
        privacyPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(privacyPanel);

        var storagePanel = CreateInfoPanel(
            "Local storage",
            $"Settings file: {SettingsSafetyThemeStorage.FilePath}");
        storagePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(storagePanel);

        var scopePanel = CreateInfoPanel(
            "v1.8 scope",
            "This stage records settings, guardrails, safety boundaries, and theme preferences. It does not add account management, permissions, encryption, live theme switching, cloud sync, or integration controls yet.");
        scopePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(scopePanel);

        MainContentControl.Content = root;
    }

    private Border CreateSettingsSafetyThemeInputPanel(SettingsSafetyThemeSummary summary)
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = "Settings controls",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        root.Children.Add(new TextBlock
        {
            Text = "These settings protect the offline foundation. They are saved locally and intentionally conservative before integrations.",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 16)
        });

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        for (var i = 0; i < 6; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        _settingsSafetyModeComboBox = CreateEnumComboBox(_settingsSafetyProfile.SafetyMode);
        _settingsAppearanceComboBox = CreateEnumComboBox(_settingsSafetyProfile.Appearance);
        _settingsAccentComboBox = CreateEnumComboBox(_settingsSafetyProfile.Accent);
        _settingsBuildLaneTextBox = CreateStandardTextBox(_settingsSafetyProfile.ActiveBuildLane);
        _settingsVersionNoteTextBox = CreateStandardTextBox(_settingsSafetyProfile.CurrentVersionNote);

        AddInputField(grid, "Safety mode", _settingsSafetyModeComboBox, 0, 0);
        AddInputField(grid, "Appearance", _settingsAppearanceComboBox, 0, 1);
        AddInputField(grid, "Accent", _settingsAccentComboBox, 1, 0);
        AddInputField(grid, "Build lane", _settingsBuildLaneTextBox, 1, 1);
        AddInputField(grid, "Version note", _settingsVersionNoteTextBox, 2, 0);

        var checks = new WrapPanel
        {
            Margin = new Thickness(0, 8, 0, 0)
        };

        _settingsLocalOnlyCheckBox = CreateSettingsSafetyCheckBox("Local-only mode", _settingsSafetyProfile.LocalOnlyMode);
        _settingsManualReviewCheckBox = CreateSettingsSafetyCheckBox("Manual review before send", _settingsSafetyProfile.RequireManualReviewBeforeSend);
        _settingsExpectedMoneyUnsafeCheckBox = CreateSettingsSafetyCheckBox("Expected money is not safe", _settingsSafetyProfile.TreatExpectedMoneyAsUnsafe);
        _settingsScreenshotSafetyCheckBox = CreateSettingsSafetyCheckBox("Screenshot privacy", _settingsSafetyProfile.HidePrivateDetailsInScreenshots);
        _settingsConfirmDestructiveCheckBox = CreateSettingsSafetyCheckBox("Confirm destructive actions", _settingsSafetyProfile.ConfirmDestructiveActions);
        _settingsDemoSafeDataCheckBox = CreateSettingsSafetyCheckBox("Demo-safe data", _settingsSafetyProfile.DemoSafeDataMode);
        _settingsExperimentalModulesCheckBox = CreateSettingsSafetyCheckBox("Show experimental modules", _settingsSafetyProfile.EnableExperimentalModules);

        checks.Children.Add(_settingsLocalOnlyCheckBox);
        checks.Children.Add(_settingsManualReviewCheckBox);
        checks.Children.Add(_settingsExpectedMoneyUnsafeCheckBox);
        checks.Children.Add(_settingsScreenshotSafetyCheckBox);
        checks.Children.Add(_settingsConfirmDestructiveCheckBox);
        checks.Children.Add(_settingsDemoSafeDataCheckBox);
        checks.Children.Add(_settingsExperimentalModulesCheckBox);

        Grid.SetRow(checks, 3);
        Grid.SetColumn(checks, 0);
        Grid.SetColumnSpan(checks, 2);
        grid.Children.Add(checks);

        root.Children.Add(grid);

        var buttonRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 18, 0, 0)
        };

        var saveButton = CreateActionButton("Save Settings", Color.FromRgb(56, 189, 248), Color.FromRgb(15, 23, 42));
        saveButton.Click += SaveSettingsSafetyThemeButton_Click;

        var resetButton = CreateActionButton("Reset Demo Safety", Color.FromRgb(30, 41, 59), Color.FromRgb(226, 232, 240));
        resetButton.Click += ResetSettingsSafetyThemeButton_Click;

        buttonRow.Children.Add(saveButton);
        buttonRow.Children.Add(resetButton);
        root.Children.Add(buttonRow);

        root.Children.Add(new TextBlock
        {
            Text = $"Current summary: {summary.EnabledGuardrails} guardrail(s), {summary.ManualReviewGates} manual review gate(s), {summary.PrivacyProtections} privacy protection(s).",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 14, 0, 0)
        });

        panel.Child = root;
        return panel;
    }

    private static CheckBox CreateSettingsSafetyCheckBox(string text, bool isChecked)
    {
        return new CheckBox
        {
            Content = text,
            IsChecked = isChecked,
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            Margin = new Thickness(0, 8, 18, 8)
        };
    }


    private void AddFollowUpButton_Click(object sender, RoutedEventArgs e)
    {
        var person = _followUpPersonTextBox?.Text.Trim() ?? string.Empty;
        var context = _followUpContextTextBox?.Text.Trim() ?? string.Empty;
        var nextAction = _followUpNextActionTextBox?.Text.Trim() ?? string.Empty;
        var notes = _followUpNotesTextBox?.Text.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(person))
        {
            person = "Unnamed follow-up";
        }

        if (string.IsNullOrWhiteSpace(context))
        {
            context = "No context entered.";
        }

        if (string.IsNullOrWhiteSpace(nextAction))
        {
            nextAction = "No next action entered.";
        }

        DateOnly? followUpDate = null;

        if (DateOnly.TryParse(_followUpDateTextBox?.Text.Trim(), out var parsedDate))
        {
            followUpDate = parsedDate;
        }

        var status = _followUpStatusComboBox?.SelectedItem is FollowUpStatus selectedStatus
            ? selectedStatus
            : FollowUpStatus.Waiting;

        var priority = _followUpPriorityComboBox?.SelectedItem is FollowUpPriority selectedPriority
            ? selectedPriority
            : FollowUpPriority.Normal;

        _followUps.Add(new FollowUpItem
        {
            PersonOrOrganisation = person,
            Context = context,
            NextAction = nextAction,
            FollowUpDate = followUpDate,
            Status = status,
            Priority = priority,
            IsMoneyLinked = _followUpMoneyLinkedCheckBox?.IsChecked == true,
            Notes = notes,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        });

        FollowUpStorage.Save(_followUps);
        ShowFollowUpsPage();
    }

    private void SaveFollowUpsButton_Click(object sender, RoutedEventArgs e)
    {
        FollowUpStorage.Save(_followUps);
        ShowFollowUpsPage();
    }

    private void ResetFollowUpsButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmRiskyAction("Reset follow-up defaults?", "This will replace your saved follow-ups with the default local sample set."))
        {
            return;
        }

        FollowUpStorage.Reset();
        _followUps = FollowUpStorage.Load();
        ShowFollowUpsPage();
    }

    private void CompleteFollowUpButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not Guid id)
        {
            return;
        }

        var item = _followUps.FirstOrDefault(item => item.Id == id);

        if (item is null)
        {
            return;
        }

        item.Status = FollowUpStatus.Completed;
        item.UpdatedAt = DateTime.Now;

        FollowUpStorage.Save(_followUps);
        ShowFollowUpsPage();
    }

    private void DeleteFollowUpButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not Guid id)
        {
            return;
        }

        if (!ConfirmRiskyAction("Delete follow-up?", "This removes the selected follow-up from local storage."))
        {
            return;
        }

        _followUps = _followUps
            .Where(item => item.Id != id)
            .ToList();

        FollowUpStorage.Save(_followUps);
        ShowFollowUpsPage();
    }

    private void UpdateMoneyPressureInputFromTextBoxes()
    {
        _moneyPressureInput.CurrentBalance = ReadMoneyValue(_currentBalanceTextBox, _moneyPressureInput.CurrentBalance);
        _moneyPressureInput.PaidIncome = ReadMoneyValue(_paidIncomeTextBox, _moneyPressureInput.PaidIncome);
        _moneyPressureInput.PendingIncome = ReadMoneyValue(_pendingIncomeTextBox, _moneyPressureInput.PendingIncome);
        _moneyPressureInput.BillsDue = ReadMoneyValue(_billsDueTextBox, _moneyPressureInput.BillsDue);
        _moneyPressureInput.DeductionsDue = ReadMoneyValue(_deductionsDueTextBox, _moneyPressureInput.DeductionsDue);
        _moneyPressureInput.FoodFuelBuffer = ReadMoneyValue(_foodFuelBufferTextBox, _moneyPressureInput.FoodFuelBuffer);
        _moneyPressureInput.EmergencyBuffer = ReadMoneyValue(_emergencyBufferTextBox, _moneyPressureInput.EmergencyBuffer);
    }

    private void ShowModulePage(LifeOSModuleKind kind)
    {
        var module = LifeOSModuleCatalog.GetModule(kind);

        SetHeader(module.Title, $"{module.Title} • {module.Badge}");

        var root = new StackPanel();

        root.Children.Add(CreateHeroPanel(module.Title, module.DetailDescription));

        var rolePanel = CreateInfoPanel("Platform role", module.PlatformRole);
        rolePanel.Margin = new Thickness(0, 22, 0, 0);
        root.Children.Add(rolePanel);

        var nextPanel = CreateInfoPanel("Next build focus", module.NextBuildFocus);
        nextPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(nextPanel);

        var statusPanel = CreateInfoPanel("Build status", GetModuleStatusText(module));
        statusPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(statusPanel);

        MainContentControl.Content = root;
    }

    private void ShowMoneyPressurePage()
    {
        var module = LifeOSModuleCatalog.GetModule(LifeOSModuleKind.MoneyPressure);
        var summary = _moneyPressureInput.Calculate();

        SetHeader(module.Title, $"{module.Title} • Manual input foundation");

        var root = new StackPanel();

        root.Children.Add(CreateHeroPanel(
            "Money Pressure",
            "Manual money-pressure foundation. Enter what matters, recalculate safe-to-spend, and keep pending invoice money separate from money that has actually landed."));

        var inputPanel = CreateMoneyPressureInputPanel();
        inputPanel.Margin = new Thickness(0, 22, 0, 0);
        root.Children.Add(inputPanel);

        var metricsPanel = CreateMoneyPressureMetricsPanel(summary);
        metricsPanel.Margin = new Thickness(0, 22, 0, 0);
        root.Children.Add(metricsPanel);

        var reasonsText = string.Join(Environment.NewLine, summary.Reasons.Select(reason => $"• {reason}"));

        var reasonsPanel = CreateInfoPanel("Why this week has pressure", reasonsText);
        reasonsPanel.Margin = new Thickness(0, 8, 0, 0);
        root.Children.Add(reasonsPanel);

        var guardrailPanel = CreateInfoPanel(
            "Phase 10 scope",
            $"Manual input with local JSON persistence. Saved file: {MoneyPressureStorage.FilePath}. No database, bank sync, invoices, mobile app, or full money module yet.");

        guardrailPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(guardrailPanel);

        var realLifePanel = CreateInfoPanel(
            "v0.4 real-life lesson",
            "On chaotic days LifeOS should not demand constant logging. The most useful tools are fast: invoice templates, pay-later checks, money pressure, and follow-up reminders that can be used in short bursts.");

        realLifePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(realLifePanel);

        MainContentControl.Content = root;
    }

    private void ShowFollowUpsPage()
    {
        var module = LifeOSModuleCatalog.GetModule(LifeOSModuleKind.FollowUps);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var summary = FollowUpCalculator.Calculate(_followUps, today);

        SetHeader(module.Title, $"{module.Title} • Local follow-up foundation");

        var root = new StackPanel();

        root.Children.Add(CreateHeroPanel(
            "Follow-Ups",
            "First editable waiting-on/follow-up foundation. Track who you are waiting on, what the next action is, whether money is linked, and when to follow up."));

        var metricsPanel = new WrapPanel
        {
            Margin = new Thickness(0, 22, 0, 0)
        };

        metricsPanel.Children.Add(CreateDashboardCard("Open follow-ups", summary.TotalOpen.ToString(), "Active"));
        metricsPanel.Children.Add(CreateDashboardCard("Waiting", summary.WaitingCount.ToString(), "Waiting-on"));
        metricsPanel.Children.Add(CreateDashboardCard("Needs action", summary.NeedsActionCount.ToString(), "Action"));
        metricsPanel.Children.Add(CreateDashboardCard("Overdue", summary.OverdueCount.ToString(), "Pressure"));
        metricsPanel.Children.Add(CreateDashboardCard("Due today", summary.DueTodayCount.ToString(), "Today"));
        metricsPanel.Children.Add(CreateDashboardCard("Money-linked", summary.MoneyLinkedCount.ToString(), "Work/money"));

        root.Children.Add(metricsPanel);

        var reasonsText = string.Join(Environment.NewLine, summary.Reasons.Select(reason => $"• {reason}"));
        var reasonsPanel = CreateInfoPanel("Why follow-ups matter this week", reasonsText);
        reasonsPanel.Margin = new Thickness(0, 8, 0, 0);
        root.Children.Add(reasonsPanel);

        var inputPanel = CreateFollowUpInputPanel();
        inputPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(inputPanel);

        var listPanel = CreateFollowUpListPanel();
        listPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(listPanel);

        var guardrailPanel = CreateInfoPanel(
            "Phase 11 scope",
            $"Local follow-up tracking with JSON persistence. Saved file: {FollowUpStorage.FilePath}. No email import, calendar sync, reminders, notifications, or CRM system yet.");

        guardrailPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(guardrailPanel);

        MainContentControl.Content = root;
    }


    private void AddAgendaButton_Click(object sender, RoutedEventArgs e)
    {
        var title = ReadTextValue(_agendaTitleTextBox, "Untitled agenda item");

        var dueDate = DateOnly.TryParse(_agendaDueDateTextBox?.Text.Trim(), out var parsedDate)
            ? parsedDate
            : (DateOnly?)null;

        var type = _agendaTypeComboBox?.SelectedItem is AgendaItemType selectedType
            ? selectedType
            : AgendaItemType.Task;

        var status = _agendaStatusComboBox?.SelectedItem is AgendaItemStatus selectedStatus
            ? selectedStatus
            : AgendaItemStatus.Planned;

        var pressure = _agendaPressureComboBox?.SelectedItem is AgendaPressureLevel selectedPressure
            ? selectedPressure
            : AgendaPressureLevel.Normal;

        _agendaItems.Add(new AgendaItem
        {
            Title = title,
            Type = type,
            Status = status,
            PressureLevel = pressure,
            DueDate = dueDate,
            TimeText = _agendaTimeTextBox?.Text.Trim() ?? string.Empty,
            IsFixedCommitment = _agendaFixedCheckBox?.IsChecked == true,
            Notes = _agendaNotesTextBox?.Text.Trim() ?? string.Empty,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        });

        AgendaStorage.Save(_agendaItems);
        ShowAgendaPage();
    }

    private void ResetAgendaButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmRiskyAction("Reset agenda defaults?", "This will replace your saved agenda items with the default local sample set."))
        {
            return;
        }

        AgendaStorage.Reset();
        _agendaItems = AgendaStorage.Load();
        ShowAgendaPage();
    }

    private void CompleteAgendaButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not Guid id)
        {
            return;
        }

        var item = _agendaItems.FirstOrDefault(item => item.Id == id);

        if (item is null)
        {
            return;
        }

        item.Status = AgendaItemStatus.Completed;
        item.UpdatedAt = DateTime.Now;

        AgendaStorage.Save(_agendaItems);
        ShowAgendaPage();
    }

    private void DeleteAgendaButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not Guid id)
        {
            return;
        }

        if (!ConfirmRiskyAction("Delete agenda item?", "This removes the selected agenda item from local storage."))
        {
            return;
        }

        _agendaItems = _agendaItems
            .Where(item => item.Id != id)
            .ToList();

        AgendaStorage.Save(_agendaItems);
        ShowAgendaPage();
    }

    private void ShowAgendaPage()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var summary = AgendaCalculator.Calculate(_agendaItems, today);

        SetHeader("Agenda", "Agenda • v4.4 • connected to Payment Calendar");

        var root = new StackPanel();

        root.Children.Add(CreateHeroPanel(
            "Agenda",
            "Track what matters this week: tasks, appointments, due dates, fixed commitments, and pressure. v4.4 links this agenda lane to the new Payment Calendar without turning LifeOS into a live calendar clone yet."));

        var metricsPanel = new WrapPanel
        {
            Margin = new Thickness(0, 22, 0, 0)
        };

        metricsPanel.Children.Add(CreateDashboardCard("Open agenda items", summary.TotalOpen.ToString(), "Agenda"));
        metricsPanel.Children.Add(CreateDashboardCard("Due today", summary.DueTodayCount.ToString(), "Today"));
        metricsPanel.Children.Add(CreateDashboardCard("Overdue", summary.OverdueCount.ToString(), "Pressure"));
        metricsPanel.Children.Add(CreateDashboardCard("This week", summary.ThisWeekCount.ToString(), "Week"));
        metricsPanel.Children.Add(CreateDashboardCard("High pressure", summary.HighPressureCount.ToString(), "Priority"));

        root.Children.Add(metricsPanel);

        var reasonsPanel = CreateInfoPanel("Agenda pressure", FormatReasons(summary.Reasons));
        reasonsPanel.Margin = new Thickness(0, 8, 0, 0);
        root.Children.Add(reasonsPanel);

        var inputPanel = CreateAgendaInputPanel();
        inputPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(inputPanel);

        var listPanel = CreateAgendaListPanel();
        listPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(listPanel);

        MainContentControl.Content = root;
    }

    private Border CreateAgendaInputPanel()
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = "Add agenda item",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        root.Children.Add(new TextBlock
        {
            Text = "Keep the week visible without turning LifeOS into a full calendar app. Capture the few things that change pressure.",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 16)
        });

        var grid = new Grid();

        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        for (var i = 0; i < 4; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        _agendaTitleTextBox = CreateStandardTextBox("New agenda item");
        _agendaDueDateTextBox = CreateStandardTextBox(DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd"));
        _agendaTimeTextBox = CreateStandardTextBox("Any time");
        _agendaTypeComboBox = CreateEnumComboBox(AgendaItemType.Task);
        _agendaStatusComboBox = CreateEnumComboBox(AgendaItemStatus.Planned);
        _agendaPressureComboBox = CreateEnumComboBox(AgendaPressureLevel.Normal);
        _agendaNotesTextBox = CreateStandardTextBox("Optional notes");

        _agendaFixedCheckBox = new CheckBox
        {
            Content = "Fixed commitment",
            Foreground = new SolidColorBrush(Color.FromRgb(203, 213, 225)),
            Margin = new Thickness(0, 8, 14, 14)
        };

        AddInputField(grid, "Title", _agendaTitleTextBox, 0, 0);
        AddInputField(grid, "Due date yyyy-mm-dd", _agendaDueDateTextBox, 0, 1);
        AddInputField(grid, "Time", _agendaTimeTextBox, 1, 0);
        AddInputField(grid, "Type", _agendaTypeComboBox, 1, 1);
        AddInputField(grid, "Status", _agendaStatusComboBox, 2, 0);
        AddInputField(grid, "Pressure", _agendaPressureComboBox, 2, 1);
        AddInputField(grid, "Notes", _agendaNotesTextBox, 3, 0);

        Grid.SetRow(_agendaFixedCheckBox, 3);
        Grid.SetColumn(_agendaFixedCheckBox, 1);
        grid.Children.Add(_agendaFixedCheckBox);

        root.Children.Add(grid);

        var buttonRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 18, 0, 0)
        };

        var addButton = CreateActionButton("Add Agenda Item", Color.FromRgb(56, 189, 248), Color.FromRgb(15, 23, 42));
        addButton.Click += AddAgendaButton_Click;

        var resetButton = CreateActionButton("Reset Defaults", Color.FromRgb(30, 41, 59), Color.FromRgb(226, 232, 240));
        resetButton.Click += ResetAgendaButton_Click;

        buttonRow.Children.Add(addButton);
        buttonRow.Children.Add(resetButton);

        root.Children.Add(buttonRow);

        panel.Child = root;
        return panel;
    }

    private Border CreateAgendaListPanel()
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = "Current agenda items",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        if (_agendaItems.Count == 0)
        {
            root.Children.Add(CreateEmptyTextBlock("No agenda items yet. Add the next real commitment, not every thought."));
            panel.Child = root;
            return panel;
        }

        foreach (var item in _agendaItems.OrderBy(item => item.DueDate ?? DateOnly.MaxValue))
        {
            var dueText = item.DueDate.HasValue
                ? item.DueDate.Value.ToString("yyyy-MM-dd")
                : "No date";

            var body = $"{item.Type} • {item.Status} • {item.PressureLevel} • {dueText} • {item.TimeText}";

            var card = CreateSimpleItemCard(item.Title, body, item.Notes);

            var buttonRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 12, 0, 0)
            };

            var completeButton = CreateSmallActionButton("Mark Complete");
            completeButton.Tag = item.Id;
            completeButton.Click += CompleteAgendaButton_Click;

            var deleteButton = CreateSmallActionButton("Delete");
            deleteButton.Tag = item.Id;
            deleteButton.Click += DeleteAgendaButton_Click;

            buttonRow.Children.Add(completeButton);
            buttonRow.Children.Add(deleteButton);

            if (card.Child is StackPanel cardStack)
            {
                cardStack.Children.Add(buttonRow);
            }

            root.Children.Add(card);
        }

        panel.Child = root;
        return panel;
    }

    private void AddPayLaterButton_Click(object sender, RoutedEventArgs e)
    {
        var dueDate = DateOnly.TryParse(_payLaterDueDateTextBox?.Text.Trim(), out var parsedDate)
            ? parsedDate
            : (DateOnly?)null;

        var status = _payLaterStatusComboBox?.SelectedItem is PayLaterStatus selectedStatus
            ? selectedStatus
            : PayLaterStatus.Planned;

        var pressure = _payLaterPressureComboBox?.SelectedItem is PayLaterPressureLevel selectedPressure
            ? selectedPressure
            : PayLaterPressureLevel.Normal;

        _payLaterItems.Add(new PayLaterItem
        {
            Name = ReadTextValue(_payLaterNameTextBox, "Untitled pay-later item"),
            Payee = ReadTextValue(_payLaterPayeeTextBox, "Unknown payee"),
            Amount = ReadMoneyValue(_payLaterAmountTextBox, 0m),
            DueDate = dueDate,
            Status = status,
            PressureLevel = pressure,
            Notes = _payLaterNotesTextBox?.Text.Trim() ?? string.Empty,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        });

        PayLaterStorage.Save(_payLaterItems);
        ShowPayLaterPage();
    }

    private void ResetPayLaterButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmRiskyAction("Reset pay-later defaults?", "This will replace your saved pay-later items with the default local sample set."))
        {
            return;
        }

        PayLaterStorage.Reset();
        _payLaterItems = PayLaterStorage.Load();
        ShowPayLaterPage();
    }

    private void MarkPayLaterPaidButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not Guid id)
        {
            return;
        }

        var item = _payLaterItems.FirstOrDefault(item => item.Id == id);

        if (item is null)
        {
            return;
        }

        item.Status = PayLaterStatus.Paid;
        item.UpdatedAt = DateTime.Now;

        PayLaterStorage.Save(_payLaterItems);
        ShowPayLaterPage();
    }

    private void DeletePayLaterButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not Guid id)
        {
            return;
        }

        if (!ConfirmRiskyAction("Delete pay-later item?", "This removes the selected deferred-payment item from local storage."))
        {
            return;
        }

        _payLaterItems = _payLaterItems
            .Where(item => item.Id != id)
            .ToList();

        PayLaterStorage.Save(_payLaterItems);
        ShowPayLaterPage();
    }

    private void ShowPayLaterPage()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var summary = PayLaterCalculator.Calculate(_payLaterItems, today);

        SetHeader("Pay Later", "Pay Later • v0.4 deferred pressure foundation");

        var root = new StackPanel();

        root.Children.Add(CreateHeroPanel(
            "Pay Later",
            "Track deferred obligations so future pressure is visible before it becomes damage."));

        var metricsPanel = new WrapPanel
        {
            Margin = new Thickness(0, 22, 0, 0)
        };

        metricsPanel.Children.Add(CreateDashboardCard("Open items", summary.TotalOpen.ToString(), "Items"));
        metricsPanel.Children.Add(CreateDashboardCard("Open amount", FormatMoney(summary.TotalAmountOpen), "Deferred"));
        metricsPanel.Children.Add(CreateDashboardCard("Due this week", FormatMoney(summary.DueThisWeekAmount), "This week"));
        metricsPanel.Children.Add(CreateDashboardCard("Overdue", FormatMoney(summary.OverdueAmount), "Pressure"));
        metricsPanel.Children.Add(CreateDashboardCard("High pressure", summary.HighPressureCount.ToString(), "Priority"));

        root.Children.Add(metricsPanel);

        var reasonsPanel = CreateInfoPanel("Pay Later pressure", FormatReasons(summary.Reasons));
        reasonsPanel.Margin = new Thickness(0, 8, 0, 0);
        root.Children.Add(reasonsPanel);

        var inputPanel = CreatePayLaterInputPanel();
        inputPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(inputPanel);

        var listPanel = CreatePayLaterListPanel();
        listPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(listPanel);

        MainContentControl.Content = root;
    }

    private Border CreatePayLaterInputPanel()
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = "Add pay-later item",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        root.Children.Add(new TextBlock
        {
            Text = "Deferred payments still create pressure. Keep them visible before shopping, fuel runs, or invoice money lands.",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 16)
        });

        var grid = new Grid();

        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        for (var i = 0; i < 4; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        _payLaterNameTextBox = CreateStandardTextBox("IRD / bill / obligation");
        _payLaterPayeeTextBox = CreateStandardTextBox("Payee");
        _payLaterAmountTextBox = CreateStandardTextBox("0.00");
        _payLaterDueDateTextBox = CreateStandardTextBox(DateOnly.FromDateTime(DateTime.Today).AddDays(7).ToString("yyyy-MM-dd"));
        _payLaterStatusComboBox = CreateEnumComboBox(PayLaterStatus.Planned);
        _payLaterPressureComboBox = CreateEnumComboBox(PayLaterPressureLevel.Normal);
        _payLaterNotesTextBox = CreateStandardTextBox("Optional notes");

        AddInputField(grid, "Name", _payLaterNameTextBox, 0, 0);
        AddInputField(grid, "Payee", _payLaterPayeeTextBox, 0, 1);
        AddInputField(grid, "Amount", _payLaterAmountTextBox, 1, 0);
        AddInputField(grid, "Due date yyyy-mm-dd", _payLaterDueDateTextBox, 1, 1);
        AddInputField(grid, "Status", _payLaterStatusComboBox, 2, 0);
        AddInputField(grid, "Pressure", _payLaterPressureComboBox, 2, 1);
        AddInputField(grid, "Notes", _payLaterNotesTextBox, 3, 0);

        root.Children.Add(grid);

        var buttonRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 18, 0, 0)
        };

        var addButton = CreateActionButton("Add Pay Later Item", Color.FromRgb(56, 189, 248), Color.FromRgb(15, 23, 42));
        addButton.Click += AddPayLaterButton_Click;

        var resetButton = CreateActionButton("Reset Defaults", Color.FromRgb(30, 41, 59), Color.FromRgb(226, 232, 240));
        resetButton.Click += ResetPayLaterButton_Click;

        buttonRow.Children.Add(addButton);
        buttonRow.Children.Add(resetButton);

        root.Children.Add(buttonRow);

        panel.Child = root;
        return panel;
    }

    private Border CreatePayLaterListPanel()
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = "Current pay-later items",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        if (_payLaterItems.Count == 0)
        {
            root.Children.Add(CreateEmptyTextBlock("No pay-later items yet. Add Afterpay, bills, or deferred payments when they create future pressure."));
            panel.Child = root;
            return panel;
        }

        foreach (var item in _payLaterItems.OrderBy(item => item.DueDate ?? DateOnly.MaxValue))
        {
            var dueText = item.DueDate.HasValue
                ? item.DueDate.Value.ToString("yyyy-MM-dd")
                : "No date";

            var body = $"{item.Payee} • {FormatMoney(item.Amount)} • {item.Status} • {item.PressureLevel} • {dueText}";

            var card = CreateSimpleItemCard(item.Name, body, item.Notes);

            var buttonRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 12, 0, 0)
            };

            var paidButton = CreateSmallActionButton("Mark Paid");
            paidButton.Tag = item.Id;
            paidButton.Click += MarkPayLaterPaidButton_Click;

            var deleteButton = CreateSmallActionButton("Delete");
            deleteButton.Tag = item.Id;
            deleteButton.Click += DeletePayLaterButton_Click;

            buttonRow.Children.Add(paidButton);
            buttonRow.Children.Add(deleteButton);

            if (card.Child is StackPanel cardStack)
            {
                cardStack.Children.Add(buttonRow);
            }

            root.Children.Add(card);
        }

        panel.Child = root;
        return panel;
    }

    private void AddWeeklyCloseOutButton_Click(object sender, RoutedEventArgs e)
    {
        var weekStart = DateOnly.TryParse(_closeOutWeekStartTextBox?.Text.Trim(), out var parsedWeekStart)
            ? parsedWeekStart
            : DateOnly.FromDateTime(DateTime.Today);

        _weeklyCloseOutEntries.Add(new WeeklyCloseOutEntry
        {
            WeekStart = weekStart,
            WhatGotDone = ReadTextValue(_closeOutDoneTextBox, "Nothing recorded yet."),
            WhatMoved = ReadTextValue(_closeOutMovedTextBox, "Nothing moved yet."),
            StillWaitingOn = ReadTextValue(_closeOutWaitingTextBox, string.Empty),
            NextWeekPressure = ReadTextValue(_closeOutNextWeekTextBox, "No next-week pressure recorded."),
            Notes = _closeOutNotesTextBox?.Text.Trim() ?? string.Empty,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        });

        WeeklyCloseOutStorage.Save(_weeklyCloseOutEntries);
        ShowWeeklyCloseOutPage();
    }

    private void ResetWeeklyCloseOutButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmRiskyAction("Reset weekly close-out defaults?", "This will replace your saved close-out entries with the default local sample set."))
        {
            return;
        }

        WeeklyCloseOutStorage.Reset();
        _weeklyCloseOutEntries = WeeklyCloseOutStorage.Load();
        ShowWeeklyCloseOutPage();
    }

    private void DeleteWeeklyCloseOutButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not Guid id)
        {
            return;
        }

        if (!ConfirmRiskyAction("Delete weekly close-out entry?", "This removes the selected weekly review entry from local storage."))
        {
            return;
        }

        _weeklyCloseOutEntries = _weeklyCloseOutEntries
            .Where(entry => entry.Id != id)
            .ToList();

        WeeklyCloseOutStorage.Save(_weeklyCloseOutEntries);
        ShowWeeklyCloseOutPage();
    }

    private void ShowWeeklyCloseOutPage()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var historySummary = WeeklyCloseOutCalculator.Calculate(_weeklyCloseOutEntries, today);
        var operatingSummary = WeeklyCloseOutOperatingCalculator.Calculate(_weeklyCloseOutReviewItems);

        SetHeader(
            "Weekly Close-Out",
            $"Weekly Close-Out • v4.7 • {operatingSummary.PressureLabel} • {operatingSummary.RollForwardItems} roll forward");

        var root = new StackPanel();

        root.Children.Add(CreateHeroPanel(
            "Weekly Close-Out",
            "v4.7 closes the weekly loop across work, money, waiting-on, proof, receipt evidence, and next-week pressure. It separates what can close now from what must deliberately roll forward."));

        var metricsPanel = new WrapPanel
        {
            Margin = new Thickness(0, 22, 0, 0)
        };

        metricsPanel.Children.Add(CreateDashboardCard("Engine", "Active", "v4.7"));
        metricsPanel.Children.Add(CreateDashboardCard("Pressure", operatingSummary.PressureLabel, "Close-out"));
        metricsPanel.Children.Add(CreateDashboardCard("Review items", operatingSummary.TotalItems.ToString(), "Cross-module"));
        metricsPanel.Children.Add(CreateDashboardCard("Close now", operatingSummary.ReadyToCloseItems.ToString(), "This week"));
        metricsPanel.Children.Add(CreateDashboardCard("Roll forward", operatingSummary.RollForwardItems.ToString(), "Next week"));
        metricsPanel.Children.Add(CreateDashboardCard("Waiting", operatingSummary.WaitingItems.ToString(), "Do not chase"));
        metricsPanel.Children.Add(CreateDashboardCard("Blocked", operatingSummary.BlockedItems.ToString(), "Visible"));
        metricsPanel.Children.Add(CreateDashboardCard("Money review", operatingSummary.MoneyReviewItems.ToString(), FormatMoney(operatingSummary.MoneyStillUnderReview)));
        metricsPanel.Children.Add(CreateDashboardCard("Proof checks", operatingSummary.ProofReviewItems.ToString(), "Evidence"));
        metricsPanel.Children.Add(CreateDashboardCard("Receipt checks", operatingSummary.ReceiptReviewItems.ToString(), "v4.6"));
        metricsPanel.Children.Add(CreateDashboardCard("Work checks", operatingSummary.WorkReviewItems.ToString(), "v4.5"));
        metricsPanel.Children.Add(CreateDashboardCard("Untrusted", operatingSummary.UntrustedItems.ToString(), "Manual gate"));
        metricsPanel.Children.Add(CreateDashboardCard("Current close-out", historySummary.HasCurrentWeekCloseOut ? "Yes" : "No", "History"));

        root.Children.Add(metricsPanel);

        var rulePanel = CreateInfoPanel(
            "v4.7 Weekly Close-Out rule",
            FormatReasons(new[]
            {
                "Close completed, evidence-backed items so they stop creating pressure.",
                "Roll unfinished work forward deliberately instead of silently carrying it.",
                "Waiting-on and blocked work remain visible without consuming active sprint time.",
                "Expected money remains excluded from safe money until paid and cleared.",
                "Receipt/OCR candidates remain untrusted until source-backed and accepted.",
                "Proof, invoice, timesheet, payment, and next-action gaps must remain visible.",
                $"{operatingSummary.ReadyToCloseItems} item(s) can close now.",
                $"{operatingSummary.RollForwardItems} item(s) should roll into next week.",
                $"{FormatMoney(operatingSummary.MoneyStillUnderReview)} remains under review."
            }));
        rulePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(rulePanel);

        var controlsPanel = CreatePanel();
        controlsPanel.Margin = new Thickness(0, 16, 0, 0);
        var controlsRoot = new StackPanel();
        controlsRoot.Children.Add(new TextBlock
        {
            Text = "Local controls",
            Foreground = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });
        controlsRoot.Children.Add(new TextBlock
        {
            Text = "Reset restores fictional local review items. It does not close real work, send messages, move money, or update external systems.",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 12)
        });
        var resetReviewButton = CreateSmallActionButton("Reset v4.7 close-out review");
        resetReviewButton.Click += ResetWeeklyCloseOutReviewButton_Click;
        controlsRoot.Children.Add(resetReviewButton);
        controlsPanel.Child = controlsRoot;
        root.Children.Add(controlsPanel);

        root.Children.Add(CreateWeeklyCloseOutReviewLanePanel(
            "Close this week now",
            "Completed, trusted, evidence-backed items that can stop creating pressure.",
            operatingSummary.CloseNow));

        root.Children.Add(CreateWeeklyCloseOutReviewLanePanel(
            "Roll into next week",
            "Unfinished items that need an explicit next action, owner, and reason for carrying forward.",
            operatingSummary.RollForward));

        root.Children.Add(CreateWeeklyCloseOutReviewLanePanel(
            "Waiting / blocked / do not chase",
            "External blockers remain visible, but they should not consume active sprint time before the follow-up or unblock point.",
            operatingSummary.WaitingOrBlocked));

        root.Children.Add(CreateWeeklyCloseOutReviewLanePanel(
            "Money and payment checks",
            "Expected, reserved, invoice-ready, or payment-related money remains visible without automatically becoming safe money.",
            operatingSummary.MoneyChecks));

        root.Children.Add(CreateWeeklyCloseOutReviewLanePanel(
            "Proof / evidence checks",
            "Proof and evidence must be complete before clean handoff, invoicing, payment confidence, or portfolio use.",
            operatingSummary.ProofChecks));

        root.Children.Add(CreateWeeklyCloseOutReviewLanePanel(
            "Receipt evidence checks",
            "Receipt/OCR items remain review-first and source-gated before they affect trusted item state.",
            operatingSummary.ReceiptChecks));

        root.Children.Add(CreateWeeklyCloseOutReviewLanePanel(
            "Work Pipeline checks",
            "Active, blocked, waiting, invoice-ready, payment-expected, and parked work feeds the close-out loop.",
            operatingSummary.WorkChecks));

        var reasonsPanel = CreateInfoPanel(
            "v4.7 close-out operating signals",
            FormatReasons(operatingSummary.Reasons));
        reasonsPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(reasonsPanel);

        var inputPanel = CreateWeeklyCloseOutInputPanel();
        inputPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(inputPanel);

        var listPanel = CreateWeeklyCloseOutListPanel();
        listPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(listPanel);

        var boundaryPanel = CreateInfoPanel(
            "v4.7 boundary",
            "Local/manual weekly review only. v4.7 does not close real projects, send follow-ups, create invoices, move money, reconcile accounts, trust OCR automatically, sync external systems, or run AI actions.");
        boundaryPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(boundaryPanel);

        var nextPanel = CreateInfoPanel(
            "After v4.7",
            "Next lane: v4.8 Command Centre Pressure Engine. Weekly Close-Out now supplies a deliberate close-now, roll-forward, waiting, money, proof, receipt, and work review loop.");
        nextPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(nextPanel);

        var storagePanel = CreateInfoPanel(
            "Local weekly close-out files",
            $"{WeeklyCloseOutStorage.FilePath}{Environment.NewLine}{WeeklyCloseOutReviewStorage.FilePath}");
        storagePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(storagePanel);

        MainContentControl.Content = root;
    }


    private void ResetWeeklyCloseOutReviewButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmRiskyAction(
                "Reset v4.7 weekly close-out review?",
                "This replaces the fictional local v4.7 review items. It does not change your historical close-out entries."))
        {
            return;
        }

        WeeklyCloseOutReviewStorage.Reset();
        _weeklyCloseOutReviewItems = WeeklyCloseOutReviewStorage.Load();
        ShowWeeklyCloseOutPage();
    }

    private Border CreateWeeklyCloseOutReviewLanePanel(
        string title,
        string description,
        IEnumerable<WeeklyCloseOutReviewItem> items)
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

        var list = items.ToList();
        if (list.Count == 0)
        {
            root.Children.Add(CreateEmptyTextBlock("No close-out review items in this lane."));
            panel.Child = root;
            return panel;
        }

        foreach (var item in list)
        {
            var amountText = item.Amount > 0 ? $" • Amount: {FormatMoney(item.Amount)}" : string.Empty;
            var body =
                $"{item.SourceModule} • {item.Status} • {item.Pressure} • Owner: {item.Owner}{amountText}{Environment.NewLine}" +
                $"Next: {item.NextAction}{Environment.NewLine}" +
                $"Evidence: {item.EvidenceState}{Environment.NewLine}" +
                $"Trusted: {(item.IsTrusted ? "Yes" : "No")} • Roll forward: {(item.RollIntoNextWeek ? "Yes" : "No")}";

            root.Children.Add(CreateSimpleItemCard(item.Title, body, item.Notes));
        }

        panel.Child = root;
        return panel;
    }

    private Border CreateWeeklyCloseOutInputPanel()
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = "Add weekly close-out",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        root.Children.Add(new TextBlock
        {
            Text = "This is the weekly reset loop: done, moved, waiting, and next pressure.",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 16)
        });

        var grid = new Grid();

        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        for (var i = 0; i < 4; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        _closeOutWeekStartTextBox = CreateStandardTextBox(DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd"));
        _closeOutDoneTextBox = CreateStandardTextBox("What got done?");
        _closeOutMovedTextBox = CreateStandardTextBox("What moved?");
        _closeOutWaitingTextBox = CreateStandardTextBox("What are you still waiting on?");
        _closeOutNextWeekTextBox = CreateStandardTextBox("What pressure exists next week?");
        _closeOutNotesTextBox = CreateStandardTextBox("Optional notes");

        AddInputField(grid, "Week start yyyy-mm-dd", _closeOutWeekStartTextBox, 0, 0);
        AddInputField(grid, "What got done", _closeOutDoneTextBox, 0, 1);
        AddInputField(grid, "What moved", _closeOutMovedTextBox, 1, 0);
        AddInputField(grid, "Still waiting on", _closeOutWaitingTextBox, 1, 1);
        AddInputField(grid, "Next week pressure", _closeOutNextWeekTextBox, 2, 0);
        AddInputField(grid, "Notes", _closeOutNotesTextBox, 2, 1);

        root.Children.Add(grid);

        var buttonRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 18, 0, 0)
        };

        var addButton = CreateActionButton("Add Close-Out Entry", Color.FromRgb(56, 189, 248), Color.FromRgb(15, 23, 42));
        addButton.Click += AddWeeklyCloseOutButton_Click;

        var resetButton = CreateActionButton("Reset Defaults", Color.FromRgb(30, 41, 59), Color.FromRgb(226, 232, 240));
        resetButton.Click += ResetWeeklyCloseOutButton_Click;

        buttonRow.Children.Add(addButton);
        buttonRow.Children.Add(resetButton);

        root.Children.Add(buttonRow);

        panel.Child = root;
        return panel;
    }

    private Border CreateWeeklyCloseOutListPanel()
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = "Current close-out entries",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        if (_weeklyCloseOutEntries.Count == 0)
        {
            root.Children.Add(CreateEmptyTextBlock("No weekly close-out entries yet. Add one at the end of a real work/life block."));
            panel.Child = root;
            return panel;
        }

        foreach (var item in _weeklyCloseOutEntries.OrderByDescending(item => item.WeekStart))
        {
            var body =
                $"Done: {item.WhatGotDone}{Environment.NewLine}" +
                $"Moved: {item.WhatMoved}{Environment.NewLine}" +
                $"Waiting: {item.StillWaitingOn}{Environment.NewLine}" +
                $"Next week: {item.NextWeekPressure}";

            var card = CreateSimpleItemCard($"Week starting {item.WeekStart:yyyy-MM-dd}", body, item.Notes);

            var buttonRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 12, 0, 0)
            };

            var deleteButton = CreateSmallActionButton("Delete");
            deleteButton.Tag = item.Id;
            deleteButton.Click += DeleteWeeklyCloseOutButton_Click;

            buttonRow.Children.Add(deleteButton);

            if (card.Child is StackPanel cardStack)
            {
                cardStack.Children.Add(buttonRow);
            }

            root.Children.Add(card);
        }

        panel.Child = root;
        return panel;
    }

    private static TextBlock CreateEmptyTextBlock(string text)
    {
        return new TextBlock
        {
            Text = text,
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            Margin = new Thickness(0, 12, 0, 0),
            TextWrapping = TextWrapping.Wrap
        };
    }

    private Border CreateSimpleItemCard(string title, string body, string notes)
    {
        var card = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(15, 23, 42)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(36, 50, 68)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(16),
            Margin = new Thickness(0, 14, 0, 0)
        };

        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = title,
            Foreground = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            TextWrapping = TextWrapping.Wrap
        });

        root.Children.Add(new TextBlock
        {
            Text = body,
            Foreground = new SolidColorBrush(Color.FromRgb(203, 213, 225)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 8, 0, 0)
        });

        if (!string.IsNullOrWhiteSpace(notes))
        {
            root.Children.Add(new TextBlock
            {
                Text = notes,
                Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 6, 0, 0)
            });
        }

        card.Child = root;
        return card;
    }

    private static string ReadTextValue(TextBox? textBox, string fallback)
    {
        var value = textBox?.Text.Trim() ?? string.Empty;
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    private static string FormatReasons(IEnumerable<string> reasons)
    {
        var reasonList = reasons.ToList();

        if (reasonList.Count == 0)
        {
            return "No pressure reasons detected.";
        }

        return string.Join(Environment.NewLine, reasonList.Select(reason => $"• {reason}"));
    }



    private void AddRelationshipProfileButton_Click(object sender, RoutedEventArgs e)
    {
        var lastContactDate = DateOnly.TryParse(_relationshipLastContactTextBox?.Text.Trim(), out var parsedLastContactDate)
            ? parsedLastContactDate
            : (DateOnly?)null;

        var nextFollowUpDate = DateOnly.TryParse(_relationshipNextFollowUpTextBox?.Text.Trim(), out var parsedNextFollowUpDate)
            ? parsedNextFollowUpDate
            : (DateOnly?)null;

        var status = _relationshipStatusComboBox?.SelectedItem is RelationshipRadarStatus selectedStatus
            ? selectedStatus
            : RelationshipRadarStatus.Active;

        var waitingOn = _relationshipWaitingOnComboBox?.SelectedItem is RelationshipWaitingOn selectedWaitingOn
            ? selectedWaitingOn
            : RelationshipWaitingOn.Unknown;

        _relationshipProfiles.Add(new RelationshipRadarProfile
        {
            Name = ReadTextValue(_relationshipNameTextBox, "Unnamed relationship"),
            RoleOrContext = ReadTextValue(_relationshipContextTextBox, "No relationship context entered."),
            Status = status,
            WaitingOn = waitingOn,
            LastContactDate = lastContactDate,
            NextFollowUpDate = nextFollowUpDate,
            LinkedWork = _relationshipLinkedWorkTextBox?.Text.Trim() ?? string.Empty,
            NextAction = ReadTextValue(_relationshipNextActionTextBox, "No next action entered."),
            Notes = _relationshipNotesTextBox?.Text.Trim() ?? string.Empty,
            DoNotChase = _relationshipDoNotChaseCheckBox?.IsChecked == true,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        });

        RelationshipRadarStorage.Save(_relationshipProfiles);
        ShowRelationshipRadarPage();
    }

    private void ResetRelationshipRadarButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmRiskyAction("Reset Relationship Radar defaults?", "This will replace saved relationship profiles with fictional local demo data."))
        {
            return;
        }

        RelationshipRadarStorage.ResetToDemoData();
        _relationshipProfiles = RelationshipRadarStorage.Load();
        ShowRelationshipRadarPage();
    }

    private void CloseRelationshipProfileButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not Guid id)
        {
            return;
        }

        var profile = _relationshipProfiles.FirstOrDefault(profile => profile.Id == id);

        if (profile is null)
        {
            return;
        }

        profile.Status = RelationshipRadarStatus.Closed;
        profile.UpdatedAt = DateTime.Now;

        RelationshipRadarStorage.Save(_relationshipProfiles);
        ShowRelationshipRadarPage();
    }

    private void DeleteRelationshipProfileButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not Guid id)
        {
            return;
        }

        if (!ConfirmRiskyAction("Delete relationship profile?", "This removes the selected relationship profile from local storage."))
        {
            return;
        }

        _relationshipProfiles = _relationshipProfiles
            .Where(profile => profile.Id != id)
            .ToList();

        RelationshipRadarStorage.Save(_relationshipProfiles);
        ShowRelationshipRadarPage();
    }

    private void ShowRelationshipRadarPage()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var summary = RelationshipRadarCalculator.Calculate(_relationshipProfiles, today);

        SetHeader("Relationship Radar", $"Relationship Radar • v1.5 local relationship state • {summary.TotalProfiles} open");

        var root = new StackPanel();

        root.Children.Add(CreateHeroPanel(
            "Relationship Radar",
            "Track relationship state without exposing real inboxes, names, emails, or private client details. Use fictional/local profiles to prove waiting-on, follow-up, do-not-chase, and linked-work behaviour."));

        var metricsPanel = new WrapPanel
        {
            Margin = new Thickness(0, 22, 0, 0)
        };

        metricsPanel.Children.Add(CreateDashboardCard("Open profiles", summary.TotalProfiles.ToString(), "Relationships"));
        metricsPanel.Children.Add(CreateDashboardCard("Waiting on them", summary.WaitingOnThemCount.ToString(), "External"));
        metricsPanel.Children.Add(CreateDashboardCard("Waiting on me", summary.WaitingOnMeCount.ToString(), "Action"));
        metricsPanel.Children.Add(CreateDashboardCard("Follow-up due", summary.FollowUpDueCount.ToString(), "Today/overdue"));
        metricsPanel.Children.Add(CreateDashboardCard("Do not chase", summary.DoNotChaseCount.ToString(), "Protected"));

        root.Children.Add(metricsPanel);

        var reasonsPanel = CreateInfoPanel("Relationship pressure", FormatReasons(summary.Reasons));
        reasonsPanel.Margin = new Thickness(0, 8, 0, 0);
        root.Children.Add(reasonsPanel);

        var dueList = summary.FollowUpDueProfiles.Count == 0
            ? "No relationship follow-ups are due right now."
            : string.Join(Environment.NewLine, summary.FollowUpDueProfiles.Select(profile => $"• {profile.Name}: {profile.NextAction}"));

        var duePanel = CreateInfoPanel("Due follow-ups", dueList);
        duePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(duePanel);

        var inputPanel = CreateRelationshipRadarInputPanel();
        inputPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(inputPanel);

        var listPanel = CreateRelationshipRadarListPanel();
        listPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(listPanel);

        var guardrailPanel = CreateInfoPanel(
            "v1.5 scope",
            $"Local-only Relationship Radar with JSON persistence. Saved file: {RelationshipRadarStorage.FilePath}. No email import, contact sync, calendar sync, auto-chasing, or real client data.");

        guardrailPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(guardrailPanel);

        MainContentControl.Content = root;
    }

    private Border CreateRelationshipRadarInputPanel()
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = "Add relationship profile",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        root.Children.Add(new TextBlock
        {
            Text = "Capture who/what the relationship represents, who is waiting on who, when it was last touched, and whether it should not be chased yet.",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 16)
        });

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        for (var i = 0; i < 7; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        _relationshipNameTextBox = CreateStandardTextBox("Project Reviewer");
        _relationshipContextTextBox = CreateStandardTextBox("Workshop proof review / client portal cleanup");
        _relationshipStatusComboBox = CreateEnumComboBox(RelationshipRadarStatus.Active);
        _relationshipWaitingOnComboBox = CreateEnumComboBox(RelationshipWaitingOn.Unknown);
        _relationshipLastContactTextBox = CreateStandardTextBox(DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd"));
        _relationshipNextFollowUpTextBox = CreateStandardTextBox(DateOnly.FromDateTime(DateTime.Today).AddDays(3).ToString("yyyy-MM-dd"));
        _relationshipLinkedWorkTextBox = CreateStandardTextBox("Workshop Proof Project");
        _relationshipNextActionTextBox = CreateStandardTextBox("Next practical action");
        _relationshipNotesTextBox = CreateStandardTextBox("Optional notes");
        _relationshipDoNotChaseCheckBox = new CheckBox
        {
            Content = "Do not chase yet",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            Margin = new Thickness(0, 8, 14, 0)
        };

        AddInputField(grid, "Name", _relationshipNameTextBox, 0, 0);
        AddInputField(grid, "Context", _relationshipContextTextBox, 0, 1);
        AddInputField(grid, "Status", _relationshipStatusComboBox, 1, 0);
        AddInputField(grid, "Waiting on", _relationshipWaitingOnComboBox, 1, 1);
        AddInputField(grid, "Last contact yyyy-mm-dd", _relationshipLastContactTextBox, 2, 0);
        AddInputField(grid, "Next follow-up yyyy-mm-dd", _relationshipNextFollowUpTextBox, 2, 1);
        AddInputField(grid, "Linked work", _relationshipLinkedWorkTextBox, 3, 0);
        AddInputField(grid, "Next action", _relationshipNextActionTextBox, 3, 1);
        AddInputField(grid, "Notes", _relationshipNotesTextBox, 4, 0);

        Grid.SetRow(_relationshipDoNotChaseCheckBox, 4);
        Grid.SetColumn(_relationshipDoNotChaseCheckBox, 1);
        grid.Children.Add(_relationshipDoNotChaseCheckBox);

        root.Children.Add(grid);

        var buttonRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 18, 0, 0)
        };

        var addButton = CreateActionButton("Add Relationship Profile", Color.FromRgb(56, 189, 248), Color.FromRgb(15, 23, 42));
        addButton.Click += AddRelationshipProfileButton_Click;

        var resetButton = CreateActionButton("Reset Demo Data", Color.FromRgb(30, 41, 59), Color.FromRgb(226, 232, 240));
        resetButton.Click += ResetRelationshipRadarButton_Click;

        buttonRow.Children.Add(addButton);
        buttonRow.Children.Add(resetButton);

        root.Children.Add(buttonRow);

        panel.Child = root;
        return panel;
    }

    private Border CreateRelationshipRadarListPanel()
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = "Relationship profiles",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        var visibleProfiles = _relationshipProfiles
            .Where(profile => profile.Status != RelationshipRadarStatus.Closed)
            .OrderBy(profile => profile.DoNotChase)
            .ThenBy(profile => profile.NextFollowUpDate ?? DateOnly.MaxValue)
            .ThenBy(profile => profile.Name)
            .ToList();

        if (visibleProfiles.Count == 0)
        {
            root.Children.Add(CreateEmptyTextBlock("No open relationship profiles. Add one or reset the fictional demo data."));
            panel.Child = root;
            return panel;
        }

        foreach (var profile in visibleProfiles)
        {
            var body = $"{FormatRelationshipRadarStatus(profile.Status)} • Waiting on: {FormatRelationshipWaitingOn(profile.WaitingOn)} • Last: {FormatDate(profile.LastContactDate)} • Next: {FormatDate(profile.NextFollowUpDate)}";
            var notes = $"Linked work: {profile.LinkedWork}{Environment.NewLine}Next: {profile.NextAction}";

            if (profile.DoNotChase)
            {
                notes += Environment.NewLine + "Do not chase yet.";
            }

            if (!string.IsNullOrWhiteSpace(profile.Notes))
            {
                notes += Environment.NewLine + profile.Notes;
            }

            var card = CreateSimpleItemCard(profile.Name, body, notes);

            var buttonRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 12, 0, 0)
            };

            var closeButton = CreateSmallActionButton("Close");
            closeButton.Tag = profile.Id;
            closeButton.Click += CloseRelationshipProfileButton_Click;

            var deleteButton = CreateSmallActionButton("Delete");
            deleteButton.Tag = profile.Id;
            deleteButton.Click += DeleteRelationshipProfileButton_Click;

            buttonRow.Children.Add(closeButton);
            buttonRow.Children.Add(deleteButton);

            if (card.Child is StackPanel cardStack)
            {
                cardStack.Children.Add(buttonRow);
            }

            root.Children.Add(card);
        }

        panel.Child = root;
        return panel;
    }

    private static string FormatRelationshipRadarStatus(RelationshipRadarStatus status)
    {
        return status switch
        {
            RelationshipRadarStatus.Active => "Active",
            RelationshipRadarStatus.WaitingOnThem => "Waiting on Them",
            RelationshipRadarStatus.WaitingOnMe => "Waiting on Me",
            RelationshipRadarStatus.FollowUpDue => "Follow-up Due",
            RelationshipRadarStatus.DoNotChaseYet => "Do Not Chase Yet",
            RelationshipRadarStatus.Parked => "Parked",
            RelationshipRadarStatus.Closed => "Closed",
            _ => status.ToString()
        };
    }

    private static string FormatRelationshipWaitingOn(RelationshipWaitingOn waitingOn)
    {
        return waitingOn switch
        {
            RelationshipWaitingOn.Me => "Me",
            RelationshipWaitingOn.Them => "Them",
            RelationshipWaitingOn.Both => "Both",
            RelationshipWaitingOn.Neither => "Neither",
            RelationshipWaitingOn.Unknown => "Unknown",
            _ => waitingOn.ToString()
        };
    }

    private void AddWorkPipelineItemButton_Click(object sender, RoutedEventArgs e)
    {
        var title = ReadTextValue(_workPipelineTitleTextBox, "Untitled pipeline item");

        var stage = _workPipelineStageComboBox?.SelectedItem is WorkPipelineStage selectedStage
            ? selectedStage
            : WorkPipelineStage.LeadIdea;

        var status = _workPipelineStatusComboBox?.SelectedItem is WorkPipelineStatus selectedStatus
            ? selectedStatus
            : WorkPipelineStatus.Active;

        var priority = _workPipelinePriorityComboBox?.SelectedItem is WorkPipelinePriority selectedPriority
            ? selectedPriority
            : WorkPipelinePriority.Normal;

        var followUpDate = DateOnly.TryParse(_workPipelineFollowUpDateTextBox?.Text.Trim(), out var parsedFollowUpDate)
            ? parsedFollowUpDate
            : (DateOnly?)null;

        var expectedValue = decimal.TryParse(_workPipelineExpectedValueTextBox?.Text.Trim(), out var parsedExpectedValue)
            ? parsedExpectedValue
            : (decimal?)null;

        _workPipelineItems.Add(new WorkPipelineItem
        {
            Title = title,
            ContactName = _workPipelineContactTextBox?.Text.Trim() ?? string.Empty,
            ClientOrCompany = _workPipelineCompanyTextBox?.Text.Trim() ?? string.Empty,
            Category = _workPipelineCategoryTextBox?.Text.Trim() ?? string.Empty,
            OpportunityType = _workPipelineOpportunityTypeTextBox?.Text.Trim() ?? string.Empty,
            WaitingOn = _workPipelineWaitingOnTextBox?.Text.Trim() ?? string.Empty,
            Stage = stage,
            Status = status,
            Priority = priority,
            NextAction = ReadTextValue(_workPipelineNextActionTextBox, "No next action entered."),
            FollowUpDate = followUpDate,
            ExpectedValue = expectedValue,
            ExpectedValueNote = _workPipelineExpectedValueNoteTextBox?.Text.Trim() ?? string.Empty,
            IsBillable = _workPipelineBillableCheckBox?.IsChecked == true,
            NeedsTimesheet = _workPipelineTimesheetCheckBox?.IsChecked == true,
            NeedsInvoice = _workPipelineInvoiceCheckBox?.IsChecked == true,
            PaymentExpected = _workPipelinePaymentExpectedCheckBox?.IsChecked == true,
            Notes = _workPipelineNotesTextBox?.Text.Trim() ?? string.Empty,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        });

        WorkPipelineStorage.Save(_workPipelineItems);
        ShowWorkPipelinePage();
    }

    private void ResetWorkPipelineButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmRiskyAction("Reset Work Pipeline defaults?", "This will replace your saved Work Pipeline items with the default local sample set."))
        {
            return;
        }

        WorkPipelineStorage.Reset();
        _workPipelineItems = WorkPipelineStorage.Load();
        ShowWorkPipelinePage();
    }

    private void ArchiveWorkPipelineItemButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not Guid id)
        {
            return;
        }

        var item = _workPipelineItems.FirstOrDefault(item => item.Id == id);

        if (item is null)
        {
            return;
        }

        item.Archive();
        WorkPipelineStorage.Save(_workPipelineItems);
        ShowWorkPipelinePage();
    }

    private void DeleteWorkPipelineItemButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not Guid id)
        {
            return;
        }

        if (!ConfirmRiskyAction("Delete Work Pipeline item?", "This permanently removes the selected pipeline item from local storage."))
        {
            return;
        }

        _workPipelineItems = _workPipelineItems
            .Where(item => item.Id != id)
            .ToList();

        WorkPipelineStorage.Save(_workPipelineItems);
        ShowWorkPipelinePage();
    }

    private void WorkPipelineFilterButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string filter)
        {
            _workPipelineFilter = filter;
            ShowWorkPipelinePage();
        }
    }

    private void ShowWorkPipelinePage()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var summary = WorkPipelineCalculator.Calculate(_workPipelineItems, today);
        var operatingSummary = WorkPipelineOperatingCalculator.Calculate(_workPipelineItems, today);
        var bridgeSnapshot = WorkPipelineSpineBridgeService.Create(today);

        SetHeader("Work Pipeline", $"Work Pipeline • v4.5 • {operatingSummary.OpenItems} open • {operatingSummary.ReviewNeededItems} review");

        var root = new StackPanel();

        root.Children.Add(CreateHeroPanel(
            "Work Pipeline",
            "v4.5 turns active work, warm leads, blocked projects, follow-ups, invoice readiness, payment expected state, and proof gaps into one operating lane. This is still local/manual; it is not a CRM, accounting system, or auto-messaging layer."));

        var metricsPanel = new WrapPanel
        {
            Margin = new Thickness(0, 22, 0, 0)
        };

        metricsPanel.Children.Add(CreateDashboardCard("Mode", "v4.5", "Work lane"));
        metricsPanel.Children.Add(CreateDashboardCard("Pressure", operatingSummary.PressureLabel, "Pipeline"));
        metricsPanel.Children.Add(CreateDashboardCard("Open", operatingSummary.OpenItems.ToString(), "Pipeline"));
        metricsPanel.Children.Add(CreateDashboardCard("Active", operatingSummary.ActiveItems.ToString(), "Moving"));
        metricsPanel.Children.Add(CreateDashboardCard("Today", operatingSummary.TodayActionItems.ToString(), "Triage"));
        metricsPanel.Children.Add(CreateDashboardCard("Waiting me", operatingSummary.WaitingOnMeItems.ToString(), "Owner"));
        metricsPanel.Children.Add(CreateDashboardCard("Waiting others", operatingSummary.WaitingOnOthersItems.ToString(), "External"));
        metricsPanel.Children.Add(CreateDashboardCard("Blocked", operatingSummary.BlockedItems.ToString(), "Stop"));
        metricsPanel.Children.Add(CreateDashboardCard("Follow-ups now", operatingSummary.FollowUpsNow.ToString(), "Due"));
        metricsPanel.Children.Add(CreateDashboardCard("Follow-ups soon", operatingSummary.FollowUpsDueSoon.ToString(), "7 days"));
        metricsPanel.Children.Add(CreateDashboardCard("Timesheets", operatingSummary.TimesheetReadyItems.ToString(), "Needed"));
        metricsPanel.Children.Add(CreateDashboardCard("Invoices", operatingSummary.InvoiceReadyItems.ToString(), "Needed"));
        metricsPanel.Children.Add(CreateDashboardCard("Payments", operatingSummary.PaymentExpectedItems.ToString(), "Expected"));
        metricsPanel.Children.Add(CreateDashboardCard("Proof gaps", operatingSummary.ProofNeededItems.ToString(), "Evidence"));
        metricsPanel.Children.Add(CreateDashboardCard("Warm/parked", operatingSummary.WarmOrParkedItems.ToString(), "Review"));
        metricsPanel.Children.Add(CreateDashboardCard("Expected value", FormatMoney(operatingSummary.ExpectedValueVisible), "Visible"));
        metricsPanel.Children.Add(CreateDashboardCard("Excluded", FormatMoney(operatingSummary.ExpectedValueExcludedFromSafe), "Not safe"));
        metricsPanel.Children.Add(CreateDashboardCard("Review", operatingSummary.ReviewNeededItems.ToString(), "State"));

        root.Children.Add(metricsPanel);

        var reasonsPanel = CreateInfoPanel("v4.5 Work Pipeline rule", FormatReasons(operatingSummary.Reasons));
        reasonsPanel.Margin = new Thickness(0, 8, 0, 0);
        root.Children.Add(reasonsPanel);

        var operatingPanel = CreateWorkPipelineV45OperatingPanel(operatingSummary);
        operatingPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(operatingPanel);

        var todayPanel = CreateWorkPipelineV45LanePanel("Today triage", operatingSummary.TodayTriage, "The items that can create pressure now: blocked work, due follow-ups, invoice/timesheet actions, expected payments, or critical items.");
        todayPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(todayPanel);

        var activePanel = CreateWorkPipelineV45LanePanel("Active work lane", operatingSummary.ActiveWork, "Work that can actually move. This lane should stay small enough to act on.");
        activePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(activePanel);

        var waitingMePanel = CreateWorkPipelineV45LanePanel("Waiting on me", operatingSummary.WaitingOnMe, "Work where I owe the next move. These are priority because nobody else can unblock them.");
        waitingMePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(waitingMePanel);

        var waitingOthersPanel = CreateWorkPipelineV45LanePanel("Waiting on others", operatingSummary.WaitingOnOthers, "Work where someone else, access, payment, files, or a reply is needed before the next move.");
        waitingOthersPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(waitingOthersPanel);

        var blockedPanel = CreateWorkPipelineV45LanePanel("Blocked work", operatingSummary.BlockedWork, "Blocked work stays visible, but it should not consume deep sprint time until the blocker clears.");
        blockedPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(blockedPanel);

        var invoicePanel = CreateWorkPipelineV45LanePanel("Invoice / timesheet readiness", operatingSummary.InvoiceReadiness, "Items that may need timesheet, invoice, client-safe summary, or proof before money can move.");
        invoicePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(invoicePanel);

        var paymentPanel = CreateWorkPipelineV45LanePanel("Payment expected / not safe money", operatingSummary.PaymentExpected, "Expected pipeline money is visible for planning but remains excluded from safe money until paid/cleared.");
        paymentPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(paymentPanel);

        var proofPanel = CreateWorkPipelineV45LanePanel("Proof / evidence gaps", operatingSummary.ProofNeeded, "Items that need proof/evidence notes before they become clean handoff, invoice, or portfolio material.");
        proofPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(proofPanel);

        var warmPanel = CreateWorkPipelineV45LanePanel("Warm / parked opportunities", operatingSummary.WarmOrParked, "Warm leads and parked ideas stay contained so they do not hijack paid active work.");
        warmPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(warmPanel);

        var bridgePanel = CreateWorkPipelineV45BridgePanel(bridgeSnapshot);
        bridgePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(bridgePanel);

        var legacyStageBreakdown = summary.StageCounts.Count == 0
            ? "No open pipeline stages."
            : string.Join(Environment.NewLine, summary.StageCounts.Select(stage => $"• {FormatWorkPipelineStage(stage.Stage)}: {stage.Count}"));

        var stagePanel = CreateInfoPanel("Stage breakdown", legacyStageBreakdown);
        stagePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(stagePanel);

        var filterPanel = CreateWorkPipelineFilterPanel();
        filterPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(filterPanel);

        var inputPanel = CreateWorkPipelineInputPanel();
        inputPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(inputPanel);

        var listPanel = CreateWorkPipelineListPanel();
        listPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(listPanel);

        var guardrailPanel = CreateInfoPanel(
            "v4.5 boundary",
            $"v4.5 strengthens the local Work Pipeline operating lane. It does not send messages, sync email/calendar/accounting/bank data, create invoices, create calendar events, run AI actions, or replace a CRM. Saved file: {WorkPipelineStorage.FilePath}.");

        guardrailPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(guardrailPanel);

        var nextPanel = CreateInfoPanel(
            "After v4.5",
            "Next lane: v4.6 Receipt OCR / Evidence-to-Item. Work Pipeline is now ready to receive reviewed evidence and OCR-derived items later without trusting raw imports automatically.");

        nextPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(nextPanel);

        MainContentControl.Content = root;
    }

    private Border CreateWorkPipelineV45OperatingPanel(WorkPipelineOperatingSummary summary)
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = "v4.5 operating signals",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        if (summary.CommandCentreSignals.Count == 0)
        {
            root.Children.Add(CreateEmptyTextBlock("No pipeline operating signals right now."));
        }
        else
        {
            foreach (var signal in summary.CommandCentreSignals)
            {
                root.Children.Add(CreateSimpleItemCard(
                    signal.Label,
                    $"{signal.Value} • {signal.Priority} • {WorkPipelineOperatingCalculator.FormatReviewBucket(signal.Bucket)}",
                    signal.Detail));
            }
        }

        if (summary.IntegrityWarnings.Count > 0)
        {
            var warningText = FormatReasons(summary.IntegrityWarnings);
            var warningPanel = CreateInfoPanel("Pipeline integrity warnings", warningText);
            warningPanel.Margin = new Thickness(0, 12, 0, 0);
            root.Children.Add(warningPanel);
        }

        panel.Child = root;
        return panel;
    }

    private Border CreateWorkPipelineV45LanePanel(string title, IEnumerable<WorkPipelineItem> items, string description)
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = title,
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        root.Children.Add(new TextBlock
        {
            Text = description,
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 12)
        });

        var list = items.ToList();

        if (list.Count == 0)
        {
            root.Children.Add(CreateEmptyTextBlock("No pipeline items in this lane."));
            panel.Child = root;
            return panel;
        }

        foreach (var item in list.Take(8))
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var bucket = WorkPipelineOperatingCalculator.GetReviewBucket(item, today);
            var moneyState = WorkPipelineOperatingCalculator.GetMoneyState(item);
            var expectedText = item.ExpectedValue.HasValue ? FormatMoney(item.ExpectedValue.Value) : "No expected value";

            var body = $"{FormatWorkPipelineStage(item.Stage)} • {FormatWorkPipelineStatus(item.Status)} • {item.Priority} • {WorkPipelineOperatingCalculator.FormatReviewBucket(bucket)} • {WorkPipelineOperatingCalculator.FormatMoneyState(moneyState)} • Follow-up: {FormatDate(item.FollowUpDate)} • Expected: {expectedText}";

            var notes = new List<string>
            {
                $"Next: {item.NextAction}",
                string.IsNullOrWhiteSpace(item.WaitingOn) ? string.Empty : $"Waiting on: {item.WaitingOn}",
                string.IsNullOrWhiteSpace(item.ClientOrCompany) ? string.Empty : $"Client/company: {item.ClientOrCompany}",
                string.IsNullOrWhiteSpace(item.ExpectedValueNote) ? "Expected money is not safe money until paid." : item.ExpectedValueNote,
                string.IsNullOrWhiteSpace(item.LinkedProofNotes) ? "Proof/evidence: not linked yet." : $"Proof/evidence: {item.LinkedProofNotes}",
                string.IsNullOrWhiteSpace(item.RiskNote) ? string.Empty : $"Risk: {item.RiskNote}"
            };

            root.Children.Add(CreateSimpleItemCard(
                item.Title,
                body,
                string.Join(Environment.NewLine, notes.Where(note => !string.IsNullOrWhiteSpace(note)))));
        }

        if (list.Count > 8)
        {
            root.Children.Add(CreateEmptyTextBlock($"{list.Count - 8} additional item(s) are hidden from this panel. Use the filters below for the full list."));
        }

        panel.Child = root;
        return panel;
    }

    private Border CreateWorkPipelineV45BridgePanel(WorkPipelineOperatingSnapshot snapshot)
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = "Work Pipeline spine bridge",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        var spinePanel = CreateInfoPanel("Item/state spine bridge", FormatReasons(snapshot.SpineBridgeNotes));
        spinePanel.Margin = new Thickness(0, 10, 0, 0);
        root.Children.Add(spinePanel);

        var calendarPanel = CreateInfoPanel("Payment Calendar bridge", FormatReasons(snapshot.PaymentCalendarBridgeNotes));
        calendarPanel.Margin = new Thickness(0, 10, 0, 0);
        root.Children.Add(calendarPanel);

        var proofPanel = CreateInfoPanel("Proof / paid-work bridge", FormatReasons(snapshot.ProofBridgeNotes));
        proofPanel.Margin = new Thickness(0, 10, 0, 0);
        root.Children.Add(proofPanel);

        var storagePanel = CreateInfoPanel("Local Work Pipeline file", snapshot.StorageFilePath);
        storagePanel.Margin = new Thickness(0, 10, 0, 0);
        root.Children.Add(storagePanel);

        panel.Child = root;
        return panel;
    }

    private Border CreateWorkPipelineFocusPanel(WorkPipelineSummary summary)
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = "Today focus",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        var focusItems = summary.PriorityItems.Take(5).ToList();

        if (focusItems.Count == 0)
        {
            root.Children.Add(CreateEmptyTextBlock("No urgent pipeline focus. Pick the next active item or keep parked work parked."));
        }
        else
        {
            foreach (var item in focusItems)
            {
                root.Children.Add(CreateSimpleItemCard(
                    item.Title,
                    $"{item.Priority} • {FormatWorkPipelineStage(item.Stage)} • {FormatWorkPipelineStatus(item.Status)} • {FormatDate(item.FollowUpDate)}",
                    item.NextAction));
            }
        }

        panel.Child = root;
        return panel;
    }

    private Border CreateWorkPipelineFilterPanel()
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = "Pipeline filter",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        var row = new WrapPanel
        {
            Margin = new Thickness(0, 14, 0, 0)
        };

        foreach (var filter in new[] { "Active", "Waiting", "WaitingOnMe", "WaitingOnOthers", "Blocked", "Money", "Opportunity", "FollowUp", "Parked", "Archived", "All" })
        {
            var button = CreateSmallActionButton(filter == "FollowUp" ? "Follow-up" : filter);
            button.Tag = filter;
            button.Click += WorkPipelineFilterButton_Click;
            row.Children.Add(button);
        }

        root.Children.Add(row);
        root.Children.Add(new TextBlock
        {
            Text = $"Current filter: {_workPipelineFilter}",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 13,
            Margin = new Thickness(0, 10, 0, 0)
        });

        panel.Child = root;
        return panel;
    }

    private Border CreateWorkPipelineInputPanel()
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = "Add pipeline item",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        root.Children.Add(new TextBlock
        {
            Text = "Keep it practical: title, stage, next action, waiting-on, follow-up date, and money/admin flags.",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 16)
        });

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        for (var i = 0; i < 9; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        _workPipelineTitleTextBox = CreateStandardTextBox("Client / project / opportunity title");
        _workPipelineContactTextBox = CreateStandardTextBox("Contact person");
        _workPipelineCompanyTextBox = CreateStandardTextBox("Client or company");
        _workPipelineCategoryTextBox = CreateStandardTextBox("Paid work / proof / warm lead / portfolio");
        _workPipelineOpportunityTypeTextBox = CreateStandardTextBox("Active paid work / warm lead / parked idea");
        _workPipelineWaitingOnTextBox = CreateStandardTextBox("Who or what is blocking it?");
        _workPipelineStageComboBox = CreateEnumComboBox(WorkPipelineStage.LeadIdea);
        _workPipelineStatusComboBox = CreateEnumComboBox(WorkPipelineStatus.Active);
        _workPipelinePriorityComboBox = CreateEnumComboBox(WorkPipelinePriority.Normal);
        _workPipelineNextActionTextBox = CreateStandardTextBox("Next practical action");
        _workPipelineFollowUpDateTextBox = CreateStandardTextBox(DateOnly.FromDateTime(DateTime.Today).AddDays(3).ToString("yyyy-MM-dd"));
        _workPipelineExpectedValueTextBox = CreateStandardTextBox("0.00");
        _workPipelineExpectedValueNoteTextBox = CreateStandardTextBox("Expected money is not safe money until paid.");
        _workPipelineNotesTextBox = CreateStandardTextBox("Optional notes");
        _workPipelineBillableCheckBox = new CheckBox { Content = "Billable", Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)), Margin = new Thickness(0, 8, 14, 0) };
        _workPipelineTimesheetCheckBox = new CheckBox { Content = "Timesheet needed", Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)), Margin = new Thickness(0, 8, 14, 0) };
        _workPipelineInvoiceCheckBox = new CheckBox { Content = "Invoice needed", Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)), Margin = new Thickness(0, 8, 14, 0) };
        _workPipelinePaymentExpectedCheckBox = new CheckBox { Content = "Payment expected", Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)), Margin = new Thickness(0, 8, 14, 0) };

        AddInputField(grid, "Title", _workPipelineTitleTextBox, 0, 0);
        AddInputField(grid, "Contact", _workPipelineContactTextBox, 0, 1);
        AddInputField(grid, "Client/company", _workPipelineCompanyTextBox, 1, 0);
        AddInputField(grid, "Category", _workPipelineCategoryTextBox, 1, 1);
        AddInputField(grid, "Opportunity type", _workPipelineOpportunityTypeTextBox, 2, 0);
        AddInputField(grid, "Waiting on", _workPipelineWaitingOnTextBox, 2, 1);
        AddInputField(grid, "Stage", _workPipelineStageComboBox, 3, 0);
        AddInputField(grid, "Status", _workPipelineStatusComboBox, 3, 1);
        AddInputField(grid, "Priority", _workPipelinePriorityComboBox, 4, 0);
        AddInputField(grid, "Follow-up date yyyy-mm-dd", _workPipelineFollowUpDateTextBox, 4, 1);
        AddInputField(grid, "Expected value", _workPipelineExpectedValueTextBox, 5, 0);
        AddInputField(grid, "Expected value note", _workPipelineExpectedValueNoteTextBox, 5, 1);
        AddInputField(grid, "Next action", _workPipelineNextActionTextBox, 6, 0);
        AddInputField(grid, "Notes", _workPipelineNotesTextBox, 6, 1);

        var checkRow = new WrapPanel();
        checkRow.Children.Add(_workPipelineBillableCheckBox);
        checkRow.Children.Add(_workPipelineTimesheetCheckBox);
        checkRow.Children.Add(_workPipelineInvoiceCheckBox);
        checkRow.Children.Add(_workPipelinePaymentExpectedCheckBox);
        Grid.SetRow(checkRow, 7);
        Grid.SetColumn(checkRow, 0);
        Grid.SetColumnSpan(checkRow, 2);
        grid.Children.Add(checkRow);

        root.Children.Add(grid);

        var buttonRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 18, 0, 0)
        };

        var addButton = CreateActionButton("Add Pipeline Item", Color.FromRgb(56, 189, 248), Color.FromRgb(15, 23, 42));
        addButton.Click += AddWorkPipelineItemButton_Click;

        var resetButton = CreateActionButton("Reset Defaults", Color.FromRgb(30, 41, 59), Color.FromRgb(226, 232, 240));
        resetButton.Click += ResetWorkPipelineButton_Click;

        buttonRow.Children.Add(addButton);
        buttonRow.Children.Add(resetButton);

        root.Children.Add(buttonRow);

        panel.Child = root;
        return panel;
    }

    private Border CreateWorkPipelineListPanel()
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = "Pipeline items",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        var items = GetFilteredWorkPipelineItems().ToList();

        if (items.Count == 0)
        {
            root.Children.Add(CreateEmptyTextBlock("No pipeline items match this filter."));
            panel.Child = root;
            return panel;
        }

        foreach (var item in items)
        {
            var body = $"{FormatWorkPipelineStage(item.Stage)} • {FormatWorkPipelineStatus(item.Status)} • {item.Priority} • Follow-up: {FormatDate(item.FollowUpDate)} • Expected: {(item.ExpectedValue.HasValue ? FormatMoney(item.ExpectedValue.Value) : "None")}";
            var notes = $"Next: {item.NextAction}";

            if (!string.IsNullOrWhiteSpace(item.WaitingOn))
            {
                notes += Environment.NewLine + $"Waiting on: {item.WaitingOn}";
            }

            if (!string.IsNullOrWhiteSpace(item.ExpectedValueNote))
            {
                notes += Environment.NewLine + item.ExpectedValueNote;
            }

            if (!string.IsNullOrWhiteSpace(item.Notes))
            {
                notes += Environment.NewLine + item.Notes;
            }

            var card = CreateSimpleItemCard(item.Title, body, notes);

            var buttonRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 12, 0, 0)
            };

            if (!item.IsArchived)
            {
                var archiveButton = CreateSmallActionButton("Archive");
                archiveButton.Tag = item.Id;
                archiveButton.Click += ArchiveWorkPipelineItemButton_Click;
                buttonRow.Children.Add(archiveButton);
            }

            var deleteButton = CreateSmallActionButton("Delete");
            deleteButton.Tag = item.Id;
            deleteButton.Click += DeleteWorkPipelineItemButton_Click;
            buttonRow.Children.Add(deleteButton);

            if (card.Child is StackPanel cardStack)
            {
                cardStack.Children.Add(buttonRow);
            }

            root.Children.Add(card);
        }

        panel.Child = root;
        return panel;
    }

    private IEnumerable<WorkPipelineItem> GetFilteredWorkPipelineItems()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var dueSoonLimit = today.AddDays(7);

        return _workPipelineFilter switch
        {
            "Active" => _workPipelineItems.Where(item => item.IsOpen && item.Status == WorkPipelineStatus.Active),
            "Waiting" => _workPipelineItems.Where(item => item.IsOpen && item.IsWaiting),
            "WaitingOnMe" => WorkPipelineCalculator.BuildWaitingView(_workPipelineItems).WaitingOnMe,
            "WaitingOnOthers" => WorkPipelineCalculator.BuildWaitingView(_workPipelineItems).WaitingOnOthers,
            "Blocked" => _workPipelineItems.Where(item => item.IsOpen && item.IsBlocked),
            "Money" => _workPipelineItems.Where(item => item.IsOpen && item.IsMoneyRelated),
            "Opportunity" => _workPipelineItems.Where(item => item.IsOpen && item.IsOpportunity),
            "FollowUp" => _workPipelineItems.Where(item => item.IsOpen && item.FollowUpDate.HasValue && item.FollowUpDate.Value <= dueSoonLimit),
            "Parked" => _workPipelineItems.Where(item => item.IsOpen && item.Status == WorkPipelineStatus.Parked),
            "Archived" => WorkPipelineCalculator.GetArchivedItems(_workPipelineItems),
            "All" => _workPipelineItems,
            _ => WorkPipelineCalculator.GetVisibleItems(_workPipelineItems)
        };
    }

    private static string FormatDate(DateOnly? date)
    {
        return date.HasValue ? date.Value.ToString("yyyy-MM-dd") : "No date";
    }

    private static string FormatWorkPipelineStage(WorkPipelineStage stage)
    {
        return stage switch
        {
            WorkPipelineStage.LeadIdea => "Lead / Idea",
            WorkPipelineStage.WaitingOnReply => "Waiting on Reply",
            WorkPipelineStage.MeetingBooked => "Meeting Booked",
            WorkPipelineStage.MaterialsReceived => "Materials Received",
            WorkPipelineStage.ProofInProgress => "Proof In Progress",
            WorkPipelineStage.SentForReview => "Sent for Review",
            WorkPipelineStage.ApprovedHappy => "Approved / Happy",
            WorkPipelineStage.PaidWorkActive => "Paid Work Active",
            WorkPipelineStage.TimesheetNeeded => "Timesheet Needed",
            WorkPipelineStage.InvoiceNeeded => "Invoice Needed",
            WorkPipelineStage.PaymentExpected => "Payment Expected",
            WorkPipelineStage.KeepWarm => "Keep Warm",
            WorkPipelineStage.ClosedArchived => "Closed / Archived",
            _ => stage.ToString()
        };
    }

    private static string FormatWorkPipelineStatus(WorkPipelineStatus status)
    {
        return status switch
        {
            WorkPipelineStatus.Active => "Active",
            WorkPipelineStatus.Waiting => "Waiting",
            WorkPipelineStatus.Blocked => "Blocked",
            WorkPipelineStatus.Warm => "Warm",
            WorkPipelineStatus.Parked => "Parked",
            WorkPipelineStatus.Completed => "Completed",
            WorkPipelineStatus.Archived => "Archived",
            _ => status.ToString()
        };
    }



    private void ResetReceiptEvidenceButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmRiskyAction("Reset receipt evidence demo?", "This replaces the local receipt evidence file with fictional v4.6 demo candidates."))
        {
            return;
        }

        ReceiptEvidenceStorage.Reset();
        _receiptEvidenceItems = ReceiptEvidenceStorage.Load();
        ShowReceiptEvidencePage();
    }

    private void ShowReceiptEvidencePage()
    {
        var summary = ReceiptEvidenceCalculator.Calculate(_receiptEvidenceItems);

        SetHeader("Receipt OCR / Evidence-to-Item", $"Receipt evidence • v4.6 • {summary.ReviewCount} review • {summary.MissingSourceCount} missing source");

        var root = new StackPanel();

        root.Children.Add(CreateHeroPanel(
            "Receipt OCR / Evidence-to-Item",
            "v4.6 turns local receipt/OCR outputs into reviewable evidence candidates before they can become trusted LifeOS items. OCR is evidence intake, not money truth."));

        var metricsPanel = new WrapPanel
        {
            Margin = new Thickness(0, 22, 0, 0)
        };

        metricsPanel.Children.Add(CreateDashboardCard("Engine", "Active", "v4.6"));
        metricsPanel.Children.Add(CreateDashboardCard("Pressure", summary.PressureLabel, "Evidence"));
        metricsPanel.Children.Add(CreateDashboardCard("Candidates", summary.TotalItems.ToString(), "Local"));
        metricsPanel.Children.Add(CreateDashboardCard("Needs review", summary.ReviewCount.ToString(), "Manual gate"));
        metricsPanel.Children.Add(CreateDashboardCard("Missing source", summary.MissingSourceCount.ToString(), "Evidence"));
        metricsPanel.Children.Add(CreateDashboardCard("Accepted", summary.AcceptedCount.ToString(), "Trusted"));
        metricsPanel.Children.Add(CreateDashboardCard("Untrusted", summary.UntrustedCount.ToString(), "Blocked"));
        metricsPanel.Children.Add(CreateDashboardCard("Candidate value", FormatMoney(summary.CandidateValue), "Not safe money"));
        metricsPanel.Children.Add(CreateDashboardCard("Money candidates", summary.MoneyCandidates.Count.ToString(), "Review"));
        metricsPanel.Children.Add(CreateDashboardCard("Work proof", summary.WorkProofCandidates.Count.ToString(), "Review"));

        root.Children.Add(metricsPanel);

        var rulePanel = CreateInfoPanel("v4.6 receipt evidence rule", FormatReasons(summary.Reasons));
        rulePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(rulePanel);

        var controlsPanel = CreatePanel();
        var controlsRoot = new StackPanel();
        controlsRoot.Children.Add(new TextBlock
        {
            Text = "Local controls",
            Foreground = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });
        controlsRoot.Children.Add(new TextBlock
        {
            Text = "Reset restores fictional local demo candidates. It does not run OCR or import external data.",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            Margin = new Thickness(0, 8, 0, 12),
            TextWrapping = TextWrapping.Wrap
        });
        var resetButton = CreateActionButton("Reset receipt evidence demo", Color.FromRgb(30, 41, 59), Color.FromRgb(226, 232, 240));
        resetButton.Click += ResetReceiptEvidenceButton_Click;
        controlsRoot.Children.Add(resetButton);
        controlsPanel.Child = controlsRoot;
        controlsPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(controlsPanel);

        root.Children.Add(CreateReceiptEvidenceLanePanel("Review queue / untrusted candidates", summary.ReviewQueue));
        root.Children.Add(CreateReceiptEvidenceLanePanel("Missing source evidence", summary.MissingSourceItems));
        root.Children.Add(CreateReceiptEvidenceLanePanel("Accepted / trusted after review", summary.AcceptedItems));
        root.Children.Add(CreateReceiptEvidenceLanePanel("Money-impact candidates", summary.MoneyCandidates));
        root.Children.Add(CreateReceiptEvidenceLanePanel("Paid-work / proof candidates", summary.WorkProofCandidates));

        var boundaryPanel = CreateInfoPanel(
            "v4.6 boundary",
            "Local/manual evidence review only. No real OCR API, scanner, Gmail/Outlook import, bank feed, accounting sync, BNPL provider, OAuth flow, invoice creation, automatic payment state, or AI action is active.");
        boundaryPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(boundaryPanel);

        var nextPanel = CreateInfoPanel(
            "After v4.6",
            "Next lane: v4.7 Weekly Close-Out. Receipt evidence is now ready to feed reviewed proof and item state without trusting raw imports automatically.");
        nextPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(nextPanel);

        var storagePanel = CreateInfoPanel(
            "Local receipt evidence file",
            ReceiptEvidenceStorage.FilePath);
        storagePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(storagePanel);

        MainContentControl.Content = root;
    }

    private Border CreateReceiptEvidenceLanePanel(string title, IEnumerable<ReceiptEvidenceItem> items)
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

        var list = items.ToList();
        if (list.Count == 0)
        {
            root.Children.Add(CreateEmptyTextBlock("No receipt evidence candidates in this lane."));
        }
        else
        {
            foreach (var item in list)
            {
                var trust = item.IsTrusted ? "Trusted" : "Untrusted";
                var body = $"{item.Merchant} • {FormatMoney(item.Amount)} • {item.ReceiptDate:yyyy-MM-dd} • {item.State} • {trust}" +
                           Environment.NewLine +
                           $"Category: {item.Category} • Source: {item.SourceType} • Confidence: {item.ConfidenceLabel} • Target: {item.TargetItemType}" +
                           Environment.NewLine +
                           $"Next: {item.NextAction}";

                var notes = $"Source evidence: {(item.HasSourceEvidence ? item.SourcePathOrNote : "Missing")}" +
                            Environment.NewLine +
                            $"Notes: {item.Notes}";

                root.Children.Add(CreateSimpleItemCard(item.Title, body, notes));
            }
        }

        panel.Child = root;
        return panel;
    }

    private void AddWorkSessionButton_Click(object sender, RoutedEventArgs e)
    {
        var date = DateOnly.TryParse(_workSessionDateTextBox?.Text.Trim(), out var parsedDate)
            ? parsedDate
            : DateOnly.FromDateTime(DateTime.Today);

        var status = _workSessionStatusComboBox?.SelectedItem is WorkSessionStatus selectedStatus
            ? selectedStatus
            : WorkSessionStatus.Completed;

        _workSessions.Add(new WorkSession
        {
            ClientOrProject = ReadTextValue(_workSessionClientTextBox, "Unassigned work"),
            Date = date,
            Hours = ReadMoneyValue(_workSessionHoursTextBox, 0m),
            HourlyRate = ReadMoneyValue(_workSessionRateTextBox, 0m),
            IsBillable = _workSessionBillableCheckBox?.IsChecked == true,
            Status = status,
            Description = ReadTextValue(_workSessionDescriptionTextBox, "No description entered."),
            Notes = _workSessionNotesTextBox?.Text.Trim() ?? string.Empty,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        });

        WorkSessionStorage.Save(_workSessions);
        ShowWorkSessionsPage();
    }

    private void ResetWorkSessionsButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmRiskyAction("Reset work-session defaults?", "This will replace your saved work sessions with the default local sample set."))
        {
            return;
        }

        WorkSessionStorage.Reset();
        _workSessions = WorkSessionStorage.Load();
        ShowWorkSessionsPage();
    }

    private void MarkWorkSessionPaidButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not Guid id)
        {
            return;
        }

        var item = _workSessions.FirstOrDefault(session => session.Id == id);

        if (item is null)
        {
            return;
        }

        item.Status = WorkSessionStatus.Paid;
        item.UpdatedAt = DateTime.Now;

        WorkSessionStorage.Save(_workSessions);
        ShowWorkSessionsPage();
    }

    private void DeleteWorkSessionButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not Guid id)
        {
            return;
        }

        if (!ConfirmRiskyAction("Delete work session?", "This removes the selected work session from local storage."))
        {
            return;
        }

        _workSessions = _workSessions
            .Where(session => session.Id != id)
            .ToList();

        WorkSessionStorage.Save(_workSessions);
        ShowWorkSessionsPage();
    }

    private void ShowWorkSessionsPage()
    {
        var summary = WorkSessionCalculator.Calculate(_workSessions);

        SetHeader("Work Sessions", "Work Sessions • v0.4 work and income foundation");

        var root = new StackPanel();

        root.Children.Add(CreateHeroPanel(
            "Work Sessions",
            "Track client/project work, billable hours, expected income, unpaid value, and paid status."));

        var metricsPanel = new WrapPanel
        {
            Margin = new Thickness(0, 22, 0, 0)
        };

        metricsPanel.Children.Add(CreateDashboardCard("Total sessions", summary.TotalSessions.ToString(), "Work"));
        metricsPanel.Children.Add(CreateDashboardCard("Total hours", summary.TotalHours.ToString("0.##"), "Hours"));
        metricsPanel.Children.Add(CreateDashboardCard("Billable hours", summary.BillableHours.ToString("0.##"), "Billable"));
        metricsPanel.Children.Add(CreateDashboardCard("Billable value", FormatMoney(summary.BillableValue), "Income"));
        metricsPanel.Children.Add(CreateDashboardCard("Unpaid value", FormatMoney(summary.UnpaidBillableValue), "Not paid yet"));
        metricsPanel.Children.Add(CreateDashboardCard("Clients/projects", summary.ActiveClientOrProjectCount.ToString(), "Active"));

        root.Children.Add(metricsPanel);

        var reasonsPanel = CreateInfoPanel("Work pressure", FormatReasons(summary.Reasons));
        reasonsPanel.Margin = new Thickness(0, 8, 0, 0);
        root.Children.Add(reasonsPanel);

        var inputPanel = CreateWorkSessionInputPanel();
        inputPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(inputPanel);

        var listPanel = CreateWorkSessionListPanel();
        listPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(listPanel);

        MainContentControl.Content = root;
    }

    private Border CreateWorkSessionInputPanel()
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = "Add work session",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        root.Children.Add(new TextBlock
        {
            Text = "Bridge real work to income pressure, invoices, proof, and paid/unpaid status.",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 16)
        });

        var grid = new Grid();

        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        for (var i = 0; i < 5; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        _workSessionClientTextBox = CreateStandardTextBox("Workshop / Admin / LifeOS");
        _workSessionDateTextBox = CreateStandardTextBox(DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd"));
        _workSessionHoursTextBox = CreateStandardTextBox("1.00");
        _workSessionRateTextBox = CreateStandardTextBox("35.00");
        _workSessionStatusComboBox = CreateEnumComboBox(WorkSessionStatus.Completed);
        _workSessionDescriptionTextBox = CreateStandardTextBox("Describe the work completed");
        _workSessionNotesTextBox = CreateStandardTextBox("Optional notes");

        _workSessionBillableCheckBox = new CheckBox
        {
            Content = "Billable",
            IsChecked = true,
            Foreground = new SolidColorBrush(Color.FromRgb(203, 213, 225)),
            Margin = new Thickness(0, 8, 14, 14)
        };

        AddInputField(grid, "Client / project", _workSessionClientTextBox, 0, 0);
        AddInputField(grid, "Date yyyy-mm-dd", _workSessionDateTextBox, 0, 1);
        AddInputField(grid, "Hours", _workSessionHoursTextBox, 1, 0);
        AddInputField(grid, "Hourly rate", _workSessionRateTextBox, 1, 1);
        AddInputField(grid, "Status", _workSessionStatusComboBox, 2, 0);
        AddInputField(grid, "Description", _workSessionDescriptionTextBox, 3, 0);
        AddInputField(grid, "Notes", _workSessionNotesTextBox, 3, 1);

        Grid.SetRow(_workSessionBillableCheckBox, 2);
        Grid.SetColumn(_workSessionBillableCheckBox, 1);
        grid.Children.Add(_workSessionBillableCheckBox);

        root.Children.Add(grid);

        var buttonRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 18, 0, 0)
        };

        var addButton = CreateActionButton("Add Work Session", Color.FromRgb(56, 189, 248), Color.FromRgb(15, 23, 42));
        addButton.Click += AddWorkSessionButton_Click;

        var resetButton = CreateActionButton("Reset Defaults", Color.FromRgb(30, 41, 59), Color.FromRgb(226, 232, 240));
        resetButton.Click += ResetWorkSessionsButton_Click;

        buttonRow.Children.Add(addButton);
        buttonRow.Children.Add(resetButton);

        root.Children.Add(buttonRow);

        panel.Child = root;
        return panel;
    }

    private Border CreateWorkSessionListPanel()
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = "Current work sessions",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        if (_workSessions.Count == 0)
        {
            root.Children.Add(CreateEmptyTextBlock("No work sessions yet. Add client/project time when it may become income, proof, or follow-up pressure."));
            panel.Child = root;
            return panel;
        }

        foreach (var item in _workSessions.OrderByDescending(item => item.Date))
        {
            var body = $"{item.ClientOrProject} • {item.Date:yyyy-MM-dd} • {item.Hours:0.##}h • {FormatMoney(item.HourlyRate)}/h • {(item.IsBillable ? "Billable" : "Non-billable")} • {item.Status} • Value: {FormatMoney(item.BillableValue)}";

            var card = CreateSimpleItemCard(item.Description, body, item.Notes);

            var buttonRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 12, 0, 0)
            };

            var paidButton = CreateSmallActionButton("Mark Paid");
            paidButton.Tag = item.Id;
            paidButton.Click += MarkWorkSessionPaidButton_Click;

            var deleteButton = CreateSmallActionButton("Delete");
            deleteButton.Tag = item.Id;
            deleteButton.Click += DeleteWorkSessionButton_Click;

            buttonRow.Children.Add(paidButton);
            buttonRow.Children.Add(deleteButton);

            if (card.Child is StackPanel cardStack)
            {
                cardStack.Children.Add(buttonRow);
            }

            root.Children.Add(card);
        }

        panel.Child = root;
        return panel;
    }

    private void AddProofItemButton_Click(object sender, RoutedEventArgs e)
    {
        var date = DateOnly.TryParse(_proofDateTextBox?.Text.Trim(), out var parsedDate)
            ? parsedDate
            : DateOnly.FromDateTime(DateTime.Today);

        var type = _proofTypeComboBox?.SelectedItem is ProofType selectedType
            ? selectedType
            : ProofType.Screenshot;

        var status = _proofStatusComboBox?.SelectedItem is ProofStatus selectedStatus
            ? selectedStatus
            : ProofStatus.Draft;

        _proofItems.Add(new ProofItem
        {
            Project = ReadTextValue(_proofProjectTextBox, "LifeOS"),
            Title = ReadTextValue(_proofTitleTextBox, "Untitled proof item"),
            Type = type,
            Status = status,
            Date = date,
            Description = ReadTextValue(_proofDescriptionTextBox, "No description entered."),
            LinkOrPath = _proofLinkOrPathTextBox?.Text.Trim() ?? string.Empty,
            Notes = _proofNotesTextBox?.Text.Trim() ?? string.Empty,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        });

        ProofStorage.Save(_proofItems);
        ShowProofTrackerPage();
    }

    private void ResetProofTrackerButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmRiskyAction("Reset proof-tracker defaults?", "This will replace your saved proof items with the default local sample set."))
        {
            return;
        }

        ProofStorage.Reset();
        _proofItems = ProofStorage.Load();
        ShowProofTrackerPage();
    }

    private void MarkProofReadyButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not Guid id)
        {
            return;
        }

        var item = _proofItems.FirstOrDefault(item => item.Id == id);

        if (item is null)
        {
            return;
        }

        item.Status = ProofStatus.Ready;
        item.UpdatedAt = DateTime.Now;

        ProofStorage.Save(_proofItems);
        ShowProofTrackerPage();
    }

    private void DeleteProofItemButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not Guid id)
        {
            return;
        }

        if (!ConfirmRiskyAction("Delete proof item?", "This removes the selected proof item from local storage."))
        {
            return;
        }

        _proofItems = _proofItems
            .Where(item => item.Id != id)
            .ToList();

        ProofStorage.Save(_proofItems);
        ShowProofTrackerPage();
    }

    private void ShowProofTrackerPage()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var summary = ProofCalculator.Calculate(_proofItems, today);

        SetHeader("Proof Tracker", "Proof Tracker • v0.4 proof foundation");

        var root = new StackPanel();

        root.Children.Add(CreateHeroPanel(
            "Proof Tracker",
            "Track screenshots, releases, client replies, invoices, case studies, commits, and other proof bricks."));

        var metricsPanel = new WrapPanel
        {
            Margin = new Thickness(0, 22, 0, 0)
        };

        metricsPanel.Children.Add(CreateDashboardCard("Total proof", summary.TotalProofItems.ToString(), "Proof"));
        metricsPanel.Children.Add(CreateDashboardCard("Ready", summary.ReadyCount.ToString(), "Shareable"));
        metricsPanel.Children.Add(CreateDashboardCard("Shared", summary.SharedCount.ToString(), "Sent"));
        metricsPanel.Children.Add(CreateDashboardCard("Accepted", summary.AcceptedCount.ToString(), "Confirmed"));
        metricsPanel.Children.Add(CreateDashboardCard("Client proof", summary.ClientProofCount.ToString(), "Client/payment"));
        metricsPanel.Children.Add(CreateDashboardCard("Recent", summary.RecentCount.ToString(), "14 days"));

        root.Children.Add(metricsPanel);

        var reasonsPanel = CreateInfoPanel("Proof pressure", FormatReasons(summary.Reasons));
        reasonsPanel.Margin = new Thickness(0, 8, 0, 0);
        root.Children.Add(reasonsPanel);

        var inputPanel = CreateProofTrackerInputPanel();
        inputPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(inputPanel);

        var listPanel = CreateProofTrackerListPanel();
        listPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(listPanel);

        MainContentControl.Content = root;
    }

    private Border CreateProofTrackerInputPanel()
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = "Add proof item",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        root.Children.Add(new TextBlock
        {
            Text = "Proof is the wall. Track what was built, shown, paid, accepted, or made reusable.",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 16)
        });

        var grid = new Grid();

        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        for (var i = 0; i < 5; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        _proofProjectTextBox = CreateStandardTextBox("LifeOS");
        _proofTitleTextBox = CreateStandardTextBox("LifeOS v0.4 trust/polish release");
        _proofTypeComboBox = CreateEnumComboBox(ProofType.Release);
        _proofStatusComboBox = CreateEnumComboBox(ProofStatus.Ready);
        _proofDateTextBox = CreateStandardTextBox(DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd"));
        _proofDescriptionTextBox = CreateStandardTextBox("Describe the proof");
        _proofLinkOrPathTextBox = CreateStandardTextBox("README.md or docs path");
        _proofNotesTextBox = CreateStandardTextBox("Optional notes");

        AddInputField(grid, "Project", _proofProjectTextBox, 0, 0);
        AddInputField(grid, "Title", _proofTitleTextBox, 0, 1);
        AddInputField(grid, "Type", _proofTypeComboBox, 1, 0);
        AddInputField(grid, "Status", _proofStatusComboBox, 1, 1);
        AddInputField(grid, "Date yyyy-mm-dd", _proofDateTextBox, 2, 0);
        AddInputField(grid, "Link/path", _proofLinkOrPathTextBox, 2, 1);
        AddInputField(grid, "Description", _proofDescriptionTextBox, 3, 0);
        AddInputField(grid, "Notes", _proofNotesTextBox, 3, 1);

        root.Children.Add(grid);

        var buttonRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 18, 0, 0)
        };

        var addButton = CreateActionButton("Add Proof Item", Color.FromRgb(56, 189, 248), Color.FromRgb(15, 23, 42));
        addButton.Click += AddProofItemButton_Click;

        var resetButton = CreateActionButton("Reset Defaults", Color.FromRgb(30, 41, 59), Color.FromRgb(226, 232, 240));
        resetButton.Click += ResetProofTrackerButton_Click;

        buttonRow.Children.Add(addButton);
        buttonRow.Children.Add(resetButton);

        root.Children.Add(buttonRow);

        panel.Child = root;
        return panel;
    }

    private Border CreateProofTrackerListPanel()
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = "Current proof items",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        if (_proofItems.Count == 0)
        {
            root.Children.Add(CreateEmptyTextBlock("No proof items yet. Add releases, screenshots, client replies, invoices, or case-study evidence."));
            panel.Child = root;
            return panel;
        }

        foreach (var item in _proofItems.OrderByDescending(item => item.Date))
        {
            var body = $"{item.Project} • {item.Type} • {item.Status} • {item.Date:yyyy-MM-dd} • {item.LinkOrPath}";
            var card = CreateSimpleItemCard(item.Title, body, item.Description + Environment.NewLine + item.Notes);

            var buttonRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 12, 0, 0)
            };

            var readyButton = CreateSmallActionButton("Mark Ready");
            readyButton.Tag = item.Id;
            readyButton.Click += MarkProofReadyButton_Click;

            var deleteButton = CreateSmallActionButton("Delete");
            deleteButton.Tag = item.Id;
            deleteButton.Click += DeleteProofItemButton_Click;

            buttonRow.Children.Add(readyButton);
            buttonRow.Children.Add(deleteButton);

            if (card.Child is StackPanel cardStack)
            {
                cardStack.Children.Add(buttonRow);
            }

            root.Children.Add(card);
        }

        panel.Child = root;
        return panel;
    }

    private static string GetModuleStatusText(LifeOSModuleDefinition module)
    {
        if (module.IsDesktopOnly)
        {
            return "This is a desktop-only LifeOS utility. It can feed data into the shared LifeOS system, but it is not expected to exist as the same tray/hotkey/overlay tool on mobile.";
        }

        if (module.IsSharedCoreModule)
        {
            return "This is a shared LifeOS module. Desktop can prove the workflow first, while mobile receives the optimized daily-use version.";
        }

        return "This module is part of the LifeOS shell direction.";
    }



    private static string BuildInvoiceReadySummary(IReadOnlyCollection<WorkSession> invoiceReadySessions)
    {
        if (invoiceReadySessions.Count == 0)
        {
            return "No invoice-ready billable sessions yet. Add completed billable work sessions, then use this centre to generate a copy-ready invoice/work summary.";
        }

        var groupedSessions = invoiceReadySessions
            .OrderBy(session => session.ClientOrProject)
            .ThenBy(session => session.Date)
            .GroupBy(session => session.ClientOrProject);

        var lines = new List<string>
        {
            "Invoice-ready work summary",
            $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}",
            string.Empty
        };

        foreach (var group in groupedSessions)
        {
            var groupSessions = group.ToList();
            var totalHours = groupSessions.Sum(session => session.Hours);
            var totalValue = groupSessions.Sum(session => session.BillableValue);

            lines.Add(group.Key);
            lines.Add($"Total: {totalHours:0.##}h / {FormatMoney(totalValue)}");

            foreach (var session in groupSessions)
            {
                lines.Add($"- {session.Date:yyyy-MM-dd}: {session.Hours:0.##}h x {FormatMoney(session.HourlyRate)} = {FormatMoney(session.BillableValue)} — {session.Description}");

                if (!string.IsNullOrWhiteSpace(session.Notes))
                {
                    lines.Add($"  Notes: {session.Notes}");
                }
            }

            lines.Add(string.Empty);
        }

        lines.Add($"Grand total: {invoiceReadySessions.Sum(session => session.Hours):0.##}h / {FormatMoney(invoiceReadySessions.Sum(session => session.BillableValue))}");
        lines.Add(string.Empty);
        lines.Add("Status note: copy this into an invoice/work-summary email, then mark sessions as invoiced or paid once confirmed.");

        return string.Join(Environment.NewLine, lines);
    }

    private void ShowPaidWorkCentrePage()
    {
        var summary = WorkSessionCalculator.Calculate(_workSessions);
        var paidWorkProofSummary = PaidWorkMoneyProofSnapshotService.Create();
        var invoiceReady = _workSessions
            .Where(session => session.IsBillable && session.Status is WorkSessionStatus.Completed or WorkSessionStatus.Invoiced)
            .OrderBy(session => session.ClientOrProject)
            .ThenBy(session => session.Date)
            .ToList();

        SetHeader("Paid Work Centre", $"Paid Work / Money / Proof • v1.7 • {FormatPaidWorkMoneyProofHealth(paidWorkProofSummary.Health)}");

        var root = new StackPanel();

        root.Children.Add(CreateHeroPanel(
            "Paid Work / Money / Proof",
            "Turn work into controlled proof-backed money: log the session, check timesheet/invoice readiness, attach proof, keep expected money separate from safe money, then decide the next client-safe action."));

        var metricsPanel = new WrapPanel { Margin = new Thickness(0, 22, 0, 0) };
        metricsPanel.Children.Add(CreateDashboardCard("Health", FormatPaidWorkMoneyProofHealth(paidWorkProofSummary.Health), "v1.7"));
        metricsPanel.Children.Add(CreateDashboardCard("Admin actions", paidWorkProofSummary.AdminActionCount.ToString(), "Need review"));
        metricsPanel.Children.Add(CreateDashboardCard("Invoice-ready", FormatMoney(paidWorkProofSummary.InvoiceReadyValue), $"{paidWorkProofSummary.InvoiceReadySessionCount} session(s)"));
        metricsPanel.Children.Add(CreateDashboardCard("Unpaid billable", FormatMoney(paidWorkProofSummary.UnpaidBillableValue), "Not safe"));
        metricsPanel.Children.Add(CreateDashboardCard("Expected pipeline", FormatMoney(paidWorkProofSummary.PendingPipelineValue), "Not safe"));
        metricsPanel.Children.Add(CreateDashboardCard("Pending manual", FormatMoney(paidWorkProofSummary.PendingManualIncome), "Not safe"));
        metricsPanel.Children.Add(CreateDashboardCard("Money at risk", FormatMoney(paidWorkProofSummary.MoneyAtRisk), "Watch"));
        metricsPanel.Children.Add(CreateDashboardCard("Proof ready", paidWorkProofSummary.ReadyProofCount.ToString(), "Attach"));
        metricsPanel.Children.Add(CreateDashboardCard("Client proof", paidWorkProofSummary.ClientProofCount.ToString(), "Reusable"));
        metricsPanel.Children.Add(CreateDashboardCard("Paid value", FormatMoney(summary.PaidValue), "Landed"));
        metricsPanel.Children.Add(CreateDashboardCard("Billable hours", summary.BillableHours.ToString("0.##"), "Tracked"));
        metricsPanel.Children.Add(CreateDashboardCard("Clients/projects", summary.ActiveClientOrProjectCount.ToString(), "Spread"));
        root.Children.Add(metricsPanel);

        var spinePanel = CreateInfoPanel(
            "v1.7 paid-work spine",
            "Do work → log work session → attach proof → check timesheet/invoice readiness → prepare copy-ready client update → send manually → mark paid only when money lands.");
        spinePanel.Margin = new Thickness(0, 8, 0, 0);
        root.Children.Add(spinePanel);

        var moneyGatePanel = CreateInfoPanel(
            "Money/proof gate",
            FormatReasons(paidWorkProofSummary.Reasons));
        moneyGatePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(moneyGatePanel);

        var focusPanel = CreatePaidWorkMoneyProofFocusPanel(paidWorkProofSummary);
        focusPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(focusPanel);

        var updatePanel = CreatePaidWorkMoneyProofClientUpdatePanel(paidWorkProofSummary);
        updatePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(updatePanel);

        var summaryText = BuildInvoiceReadySummary(invoiceReady);
        var summaryPanel = CreatePanel();
        var summaryStack = new StackPanel();
        summaryStack.Children.Add(new TextBlock
        {
            Text = "Legacy invoice-ready work summary",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });
        summaryStack.Children.Add(new TextBlock
        {
            Text = "Kept for continuity. v1.7 adds the stronger money/proof gate above this older invoice-ready summary.",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 12)
        });
        summaryStack.Children.Add(new TextBox
        {
            Text = summaryText,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MinHeight = 180,
            IsReadOnly = true,
            Background = new SolidColorBrush(Color.FromRgb(15, 23, 42)),
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(51, 65, 85)),
            Padding = new Thickness(12),
            FontFamily = new FontFamily("Consolas")
        });
        summaryPanel.Child = summaryStack;
        summaryPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(summaryPanel);

        var listPanel = CreateInfoPanel(
            "Invoice-ready items",
            invoiceReady.Count == 0
                ? "No completed/invoiced billable sessions are ready yet. Add work in Work Sessions first."
                : string.Join(Environment.NewLine, invoiceReady.Select(session => $"• {session.Date:yyyy-MM-dd} | {session.ClientOrProject} | {session.Hours:0.##}h × {FormatMoney(session.HourlyRate)} = {FormatMoney(session.BillableValue)} | {session.Description}")));
        listPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(listPanel);

        var scopePanel = CreateInfoPanel(
            "v1.7 boundary",
            "This is a local-first paid-work admin and proof-readiness layer. It does not send invoices, collect payments, calculate tax, connect to accounting systems, or decide that expected money is safe.");
        scopePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(scopePanel);

        MainContentControl.Content = root;
    }

    private Border CreatePaidWorkMoneyProofFocusPanel(PaidWorkMoneyProofSummary summary)
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = "Paid-work focus queue",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        if (summary.FocusItems.Count == 0)
        {
            root.Children.Add(CreateEmptyTextBlock("No paid-work focus items need action right now."));
            panel.Child = root;
            return panel;
        }

        foreach (var item in summary.FocusItems.Take(8))
        {
            root.Children.Add(CreateSimpleItemCard(
                item.Title,
                $"{item.Priority} • {item.Source} • {item.Value}",
                item.NextAction));
        }

        panel.Child = root;
        return panel;
    }

    private Border CreatePaidWorkMoneyProofClientUpdatePanel(PaidWorkMoneyProofSummary summary)
    {
        var panel = CreatePanel();
        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = "Copy-ready client/admin update",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        root.Children.Add(new TextBlock
        {
            Text = "Manual-only. Review wording before sending. This is not invoice generation or automatic communication.",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 12)
        });

        root.Children.Add(new TextBox
        {
            Text = summary.CopyReadyClientUpdate,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MinHeight = 240,
            IsReadOnly = true,
            Background = new SolidColorBrush(Color.FromRgb(15, 23, 42)),
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(51, 65, 85)),
            Padding = new Thickness(12),
            FontFamily = new FontFamily("Consolas")
        });

        panel.Child = root;
        return panel;
    }

    private void ShowMoneyTimelinePage()
    {
        var paidWorkProofSummary = PaidWorkMoneyProofSnapshotService.Create();
        var targetDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7));
        var currentBalance = _moneyPressureInput.CurrentBalance;
        var expectedIncome = _moneyPressureInput.PaidIncome + _moneyPressureInput.PendingIncome + _workSessions
            .Where(session => session.IsBillable && session.Status is WorkSessionStatus.Completed or WorkSessionStatus.Invoiced)
            .Sum(session => session.BillableValue);
        var outgoing = _moneyPressureInput.BillsDue + _moneyPressureInput.DeductionsDue + _moneyPressureInput.FoodFuelBuffer + _moneyPressureInput.EmergencyBuffer;
        var projectedBalance = currentBalance + expectedIncome - outgoing;
        var lowestPoint = Math.Min(currentBalance, projectedBalance);
        var safeToSpend = Math.Max(0m, projectedBalance);
        var pressureLabel = projectedBalance switch
        {
            < 0 => "Danger",
            < 50 => "High",
            < 150 => "Medium",
            _ => "Calm"
        };

        SetHeader("Money Timeline", "Money Timeline • v1.7 proof-gated money view");

        var root = new StackPanel();
        root.Children.Add(CreateHeroPanel(
            "Money Timeline",
            "A first date-based cashflow view based on the paper-bills workflow: money coming in, bills going out, projected balance, lowest point, pressure label, and safe-to-spend estimate."));

        var metricsPanel = new WrapPanel { Margin = new Thickness(0, 22, 0, 0) };
        metricsPanel.Children.Add(CreateDashboardCard("Current balance", FormatMoney(currentBalance), "Starting point"));
        metricsPanel.Children.Add(CreateDashboardCard("Incoming by target", FormatMoney(expectedIncome), targetDate.ToString("yyyy-MM-dd")));
        metricsPanel.Children.Add(CreateDashboardCard("Outgoing/buffers", FormatMoney(outgoing), "Known pressure"));
        metricsPanel.Children.Add(CreateDashboardCard("Projected balance", FormatMoney(projectedBalance), pressureLabel));
        metricsPanel.Children.Add(CreateDashboardCard("Lowest point", FormatMoney(lowestPoint), "Watch"));
        metricsPanel.Children.Add(CreateDashboardCard("Safe to spend", FormatMoney(safeToSpend), "Conservative"));
        metricsPanel.Children.Add(CreateDashboardCard("Money at risk", FormatMoney(paidWorkProofSummary.MoneyAtRisk), FormatPaidWorkMoneyProofHealth(paidWorkProofSummary.Health)));
        metricsPanel.Children.Add(CreateDashboardCard("Proof ready", paidWorkProofSummary.ReadyProofCount.ToString(), "Before send"));
        root.Children.Add(metricsPanel);

        var timelinePanel = CreateInfoPanel(
            "Timeline reading",
            $"By {targetDate:yyyy-MM-dd}, LifeOS currently estimates {FormatMoney(expectedIncome)} coming in and {FormatMoney(outgoing)} going out/buffered. Projected balance is {FormatMoney(projectedBalance)}. Lowest visible point is {FormatMoney(lowestPoint)}. Pressure: {pressureLabel}.");
        timelinePanel.Margin = new Thickness(0, 8, 0, 0);
        root.Children.Add(timelinePanel);

        var sourcePanel = CreateInfoPanel(
            "What this is using in v1.7",
            "Incoming includes manual paid/pending income plus completed/invoiced billable work sessions. v1.7 also shows proof-gated money-at-risk so expected income stays separate from safe-to-spend until work, proof, invoice, and payment state are clear.");
        sourcePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(sourcePanel);

        var proofGatePanel = CreateInfoPanel(
            "Proof-gated money",
            FormatReasons(paidWorkProofSummary.Reasons));
        proofGatePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(proofGatePanel);

        var motherPanel = CreateInfoPanel(
            "Why this exists",
            "This module is the digital version of the paper-bills idea: do not just track categories; line up dates, incoming money, bills, and the lowest point so you know what is actually safe before spending.");
        motherPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(motherPanel);

        MainContentControl.Content = root;
    }




    private void ResetDailyOperatingFlowButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmRiskyAction("Reset Daily Operating Flow defaults?", "This replaces your saved daily operating flow blocks with fictional demo defaults."))
        {
            return;
        }

        DailyOperatingFlowStorage.ResetToDemoData();
        ShowDailyOperatingFlowPage();
    }

    private void StartDailyOperatingFlowBlockButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Guid id)
        {
            UpdateDailyOperatingFlowBlockStatus(id, DailyOperatingFlowStatus.InProgress);
        }
    }

    private void CompleteDailyOperatingFlowBlockButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Guid id)
        {
            UpdateDailyOperatingFlowBlockStatus(id, DailyOperatingFlowStatus.Done);
        }
    }

    private void ParkDailyOperatingFlowBlockButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Guid id)
        {
            UpdateDailyOperatingFlowBlockStatus(id, DailyOperatingFlowStatus.Parked);
        }
    }

    private void ArchiveDailyOperatingFlowBlockButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Guid id)
        {
            UpdateDailyOperatingFlowBlockStatus(id, DailyOperatingFlowStatus.Archived);
        }
    }

    private void UpdateDailyOperatingFlowBlockStatus(Guid id, DailyOperatingFlowStatus status)
    {
        var blocks = DailyOperatingFlowStorage.Load();
        var block = blocks.FirstOrDefault(block => block.Id == id);

        if (block is null)
        {
            return;
        }

        block.Status = status;
        block.UpdatedAt = DateTime.Now;

        DailyOperatingFlowStorage.Save(blocks);
        ShowDailyOperatingFlowPage();
    }

    private void ShowDailyOperatingFlowPage()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var blocks = DailyOperatingFlowStorage.Load();
        var summary = DailyOperatingFlowCalculator.Calculate(blocks, today);

        SetHeader("Daily Operating Flow", $"Daily Operating Flow • v1.6 local day control • {summary.TodayOpenCount} visible today");

        var root = new StackPanel();

        root.Children.Add(CreateHeroPanel(
            "Daily Operating Flow",
            "Control the day without turning every thought into a task. Use anchors, next actions, waiting checkpoints, low-energy fallback blocks, proof capture, recovery blocks, and stop points."));

        var metricsPanel = new WrapPanel { Margin = new Thickness(0, 22, 0, 0) };
        metricsPanel.Children.Add(CreateDashboardCard("Today visible", summary.TodayOpenCount.ToString(), "Flow"));
        metricsPanel.Children.Add(CreateDashboardCard("In progress", summary.InProgressCount.ToString(), "Now"));
        metricsPanel.Children.Add(CreateDashboardCard("Done today", summary.DoneTodayCount.ToString(), "Wins"));
        metricsPanel.Children.Add(CreateDashboardCard("Pinned", summary.PinnedCount.ToString(), "Anchors"));
        metricsPanel.Children.Add(CreateDashboardCard("High pressure", summary.HighPressureCount.ToString(), "Careful"));
        metricsPanel.Children.Add(CreateDashboardCard("Waiting", summary.WaitingCount.ToString(), "Do not force"));
        metricsPanel.Children.Add(CreateDashboardCard("Low-energy", summary.LowEnergyFallbackCount.ToString(), "Fallback"));
        metricsPanel.Children.Add(CreateDashboardCard("Stop points", summary.StopPointCount.ToString(), "Control"));
        root.Children.Add(metricsPanel);

        var buttonPanel = CreatePanel();
        var buttonRoot = new StackPanel();
        buttonRoot.Children.Add(new TextBlock
        {
            Text = "Daily flow controls",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        buttonRoot.Children.Add(new TextBlock
        {
            Text = "Use reset only for fictional demo data. Day blocks are local JSON and intentionally manual.",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 12)
        });

        var resetButton = CreateActionButton("Reset Demo Flow", Color.FromRgb(30, 41, 59), Color.FromRgb(226, 232, 240));
        resetButton.Click += ResetDailyOperatingFlowButton_Click;
        buttonRoot.Children.Add(resetButton);
        buttonPanel.Child = buttonRoot;
        buttonPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(buttonPanel);

        var todayPanel = CreateDailyOperatingFlowListPanel("Today operating blocks", summary.TodayBlocks);
        todayPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(todayPanel);

        var waitingPanel = CreateDailyOperatingFlowListPanel("Waiting / do not force", summary.WaitingBlocks.Take(6));
        waitingPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(waitingPanel);

        var recoveryPanel = CreateDailyOperatingFlowListPanel("Low-energy / recovery / stop points", summary.RecoveryBlocks.Take(8));
        recoveryPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(recoveryPanel);

        var reasonsPanel = CreateInfoPanel("Daily flow pressure", FormatReasons(summary.Reasons));
        reasonsPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(reasonsPanel);

        var guardrailPanel = CreateInfoPanel(
            "v1.6 scope",
            $"Local Daily Operating Flow blocks with JSON persistence. Saved file: {DailyOperatingFlowStorage.FilePath}. No calendar sync, notifications, AI scheduling, mobile reminders, or automatic task creation.");
        guardrailPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(guardrailPanel);

        MainContentControl.Content = root;
    }

    private Border CreateDailyOperatingFlowListPanel(string title, IEnumerable<DailyOperatingFlowBlock> blocks)
    {
        var panel = CreatePanel();
        var root = new StackPanel();
        var blockList = blocks.ToList();

        root.Children.Add(new TextBlock
        {
            Text = title,
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        if (blockList.Count == 0)
        {
            root.Children.Add(CreateEmptyTextBlock("No blocks in this section."));
            panel.Child = root;
            return panel;
        }

        foreach (var block in blockList)
        {
            var body = $"{block.Kind} • {block.Status} • {block.Pressure} • {block.TimeWindow} • {block.Area}";
            var notes = $"Next: {block.NextAction}";

            if (!string.IsNullOrWhiteSpace(block.Detail))
            {
                notes += Environment.NewLine + block.Detail;
            }

            var card = CreateSimpleItemCard(block.Title, body, notes);

            var buttonRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 12, 0, 0)
            };

            var startButton = CreateSmallActionButton("Start");
            startButton.Tag = block.Id;
            startButton.Click += StartDailyOperatingFlowBlockButton_Click;

            var doneButton = CreateSmallActionButton("Done");
            doneButton.Tag = block.Id;
            doneButton.Click += CompleteDailyOperatingFlowBlockButton_Click;

            var parkButton = CreateSmallActionButton("Park");
            parkButton.Tag = block.Id;
            parkButton.Click += ParkDailyOperatingFlowBlockButton_Click;

            var archiveButton = CreateSmallActionButton("Archive");
            archiveButton.Tag = block.Id;
            archiveButton.Click += ArchiveDailyOperatingFlowBlockButton_Click;

            buttonRow.Children.Add(startButton);
            buttonRow.Children.Add(doneButton);
            buttonRow.Children.Add(parkButton);
            buttonRow.Children.Add(archiveButton);

            if (card.Child is StackPanel cardStack)
            {
                cardStack.Children.Add(buttonRow);
            }

            root.Children.Add(card);
        }

        panel.Child = root;
        return panel;
    }

    private void ShowDailyStatePage()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var dailyItems = DailyStateStorage.Load();
        var dailySummary = DailyStateCalculator.Calculate(dailyItems, today);
        var scheduledItems = ScheduledCommunicationStorage.Load();
        var scheduledSummary = ScheduledCommunicationCalculator.Calculate(scheduledItems, today);

        SetHeader("Daily State", "Daily State • v1.3 passive waiting / scheduled communication");

        var root = new StackPanel();

        root.Children.Add(CreateHeroPanel(
            "Daily State",
            "Track today’s operational state without turning everything into a task. Passive waiting, do-not-chase, scheduled communication, low-energy options, and stop points stay visible without creating noise."));

        var metricsPanel = new WrapPanel { Margin = new Thickness(0, 22, 0, 0) };
        metricsPanel.Children.Add(CreateDashboardCard("Today visible", dailySummary.TodayOpenCount.ToString(), "Daily State"));
        metricsPanel.Children.Add(CreateDashboardCard("Done today", dailySummary.DoneTodayCount.ToString(), "Wins"));
        metricsPanel.Children.Add(CreateDashboardCard("Passive waiting", dailySummary.PassiveWaitingCount.ToString(), "Do not force"));
        metricsPanel.Children.Add(CreateDashboardCard("Do not chase", dailySummary.DoNotChaseCount.ToString(), "Protected"));
        metricsPanel.Children.Add(CreateDashboardCard("Scheduled", scheduledSummary.PlannedTodayCount.ToString(), "Communication"));
        metricsPanel.Children.Add(CreateDashboardCard("Waiting after send", scheduledSummary.WaitingAfterSendCount.ToString(), "Passive"));
        root.Children.Add(metricsPanel);

        var todayPanel = CreateInfoPanel(
            "Today state",
            dailySummary.TodayItems.Count == 0
                ? "No daily state items are visible for today."
                : FormatReasons(dailySummary.TodayItems.Select(item => $"{item.Type} • {item.Title}: {item.NextAction}")));
        todayPanel.Margin = new Thickness(0, 8, 0, 0);
        root.Children.Add(todayPanel);

        var hiddenPanel = CreateInfoPanel(
            "Passive / hidden from Today",
            dailySummary.HiddenItems.Count == 0
                ? "No passive waiting or do-not-chase items are currently parked."
                : FormatReasons(dailySummary.HiddenItems.Take(8).Select(item => $"{item.Status} • {item.Title}: {item.NextAction}")));
        hiddenPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(hiddenPanel);

        var scheduledPanel = CreateInfoPanel(
            "Scheduled communication",
            scheduledSummary.PlannedToday.Count == 0 && scheduledSummary.WaitingAfterSend.Count == 0
                ? "No scheduled communication pressure today."
                : FormatReasons(
                    scheduledSummary.PlannedToday.Select(item => $"Planned • {item.Recipient} • {item.ScheduledAt:g}: {item.Purpose}")
                    .Concat(scheduledSummary.WaitingAfterSend.Select(item => $"Waiting after send • {item.Recipient}: {item.Purpose}"))));
        scheduledPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(scheduledPanel);

        var reasonsPanel = CreateInfoPanel("Daily state pressure", FormatReasons(dailySummary.Reasons.Concat(scheduledSummary.Reasons)));
        reasonsPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(reasonsPanel);

        var storagePanel = CreateInfoPanel(
            "Local storage",
            $"Daily state: {DailyStateStorage.FilePath}{Environment.NewLine}Scheduled communication: {ScheduledCommunicationStorage.FilePath}");
        storagePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(storagePanel);

        MainContentControl.Content = root;
    }

    private void ShowTimesheetEvidencePage()
    {
        var entries = TimesheetEvidenceStorage.Load();
        var summary = TimesheetEvidenceCalculator.Calculate(entries);

        SetHeader("Timesheet Evidence", "Timesheet Evidence • v1.3 proof-linked billing support");

        var root = new StackPanel();

        root.Children.Add(CreateHeroPanel(
            "Timesheet Evidence",
            "Turn work activity into client-safe timesheet descriptions before the detail fades. This is manual/local-first support for billable work, proof links, and invoice readiness."));

        var metricsPanel = new WrapPanel { Margin = new Thickness(0, 22, 0, 0) };
        metricsPanel.Children.Add(CreateDashboardCard("Total entries", summary.TotalEntries.ToString(), "Evidence"));
        metricsPanel.Children.Add(CreateDashboardCard("Ready", summary.ReadyForTimesheetCount.ToString(), "Timesheet"));
        metricsPanel.Children.Add(CreateDashboardCard("Drafts", summary.DraftCount.ToString(), "Needs cleanup"));
        metricsPanel.Children.Add(CreateDashboardCard("Suggested hours", summary.SuggestedHoursTotal.ToString("0.##"), "Ready value"));
        root.Children.Add(metricsPanel);

        var readyPanel = CreateInfoPanel(
            "Ready for timesheet",
            summary.ReadyEntries.Count == 0
                ? "No entries are ready for timesheet yet."
                : FormatReasons(summary.ReadyEntries.Take(8).Select(entry => $"{entry.Date:yyyy-MM-dd} • {entry.ClientOrProject} • {entry.SuggestedHours:0.##}h: {entry.Description}")));
        readyPanel.Margin = new Thickness(0, 8, 0, 0);
        root.Children.Add(readyPanel);

        var recentPanel = CreateInfoPanel(
            "Recent evidence entries",
            entries.Count == 0
                ? "No timesheet evidence entries have been captured yet."
                : FormatReasons(entries.OrderByDescending(entry => entry.Date).Take(8).Select(entry => $"{entry.Status} • {entry.Type} • {entry.ClientOrProject}: {entry.Description}")));
        recentPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(recentPanel);

        var rulesPanel = CreateInfoPanel(
            "Time bucket rules",
            "0.25h = light admin / quick check / short reply.\n0.5h = real investigation / review / setup check / structured follow-up.\n1.0h+ = implementation / testing / proof build / debugging / documentation.");
        rulesPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(rulesPanel);

        var reasonsPanel = CreateInfoPanel("Timesheet evidence pressure", FormatReasons(summary.Reasons));
        reasonsPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(reasonsPanel);

        var storagePanel = CreateInfoPanel("Local storage", TimesheetEvidenceStorage.FilePath);
        storagePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(storagePanel);

        MainContentControl.Content = root;
    }

    private void ShowEvidenceVaultPage()
    {
        var items = EvidenceVaultStorage.Load()
            .OrderByDescending(item => item.EvidenceDate)
            .ThenBy(item => item.Title)
            .ToList();
        var summary = EvidenceVaultCalculator.Calculate(items);

        SetHeader("Evidence Vault", "Evidence Vault • v1.4 usable local evidence workspace");

        var root = new StackPanel();

        root.Children.Add(CreateHeroPanel(
            "Evidence Vault",
            "Capture evidence metadata, review what is missing, link proof to work context, and keep source references local-first. This is manual proof control before OCR, mobile capture, cloud sync, or provider integrations."));

        var metricsPanel = new WrapPanel { Margin = new Thickness(0, 22, 0, 0) };
        metricsPanel.Children.Add(CreateDashboardCard("Total evidence", summary.TotalItems.ToString(), "Vault"));
        metricsPanel.Children.Add(CreateDashboardCard("Open", summary.OpenItems.ToString(), "Active"));
        metricsPanel.Children.Add(CreateDashboardCard("Needs review", summary.NeedsReviewCount.ToString(), "Pressure"));
        metricsPanel.Children.Add(CreateDashboardCard("Screenshots", summary.ScreenshotCount.ToString(), "Proof"));
        metricsPanel.Children.Add(CreateDashboardCard("Emails/messages", summary.EmailProofCount.ToString(), "Communication"));
        metricsPanel.Children.Add(CreateDashboardCard("Timesheet proof", summary.TimesheetProofCount.ToString(), "Admin"));
        metricsPanel.Children.Add(CreateDashboardCard("Commits", summary.CommitProofCount.ToString(), "Repo"));
        root.Children.Add(metricsPanel);

        var formPanel = CreatePanel();
        formPanel.Margin = new Thickness(0, 8, 0, 0);
        var form = new StackPanel();

        form.Children.Add(new TextBlock
        {
            Text = "Add evidence record",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 18,
            FontWeight = FontWeights.Bold
        });

        form.Children.Add(new TextBlock
        {
            Text = "Use this for screenshots, email/message proof, work notes, commits, test results, documents, receipts, invoices, and client-safe proof references.",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 8, 0, 14)
        });

        var titleTextBox = CreateTextBox("Evidence title");
        var contextTextBox = CreateTextBox("Linked project/client/context");
        var sourceTextBox = CreateTextBox("Source path or reference");
        var summaryTextBox = CreateTextBox("Summary / what this proves");
        summaryTextBox.AcceptsReturn = true;
        summaryTextBox.MinHeight = 70;
        var notesTextBox = CreateTextBox("Notes / review comments");
        notesTextBox.AcceptsReturn = true;
        notesTextBox.MinHeight = 70;

        var typeComboBox = new ComboBox
        {
            ItemsSource = Enum.GetValues<EvidenceVaultItemType>(),
            SelectedItem = EvidenceVaultItemType.Screenshot,
            Margin = new Thickness(0, 0, 0, 10),
            Padding = new Thickness(10),
            Background = new SolidColorBrush(Color.FromRgb(15, 23, 42)),
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240))
        };

        var statusComboBox = new ComboBox
        {
            ItemsSource = Enum.GetValues<EvidenceVaultStatus>(),
            SelectedItem = EvidenceVaultStatus.Captured,
            Margin = new Thickness(0, 0, 0, 10),
            Padding = new Thickness(10),
            Background = new SolidColorBrush(Color.FromRgb(15, 23, 42)),
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240))
        };

        var dateTextBox = CreateTextBox(DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd"));
        var needsReviewCheckBox = new CheckBox
        {
            Content = "Needs review",
            Foreground = new SolidColorBrush(Color.FromRgb(203, 213, 225)),
            Margin = new Thickness(0, 0, 0, 12)
        };

        form.Children.Add(CreateFieldLabel("Type"));
        form.Children.Add(typeComboBox);
        form.Children.Add(CreateFieldLabel("Status"));
        form.Children.Add(statusComboBox);
        form.Children.Add(CreateFieldLabel("Evidence date"));
        form.Children.Add(dateTextBox);
        form.Children.Add(CreateFieldLabel("Title"));
        form.Children.Add(titleTextBox);
        form.Children.Add(CreateFieldLabel("Linked context"));
        form.Children.Add(contextTextBox);
        form.Children.Add(CreateFieldLabel("Source/reference"));
        form.Children.Add(sourceTextBox);
        form.Children.Add(CreateFieldLabel("Summary"));
        form.Children.Add(summaryTextBox);
        form.Children.Add(CreateFieldLabel("Notes"));
        form.Children.Add(notesTextBox);
        form.Children.Add(needsReviewCheckBox);

        var addButton = new Button
        {
            Content = "Add Evidence Record",
            Padding = new Thickness(14, 10, 14, 10),
            Background = new SolidColorBrush(Color.FromRgb(14, 165, 233)),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            FontWeight = FontWeights.SemiBold,
            Cursor = System.Windows.Input.Cursors.Hand
        };

        addButton.Click += (_, _) =>
        {
            var title = titleTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                title = "Untitled evidence";
            }

            if (!DateOnly.TryParse(dateTextBox.Text.Trim(), out var evidenceDate))
            {
                evidenceDate = DateOnly.FromDateTime(DateTime.Today);
            }

            var newItem = new EvidenceVaultItem
            {
                Title = title,
                Type = typeComboBox.SelectedItem is EvidenceVaultItemType selectedType ? selectedType : EvidenceVaultItemType.Other,
                Status = statusComboBox.SelectedItem is EvidenceVaultStatus selectedStatus ? selectedStatus : EvidenceVaultStatus.Captured,
                EvidenceDate = evidenceDate,
                ProjectOrClient = contextTextBox.Text.Trim(),
                SourcePathOrReference = sourceTextBox.Text.Trim(),
                Summary = summaryTextBox.Text.Trim(),
                Notes = notesTextBox.Text.Trim(),
                NeedsReview = needsReviewCheckBox.IsChecked == true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            newItem.NeedsReview = newItem.NeedsReview || EvidenceVaultReviewRules.IsReviewRequired(newItem);

            var saved = EvidenceVaultStorage.Load();
            saved.Add(newItem);
            EvidenceVaultStorage.Save(saved);
            ShowEvidenceVaultPage();
        };

        form.Children.Add(addButton);
        formPanel.Child = form;
        root.Children.Add(formPanel);

        var reviewPanel = CreateInfoPanel(
            "Evidence needing review",
            summary.NeedsReviewItems.Count == 0
                ? "No evidence items currently need review."
                : FormatReasons(summary.NeedsReviewItems.Take(8).Select(item => $"{item.Type} • {item.Title} • {item.ProjectOrClient}: {EvidenceVaultReviewRules.GetReviewReason(item)}")));
        reviewPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(reviewPanel);

        var listPanel = CreatePanel();
        listPanel.Margin = new Thickness(0, 16, 0, 0);
        var listStack = new StackPanel();
        listStack.Children.Add(new TextBlock
        {
            Text = "Evidence records",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 18,
            FontWeight = FontWeights.Bold
        });

        if (items.Count == 0)
        {
            listStack.Children.Add(new TextBlock
            {
                Text = "No Evidence Vault records have been captured yet.",
                Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                Margin = new Thickness(0, 10, 0, 0)
            });
        }
        else
        {
            foreach (var item in items.Take(20))
            {
                var itemPanel = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(15, 23, 42)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(12),
                    Padding = new Thickness(14),
                    Margin = new Thickness(0, 12, 0, 0)
                };

                var itemStack = new StackPanel();
                itemStack.Children.Add(new TextBlock
                {
                    Text = $"{item.Status} • {item.Type} • {item.Title}",
                    Foreground = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
                    FontSize = 15,
                    FontWeight = FontWeights.SemiBold,
                    TextWrapping = TextWrapping.Wrap
                });

                itemStack.Children.Add(new TextBlock
                {
                    Text = $"{item.EvidenceDate:yyyy-MM-dd} • {item.ProjectOrClient} • {item.SourcePathOrReference}",
                    Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                    FontSize = 12,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 6, 0, 0)
                });

                itemStack.Children.Add(new TextBlock
                {
                    Text = string.IsNullOrWhiteSpace(item.Summary) ? "No summary entered." : item.Summary,
                    Foreground = new SolidColorBrush(Color.FromRgb(203, 213, 225)),
                    FontSize = 13,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 8, 0, 0)
                });

                if (EvidenceVaultReviewRules.IsReviewRequired(item))
                {
                    itemStack.Children.Add(new TextBlock
                    {
                        Text = $"Review: {EvidenceVaultReviewRules.GetReviewReason(item)}",
                        Foreground = new SolidColorBrush(Color.FromRgb(251, 191, 36)),
                        FontSize = 12,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 8, 0, 0)
                    });
                }

                var actions = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 12, 0, 0)
                };

                actions.Children.Add(CreateEvidenceActionButton("Reviewed", () => UpdateEvidenceStatus(item.Id, EvidenceVaultStatus.Reviewed, false)));
                actions.Children.Add(CreateEvidenceActionButton("Needs Review", () => UpdateEvidenceStatus(item.Id, EvidenceVaultStatus.NeedsReview, true)));
                actions.Children.Add(CreateEvidenceActionButton("Archive", () => UpdateEvidenceStatus(item.Id, EvidenceVaultStatus.Archived, false)));
                actions.Children.Add(CreateEvidenceActionButton("Delete", () => DeleteEvidenceItem(item.Id)));
                itemStack.Children.Add(actions);

                itemPanel.Child = itemStack;
                listStack.Children.Add(itemPanel);
            }
        }

        listPanel.Child = listStack;
        root.Children.Add(listPanel);

        var reasonsPanel = CreateInfoPanel("Evidence Vault pressure", FormatReasons(summary.Reasons));
        reasonsPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(reasonsPanel);

        var storagePanel = CreateInfoPanel("Local storage", EvidenceVaultStorage.FilePath);
        storagePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(storagePanel);

        MainContentControl.Content = root;
    }

    private static TextBlock CreateFieldLabel(string label)
    {
        return new TextBlock
        {
            Text = label,
            Foreground = new SolidColorBrush(Color.FromRgb(203, 213, 225)),
            FontSize = 12,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 4, 0, 4)
        };
    }

    private static TextBox CreateTextBox(string text = "")
    {
        return new TextBox
        {
            Text = text,
            Margin = new Thickness(0, 0, 0, 10),
            Padding = new Thickness(10),
            Background = new SolidColorBrush(Color.FromRgb(15, 23, 42)),
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(51, 65, 85)),
            BorderThickness = new Thickness(1),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap
        };
    }

    private Button CreateEvidenceActionButton(string label, Action action)
    {
        var button = new Button
        {
            Content = label,
            Margin = new Thickness(0, 0, 8, 0),
            Padding = new Thickness(10, 6, 10, 6),
            Background = new SolidColorBrush(Color.FromRgb(30, 41, 59)),
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(51, 65, 85)),
            BorderThickness = new Thickness(1),
            Cursor = System.Windows.Input.Cursors.Hand
        };

        button.Click += (_, _) => action();
        return button;
    }

    private void UpdateEvidenceStatus(Guid id, EvidenceVaultStatus status, bool needsReview)
    {
        var items = EvidenceVaultStorage.Load();
        var item = items.FirstOrDefault(candidate => candidate.Id == id);
        if (item is null)
        {
            return;
        }

        item.Status = status;
        item.NeedsReview = needsReview;
        item.UpdatedAt = DateTime.Now;
        EvidenceVaultStorage.Save(items);
        ShowEvidenceVaultPage();
    }

    private void DeleteEvidenceItem(Guid id)
    {
        if (!ConfirmRiskyAction("Delete evidence record?", "This removes the local Evidence Vault metadata record. It does not delete the original source file."))
        {
            return;
        }

        var items = EvidenceVaultStorage.Load();
        items.RemoveAll(item => item.Id == id);
        EvidenceVaultStorage.Save(items);
        ShowEvidenceVaultPage();
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

        SetHeader("Command Centre", $"Unified Command Centre • v4.7 • {summary.OverallPressureLabel}");

        var root = new StackPanel();

        root.Children.Add(CreateHeroPanel(
            "LifeOS Command Centre",
            "LifeOS now turns local module data into a unified signal list: paid work that can move today, follow-ups due, blocked/waiting client work, timesheet/invoice/payment warnings, and safe-money pressure."));

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
            summary.NextSafestAction);

        actionPanel.Margin = new Thickness(0, 8, 0, 0);
        root.Children.Add(actionPanel);


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
            FormatReasons(summary.WorkPipeline.CommandCentreSignals.Select(signal => $"{signal.Label}: {signal.Value} — {signal.Detail}")));

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

    private Border CreateMoneyPressureInputPanel()
    {
        var panel = CreatePanel();

        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = "Manual weekly inputs",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        root.Children.Add(new TextBlock
        {
            Text = "These values are temporary for Phase 9. They recalculate the summary but are not saved yet.",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 16)
        });

        var grid = new Grid();

        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        for (var i = 0; i < 3; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        _currentBalanceTextBox = CreateMoneyInputTextBox(_moneyPressureInput.CurrentBalance);
        _paidIncomeTextBox = CreateMoneyInputTextBox(_moneyPressureInput.PaidIncome);
        _pendingIncomeTextBox = CreateMoneyInputTextBox(_moneyPressureInput.PendingIncome);
        _billsDueTextBox = CreateMoneyInputTextBox(_moneyPressureInput.BillsDue);
        _deductionsDueTextBox = CreateMoneyInputTextBox(_moneyPressureInput.DeductionsDue);
        _foodFuelBufferTextBox = CreateMoneyInputTextBox(_moneyPressureInput.FoodFuelBuffer);
        _emergencyBufferTextBox = CreateMoneyInputTextBox(_moneyPressureInput.EmergencyBuffer);

        AddInputField(grid, "Current balance", _currentBalanceTextBox, 0, 0);
        AddInputField(grid, "Paid income", _paidIncomeTextBox, 0, 1);
        AddInputField(grid, "Pending income", _pendingIncomeTextBox, 0, 2);

        AddInputField(grid, "Bills due", _billsDueTextBox, 1, 0);
        AddInputField(grid, "Deductions", _deductionsDueTextBox, 1, 1);
        AddInputField(grid, "Food/fuel buffer", _foodFuelBufferTextBox, 1, 2);

        AddInputField(grid, "Emergency buffer", _emergencyBufferTextBox, 2, 0);

        root.Children.Add(grid);

        var buttonRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 18, 0, 0)
        };

        var recalculateButton = new Button
        {
            Content = "Recalculate Money Pressure",
            Padding = new Thickness(16, 10, 16, 10),
            Margin = new Thickness(0, 0, 10, 0),
            HorizontalAlignment = HorizontalAlignment.Left,
            Background = new SolidColorBrush(Color.FromRgb(56, 189, 248)),
            Foreground = new SolidColorBrush(Color.FromRgb(15, 23, 42)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(56, 189, 248)),
            FontWeight = FontWeights.SemiBold,
            Cursor = System.Windows.Input.Cursors.Hand
        };

        recalculateButton.Click += RecalculateMoneyPressureButton_Click;

        var saveButton = new Button
        {
            Content = "Save Inputs",
            Padding = new Thickness(16, 10, 16, 10),
            Margin = new Thickness(0, 0, 10, 0),
            HorizontalAlignment = HorizontalAlignment.Left,
            Background = new SolidColorBrush(Color.FromRgb(34, 197, 94)),
            Foreground = new SolidColorBrush(Color.FromRgb(15, 23, 42)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(34, 197, 94)),
            FontWeight = FontWeights.SemiBold,
            Cursor = System.Windows.Input.Cursors.Hand
        };

        saveButton.Click += SaveMoneyPressureButton_Click;

        var resetButton = new Button
        {
            Content = "Reset Defaults",
            Padding = new Thickness(16, 10, 16, 10),
            HorizontalAlignment = HorizontalAlignment.Left,
            Background = new SolidColorBrush(Color.FromRgb(30, 41, 59)),
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(51, 65, 85)),
            FontWeight = FontWeights.SemiBold,
            Cursor = System.Windows.Input.Cursors.Hand
        };

        resetButton.Click += ResetMoneyPressureButton_Click;

        buttonRow.Children.Add(recalculateButton);
        buttonRow.Children.Add(saveButton);
        buttonRow.Children.Add(resetButton);

        root.Children.Add(buttonRow);

        panel.Child = root;
        return panel;
    }

    private Border CreateFollowUpInputPanel()
    {
        var panel = CreatePanel();

        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = "Add follow-up",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        root.Children.Add(new TextBlock
        {
            Text = "Keep this lightweight. This is for waiting-on items, client replies, payment confirmation, and next-action dates.",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 16)
        });

        var grid = new Grid();

        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        for (var i = 0; i < 5; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        _followUpPersonTextBox = CreateStandardTextBox("Example: Project Reviewer / Admin Workflow");
        _followUpContextTextBox = CreateStandardTextBox("Example: OnboardingFlow scoped work");
        _followUpNextActionTextBox = CreateStandardTextBox("Example: Follow up Monday 20 July");
        _followUpDateTextBox = CreateStandardTextBox(DateOnly.FromDateTime(DateTime.Today).AddDays(3).ToString("yyyy-MM-dd"));
        _followUpNotesTextBox = CreateStandardTextBox("Optional notes");

        _followUpStatusComboBox = CreateEnumComboBox(FollowUpStatus.Waiting);
        _followUpPriorityComboBox = CreateEnumComboBox(FollowUpPriority.Normal);

        _followUpMoneyLinkedCheckBox = new CheckBox
        {
            Content = "Money-linked / paid-work linked",
            Foreground = new SolidColorBrush(Color.FromRgb(203, 213, 225)),
            Margin = new Thickness(0, 8, 14, 14)
        };

        AddInputField(grid, "Person / organisation", _followUpPersonTextBox, 0, 0);
        AddInputField(grid, "Context", _followUpContextTextBox, 0, 1);
        AddInputField(grid, "Next action", _followUpNextActionTextBox, 1, 0);
        AddInputField(grid, "Follow-up date (yyyy-mm-dd)", _followUpDateTextBox, 1, 1);
        AddInputField(grid, "Status", _followUpStatusComboBox, 2, 0);
        AddInputField(grid, "Priority", _followUpPriorityComboBox, 2, 1);
        AddInputField(grid, "Notes", _followUpNotesTextBox, 3, 0);

        Grid.SetRow(_followUpMoneyLinkedCheckBox, 3);
        Grid.SetColumn(_followUpMoneyLinkedCheckBox, 1);
        grid.Children.Add(_followUpMoneyLinkedCheckBox);

        root.Children.Add(grid);

        var buttonRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 18, 0, 0)
        };

        var addButton = CreateActionButton("Add Follow-Up", Color.FromRgb(56, 189, 248), Color.FromRgb(15, 23, 42));
        addButton.Click += AddFollowUpButton_Click;

        var saveButton = CreateActionButton("Save Follow-Ups", Color.FromRgb(34, 197, 94), Color.FromRgb(15, 23, 42));
        saveButton.Click += SaveFollowUpsButton_Click;

        var resetButton = CreateActionButton("Reset Defaults", Color.FromRgb(30, 41, 59), Color.FromRgb(226, 232, 240));
        resetButton.Click += ResetFollowUpsButton_Click;

        buttonRow.Children.Add(addButton);
        buttonRow.Children.Add(saveButton);
        buttonRow.Children.Add(resetButton);

        root.Children.Add(buttonRow);

        panel.Child = root;
        return panel;
    }

    private Border CreateFollowUpListPanel()
    {
        var panel = CreatePanel();

        var root = new StackPanel();

        root.Children.Add(new TextBlock
        {
            Text = "Current follow-ups",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        if (_followUps.Count == 0)
        {
            root.Children.Add(new TextBlock
            {
                Text = "No follow-ups yet. Add people, invoices, replies, or loose threads you need to track.",
                Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                Margin = new Thickness(0, 12, 0, 0)
            });

            panel.Child = root;
            return panel;
        }

        foreach (var item in _followUps.OrderBy(item => item.FollowUpDate ?? DateOnly.MaxValue))
        {
            root.Children.Add(CreateFollowUpItemCard(item));
        }

        panel.Child = root;
        return panel;
    }

    private Border CreateFollowUpItemCard(FollowUpItem item)
    {
        var card = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(15, 23, 42)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(36, 50, 68)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(16),
            Margin = new Thickness(0, 14, 0, 0)
        };

        var root = new StackPanel();

        var header = new DockPanel();

        header.Children.Add(new TextBlock
        {
            Text = item.PersonOrOrganisation,
            Foreground = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
            FontSize = 18,
            FontWeight = FontWeights.Bold
        });

        var badge = new TextBlock
        {
            Text = $"{item.Status} • {item.Priority}",
            Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248)),
            FontSize = 12,
            FontWeight = FontWeights.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        DockPanel.SetDock(badge, Dock.Right);
        header.Children.Add(badge);

        root.Children.Add(header);

        root.Children.Add(new TextBlock
        {
            Text = item.Context,
            Foreground = new SolidColorBrush(Color.FromRgb(203, 213, 225)),
            FontSize = 14,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 8, 0, 0)
        });

        root.Children.Add(new TextBlock
        {
            Text = $"Next action: {item.NextAction}",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 0)
        });

        var dateText = item.FollowUpDate.HasValue
            ? item.FollowUpDate.Value.ToString("yyyy-MM-dd")
            : "No date";

        root.Children.Add(new TextBlock
        {
            Text = $"Follow-up date: {dateText} | Money-linked: {(item.IsMoneyLinked ? "Yes" : "No")}",
            Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 0)
        });

        if (!string.IsNullOrWhiteSpace(item.Notes))
        {
            root.Children.Add(new TextBlock
            {
                Text = item.Notes,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 6, 0, 0)
            });
        }

        var buttonRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 12, 0, 0)
        };

        var completeButton = CreateSmallActionButton("Mark Complete");
        completeButton.Tag = item.Id;
        completeButton.Click += CompleteFollowUpButton_Click;

        var deleteButton = CreateSmallActionButton("Delete");
        deleteButton.Tag = item.Id;
        deleteButton.Click += DeleteFollowUpButton_Click;

        buttonRow.Children.Add(completeButton);
        buttonRow.Children.Add(deleteButton);

        root.Children.Add(buttonRow);

        card.Child = root;
        return card;
    }

    private static TextBox CreateStandardTextBox(string text = "")
    {
        return new TextBox
        {
            Text = text,
            Margin = new Thickness(0, 6, 14, 14),
            Padding = new Thickness(10, 8, 10, 8),
            Background = new SolidColorBrush(Color.FromRgb(15, 23, 42)),
            Foreground = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(51, 65, 85)),
            BorderThickness = new Thickness(1),
            FontSize = 14
        };
    }

    private static ComboBox CreateEnumComboBox<TEnum>(TEnum selectedValue)
        where TEnum : struct, Enum
    {
        var comboBox = new ComboBox
        {
            ItemsSource = Enum.GetValues<TEnum>(),
            SelectedItem = selectedValue,
            Margin = new Thickness(0, 6, 14, 14),
            Padding = new Thickness(8, 6, 8, 6),
            Background = new SolidColorBrush(Color.FromRgb(15, 23, 42)),
            Foreground = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(51, 65, 85)),
            BorderThickness = new Thickness(1),
            FontSize = 14
        };

        return comboBox;
    }

    private static Button CreateActionButton(string text, Color background, Color foreground)
    {
        return new Button
        {
            Content = text,
            Padding = new Thickness(16, 10, 16, 10),
            Margin = new Thickness(0, 0, 10, 0),
            HorizontalAlignment = HorizontalAlignment.Left,
            Background = new SolidColorBrush(background),
            Foreground = new SolidColorBrush(foreground),
            BorderBrush = new SolidColorBrush(background),
            FontWeight = FontWeights.SemiBold,
            Cursor = System.Windows.Input.Cursors.Hand
        };
    }

    private static Button CreateSmallActionButton(string text)
    {
        return new Button
        {
            Content = text,
            Padding = new Thickness(10, 6, 10, 6),
            Margin = new Thickness(0, 0, 8, 0),
            HorizontalAlignment = HorizontalAlignment.Left,
            Background = new SolidColorBrush(Color.FromRgb(30, 41, 59)),
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(51, 65, 85)),
            FontSize = 12,
            Cursor = System.Windows.Input.Cursors.Hand
        };
    }

    private void SaveMoneyPressureButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateMoneyPressureInputFromTextBoxes();
        MoneyPressureStorage.Save(_moneyPressureInput);
        ShowMoneyPressurePage();
    }

    private void ResetMoneyPressureButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmRiskyAction("Reset money-pressure inputs?", "This will clear your saved manual money-pressure inputs and reload defaults."))
        {
            return;
        }

        MoneyPressureStorage.Reset();
        _moneyPressureInput = new MoneyPressureManualInput();
        ShowMoneyPressurePage();
    }

    private static TextBox CreateMoneyInputTextBox(decimal value)
    {
        return new TextBox
        {
            Text = value.ToString("0.00"),
            Margin = new Thickness(0, 6, 14, 14),
            Padding = new Thickness(10, 8, 10, 8),
            Background = new SolidColorBrush(Color.FromRgb(15, 23, 42)),
            Foreground = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(51, 65, 85)),
            BorderThickness = new Thickness(1),
            FontSize = 14
        };
    }

    private static decimal ReadMoneyValue(TextBox? textBox, decimal fallback)
    {
        if (textBox is null)
        {
            return fallback;
        }

        var text = textBox.Text
            .Replace("$", string.Empty)
            .Replace(",", string.Empty)
            .Trim();

        if (decimal.TryParse(text, out var value))
        {
            return Math.Max(0m, value);
        }

        return fallback;
    }

    private static void AddInputField(Grid grid, string label, Control control, int row, int column)
    {
        var stack = new StackPanel();

        stack.Children.Add(new TextBlock
        {
            Text = label,
            Foreground = new SolidColorBrush(Color.FromRgb(203, 213, 225)),
            FontSize = 13,
            FontWeight = FontWeights.SemiBold
        });

        stack.Children.Add(control);

        Grid.SetRow(stack, row);
        Grid.SetColumn(stack, column);

        grid.Children.Add(stack);
    }

    private WrapPanel CreateMoneyPressureMetricsPanel(MoneyPressureSummary summary)
    {
        var metricsPanel = new WrapPanel();

        metricsPanel.Children.Add(CreateDashboardCard("Safe to spend", FormatMoney(summary.SafeToSpend), summary.PressureLabel));
        metricsPanel.Children.Add(CreateDashboardCard("Current balance", FormatMoney(summary.CurrentBalance), "Manual entry"));
        metricsPanel.Children.Add(CreateDashboardCard("Paid income", FormatMoney(summary.ConfirmedPaidIncome), "Counted as safe"));
        metricsPanel.Children.Add(CreateDashboardCard("Pending income", FormatMoney(summary.PendingIncome), "Not safe yet"));
        metricsPanel.Children.Add(CreateDashboardCard("Bills due", FormatMoney(summary.BillsDue), "This week"));
        metricsPanel.Children.Add(CreateDashboardCard("Deductions", FormatMoney(summary.DeductionsDue), "Active"));
        metricsPanel.Children.Add(CreateDashboardCard("Food/fuel buffer", FormatMoney(summary.FoodFuelBuffer), "Reserved"));
        metricsPanel.Children.Add(CreateDashboardCard("Emergency buffer", FormatMoney(summary.EmergencyBuffer), "Reserved"));

        return metricsPanel;
    }

    private void ShowPlaceholderPage(string title, string description, string detail)
    {
        SetHeader(title, $"{title} • planned LifeOS module");

        var root = new StackPanel();

        root.Children.Add(CreateHeroPanel(title, description));

        var detailsPanel = CreateInfoPanel("Current status", detail);
        detailsPanel.Margin = new Thickness(0, 22, 0, 0);
        root.Children.Add(detailsPanel);

        var scopePanel = CreateInfoPanel(
            "Shell v0.1 scope",
            "This screen is intentionally a placeholder. The shell proves navigation, module boundaries, and the desktop/mobile architecture before full feature build-out.");
        scopePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(scopePanel);

        MainContentControl.Content = root;
    }

    private void SetHeader(string title, string subtitle)
    {
        PageTitleTextBlock.Text = title;
        PageSubtitleTextBlock.Text = subtitle;
        CurrentSectionTextBlock.Text = $"Current section: {title}";
    }


    private static bool ConfirmRiskyAction(string title, string message)
    {
        var result = MessageBox.Show(
            message,
            title,
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        return result == MessageBoxResult.Yes;
    }

    private Border CreateHeroPanel(string title, string body)
    {
        var panel = CreatePanel();

        var stack = new StackPanel();

        stack.Children.Add(new TextBlock
        {
            Text = title,
            Foreground = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
            FontSize = 30,
            FontWeight = FontWeights.Bold,
            TextWrapping = TextWrapping.Wrap
        });

        stack.Children.Add(new TextBlock
        {
            Text = body,
            Foreground = new SolidColorBrush(Color.FromRgb(203, 213, 225)),
            FontSize = 15,
            LineHeight = 23,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 12, 0, 0),
            MaxWidth = 900
        });

        panel.Child = stack;
        return panel;
    }

    private Border CreateDashboardCard(string title, string body, string badge)
    {
        var card = new Border
        {
            Width = 280,
            MinHeight = 170,
            Background = new SolidColorBrush(Color.FromRgb(17, 24, 39)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(36, 50, 68)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(16),
            Padding = new Thickness(18),
            Margin = new Thickness(0, 0, 16, 16)
        };

        var stack = new StackPanel();

        stack.Children.Add(new TextBlock
        {
            Text = badge,
            Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248)),
            FontSize = 12,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 10)
        });

        stack.Children.Add(new TextBlock
        {
            Text = title,
            Foreground = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
            FontSize = 20,
            FontWeight = FontWeights.Bold,
            TextWrapping = TextWrapping.Wrap
        });

        stack.Children.Add(new TextBlock
        {
            Text = body,
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 13,
            LineHeight = 19,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 10, 0, 0)
        });

        card.Child = stack;
        return card;
    }

    private Border CreateInfoPanel(string title, string body)
    {
        var panel = CreatePanel();

        var stack = new StackPanel();

        stack.Children.Add(new TextBlock
        {
            Text = title,
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 18,
            FontWeight = FontWeights.Bold
        });

        stack.Children.Add(new TextBlock
        {
            Text = body,
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 14,
            LineHeight = 22,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 8, 0, 0)
        });

        panel.Child = stack;
        return panel;
    }

    private Border CreatePanel()
    {
        return new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(17, 24, 39)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(36, 50, 68)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(18),
            Padding = new Thickness(24)
        };
    }


    private static string FormatPaidWorkMoneyProofHealth(PaidWorkMoneyProofHealth health)
    {
        return health switch
        {
            PaidWorkMoneyProofHealth.Calm => "Calm",
            PaidWorkMoneyProofHealth.Watch => "Watch",
            PaidWorkMoneyProofHealth.NeedsReview => "Needs Review",
            PaidWorkMoneyProofHealth.HighPressure => "High Pressure",
            _ => health.ToString()
        };
    }

    private static string FormatMoney(decimal value)
    {
        return value.ToString("C");
    }
}
