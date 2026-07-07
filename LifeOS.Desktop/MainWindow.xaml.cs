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

    private void AgendaNavButton_Click(object sender, RoutedEventArgs e) => ShowAgendaPage();

    private void PayLaterNavButton_Click(object sender, RoutedEventArgs e) => ShowPayLaterPage();

    private void WeeklyCloseOutNavButton_Click(object sender, RoutedEventArgs e) => ShowWeeklyCloseOutPage();

    private void WorkSessionsNavButton_Click(object sender, RoutedEventArgs e) => ShowWorkSessionsPage();

    private void PaidWorkCentreNavButton_Click(object sender, RoutedEventArgs e) => ShowPaidWorkCentrePage();

    private void ProofTrackerNavButton_Click(object sender, RoutedEventArgs e) => ShowProofTrackerPage();

    private void FollowUpsNavButton_Click(object sender, RoutedEventArgs e) => ShowFollowUpsPage();

    private void WorkPipelineNavButton_Click(object sender, RoutedEventArgs e) => ShowWorkPipelinePage();

    private void ProjectsNavButton_Click(object sender, RoutedEventArgs e) => ShowModulePage(LifeOSModuleKind.Projects);

    private void TimerAgentNavButton_Click(object sender, RoutedEventArgs e) => ShowModulePage(LifeOSModuleKind.TimerAgent);

    private void SettingsNavButton_Click(object sender, RoutedEventArgs e) => ShowModulePage(LifeOSModuleKind.Settings);

    private void RecalculateMoneyPressureButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateMoneyPressureInputFromTextBoxes();
        ShowMoneyPressurePage();
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

        SetHeader("Agenda", "Agenda • v0.4 weekly pressure foundation");

        var root = new StackPanel();

        root.Children.Add(CreateHeroPanel(
            "Agenda",
            "Track what matters this week: tasks, appointments, due dates, fixed commitments, and pressure."));

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
        var summary = WeeklyCloseOutCalculator.Calculate(_weeklyCloseOutEntries, today);

        SetHeader("Weekly Close-Out", "Weekly Close-Out • v0.4 weekly review foundation");

        var root = new StackPanel();

        root.Children.Add(CreateHeroPanel(
            "Weekly Close-Out",
            "Capture what got done, what moved, what is still waiting, and what pressure rolls into next week."));

        var metricsPanel = new WrapPanel
        {
            Margin = new Thickness(0, 22, 0, 0)
        };

        metricsPanel.Children.Add(CreateDashboardCard("Total entries", summary.TotalEntries.ToString(), "Close-out"));
        metricsPanel.Children.Add(CreateDashboardCard("This week", summary.EntriesThisWeek.ToString(), "Current week"));
        metricsPanel.Children.Add(CreateDashboardCard("Current close-out", summary.HasCurrentWeekCloseOut ? "Yes" : "No", "Weekly loop"));
        metricsPanel.Children.Add(CreateDashboardCard("Waiting-on count", summary.WaitingOnCount.ToString(), "Pressure"));

        root.Children.Add(metricsPanel);

        var reasonsPanel = CreateInfoPanel("Weekly close-out pressure", FormatReasons(summary.Reasons));
        reasonsPanel.Margin = new Thickness(0, 8, 0, 0);
        root.Children.Add(reasonsPanel);

        var inputPanel = CreateWeeklyCloseOutInputPanel();
        inputPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(inputPanel);

        var listPanel = CreateWeeklyCloseOutListPanel();
        listPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(listPanel);

        MainContentControl.Content = root;
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

        SetHeader("Work Pipeline", $"Work Pipeline • v0.9 release candidate • {summary.OpenItems} open");

        var root = new StackPanel();

        root.Children.Add(CreateHeroPanel(
            "Work Pipeline",
            "Track paid work, warm leads, proof projects, blocked work, follow-ups, timesheets, invoices, expected payments, and parked ideas without turning LifeOS into a bloated CRM."));

        var metricsPanel = new WrapPanel
        {
            Margin = new Thickness(0, 22, 0, 0)
        };

        metricsPanel.Children.Add(CreateDashboardCard("Open", summary.OpenItems.ToString(), "Pipeline"));
        metricsPanel.Children.Add(CreateDashboardCard("Active", summary.ActiveItems.ToString(), "Moving"));
        metricsPanel.Children.Add(CreateDashboardCard("Waiting", summary.WaitingItems.ToString(), "Waiting-on"));
        metricsPanel.Children.Add(CreateDashboardCard("Blocked", summary.BlockedItems.ToString(), "Blocked"));
        metricsPanel.Children.Add(CreateDashboardCard("Follow-ups", (summary.FollowUpsOverdue + summary.FollowUpsDueToday + summary.FollowUpsDueSoon).ToString(), "Next 7 days"));
        metricsPanel.Children.Add(CreateDashboardCard("Expected value", FormatMoney(summary.ExpectedValueTotal), "Not safe money"));
        metricsPanel.Children.Add(CreateDashboardCard("Timesheets", summary.TimesheetsNeeded.ToString(), "Needed"));
        metricsPanel.Children.Add(CreateDashboardCard("Invoices", summary.InvoicesNeeded.ToString(), "Needed"));
        metricsPanel.Children.Add(CreateDashboardCard("Payments", summary.PaymentsExpected.ToString(), "Expected"));

        root.Children.Add(metricsPanel);

        var reasonsPanel = CreateInfoPanel("Pipeline pressure", FormatReasons(summary.Reasons));
        reasonsPanel.Margin = new Thickness(0, 8, 0, 0);
        root.Children.Add(reasonsPanel);

        var stageBreakdown = summary.StageCounts.Count == 0
            ? "No open pipeline stages."
            : string.Join(Environment.NewLine, summary.StageCounts.Select(stage => $"• {FormatWorkPipelineStage(stage.Stage)}: {stage.Count}"));

        var stagePanel = CreateInfoPanel("Stage breakdown", stageBreakdown);
        stagePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(stagePanel);

        var focusPanel = CreateWorkPipelineFocusPanel(summary);
        focusPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(focusPanel);

        var bridgeCount = WorkPipelineFollowUpBridge.CreateFollowUps(_workPipelineItems).Count;
        var bridgePanel = CreateInfoPanel("Follow-up bridge", $"{bridgeCount} pipeline item(s) can be mirrored into Follow-Ups when that workflow is enabled. This stage only prepares the bridge; it does not auto-create duplicate follow-ups.");
        bridgePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(bridgePanel);

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
            "v0.9 scope",
            $"Work Pipeline now includes follow-up states, waiting-on views, opportunity heat, stage counts, storage backup safety, and Command Centre pressure signals. Saved file: {WorkPipelineStorage.FilePath}. No auto-messaging, CRM, or accounting automation.");

        guardrailPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(guardrailPanel);

        MainContentControl.Content = root;
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

        _workSessionClientTextBox = CreateStandardTextBox("AIE / JV / LifeOS");
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
        var invoiceReady = _workSessions
            .Where(session => session.IsBillable && session.Status is WorkSessionStatus.Completed or WorkSessionStatus.Invoiced)
            .OrderBy(session => session.ClientOrProject)
            .ThenBy(session => session.Date)
            .ToList();

        SetHeader("Paid Work Centre", "Paid Work Centre • v0.5 invoice-ready admin");

        var root = new StackPanel();

        root.Children.Add(CreateHeroPanel(
            "Paid Work Centre",
            "Turn real work sessions into invoice-ready summaries. This is the bridge from doing the work, to logging time, to knowing what is owed, what is paid, and what should feed the Money Timeline."));

        var metricsPanel = new WrapPanel { Margin = new Thickness(0, 22, 0, 0) };
        metricsPanel.Children.Add(CreateDashboardCard("Invoice-ready sessions", invoiceReady.Count.ToString(), "Ready"));
        metricsPanel.Children.Add(CreateDashboardCard("Invoice-ready value", FormatMoney(invoiceReady.Sum(session => session.BillableValue)), "Owed"));
        metricsPanel.Children.Add(CreateDashboardCard("Unpaid billable", FormatMoney(summary.UnpaidBillableValue), "Pressure"));
        metricsPanel.Children.Add(CreateDashboardCard("Paid value", FormatMoney(summary.PaidValue), "Landed"));
        metricsPanel.Children.Add(CreateDashboardCard("Billable hours", summary.BillableHours.ToString("0.##"), "Tracked"));
        metricsPanel.Children.Add(CreateDashboardCard("Clients/projects", summary.ActiveClientOrProjectCount.ToString(), "Spread"));
        root.Children.Add(metricsPanel);

        var spinePanel = CreateInfoPanel(
            "v0.5 spine",
            "Do work → log work session → review invoice-ready items → generate work summary → send invoice/email → mark paid → feed expected income into Money Timeline.");
        spinePanel.Margin = new Thickness(0, 8, 0, 0);
        root.Children.Add(spinePanel);

        var summaryText = BuildInvoiceReadySummary(invoiceReady);
        var summaryPanel = CreatePanel();
        var summaryStack = new StackPanel();
        summaryStack.Children.Add(new TextBlock
        {
            Text = "Copy-ready work summary",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });
        summaryStack.Children.Add(new TextBlock
        {
            Text = "Use this as the base for a client work summary or invoice note. PDF generation comes later; v0.5 keeps it copy-ready and safe.",
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
            MinHeight = 220,
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
            "v0.5 boundary",
            "This is an invoice-ready/admin layer, not final accounting software. It does not send invoices, collect payments, calculate tax, or replace client approval. It helps prepare clear work summaries and track what is owed/paid.");
        scopePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(scopePanel);

        MainContentControl.Content = root;
    }



    private void ShowMoneyTimelinePage()
    {
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

        SetHeader("Money Timeline", "Money Timeline • v0.5 paper-bills workflow");

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
        root.Children.Add(metricsPanel);

        var timelinePanel = CreateInfoPanel(
            "Timeline reading",
            $"By {targetDate:yyyy-MM-dd}, LifeOS currently estimates {FormatMoney(expectedIncome)} coming in and {FormatMoney(outgoing)} going out/buffered. Projected balance is {FormatMoney(projectedBalance)}. Lowest visible point is {FormatMoney(lowestPoint)}. Pressure: {pressureLabel}.");
        timelinePanel.Margin = new Thickness(0, 8, 0, 0);
        root.Children.Add(timelinePanel);

        var sourcePanel = CreateInfoPanel(
            "What this is using in v0.5",
            "Incoming includes manual paid/pending income plus completed/invoiced billable work sessions. Outgoing includes manual bills, deductions, food/fuel buffer, and emergency buffer. Later versions can add dated income/outgoing rows, recurring bills, reminders, and direct links from invoices.");
        sourcePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(sourcePanel);

        var motherPanel = CreateInfoPanel(
            "Why this exists",
            "This module is the digital version of the paper-bills idea: do not just track categories; line up dates, incoming money, bills, and the lowest point so you know what is actually safe before spending.");
        motherPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(motherPanel);

        var stagePanel = CreateInfoPanel(
            "v0.5 boundary",
            "This is a first pressure/timeline view, not bank sync or formal budgeting software. It is intentionally simple: local data, clear assumptions, conservative safe-to-spend, and no automatic financial decisions.");
        stagePanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(stagePanel);

        MainContentControl.Content = root;
    }


    private void ShowCommandCentre()
    {
        var summary = CommandCentreSummaryService.Create();

        SetHeader("Command Centre", $"Unified Command Centre • v1.3 • {summary.OverallPressureLabel}");

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

        metricsPanel.Children.Add(CreateDashboardCard("Pipeline open", summary.WorkPipeline.OpenItems.ToString(), "Work Pipeline"));
        metricsPanel.Children.Add(CreateDashboardCard("Pipeline blocked", summary.WorkPipeline.BlockedItems.ToString(), "Blocked"));
        metricsPanel.Children.Add(CreateDashboardCard("Pipeline follow-ups", (summary.WorkPipeline.FollowUpsOverdue + summary.WorkPipeline.FollowUpsDueToday).ToString(), "Now"));
        metricsPanel.Children.Add(CreateDashboardCard("Expected pipeline", FormatMoney(summary.WorkPipeline.ExpectedValueTotal), "Not safe money"));

        metricsPanel.Children.Add(CreateDashboardCard("Billable value", FormatMoney(summary.WorkSessions.BillableValue), "Work"));
        metricsPanel.Children.Add(CreateDashboardCard("Unpaid work", FormatMoney(summary.WorkSessions.UnpaidBillableValue), "Income"));
        metricsPanel.Children.Add(CreateDashboardCard("Proof items", summary.ProofTracker.TotalProofItems.ToString(), "Proof"));
        metricsPanel.Children.Add(CreateDashboardCard("Proof ready", summary.ProofTracker.ReadyCount.ToString(), "Shareable"));

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

        var workPanel = CreateInfoPanel(
            "v1.3 evidence rule",
            "Command Centre now has a metadata-only Evidence Vault foundation. Evidence can be tracked and reviewed without building receipt OCR or provider sync yet.");

        workPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(workPanel);

        var guardrailPanel = CreateInfoPanel(
            "v1.3 scope",
            "Evidence Vault metadata only. No receipt OCR, email sync, mobile scanner, cloud storage, provider auth, or automatic evidence classification.");

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

        _followUpPersonTextBox = CreateStandardTextBox("Example: Vanessa / OSHE");
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

    private static string FormatMoney(decimal value)
    {
        return value.ToString("C");
    }
}
