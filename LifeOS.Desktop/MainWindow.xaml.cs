using LifeOS.Core.Money;
using LifeOS.Shared.Money;
using LifeOS.Shared.Shell;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LifeOS.Core.FollowUps;
using LifeOS.Shared.FollowUps;
using LifeOS.Shared.CommandCentre;

using LifeOS.Core.Agenda;
using LifeOS.Core.PayLater;
using LifeOS.Core.WeeklyCloseOut;
using LifeOS.Shared.Agenda;
using LifeOS.Shared.PayLater;
using LifeOS.Shared.WeeklyCloseOut;
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

    public MainWindow()
    {
        InitializeComponent();
        ShowCommandCentre();
    }

    private void CommandCentreNavButton_Click(object sender, RoutedEventArgs e) => ShowCommandCentre();

    private void MoneyPressureNavButton_Click(object sender, RoutedEventArgs e) => ShowMoneyPressurePage();

    private void AgendaNavButton_Click(object sender, RoutedEventArgs e) => ShowAgendaPage();

    private void PayLaterNavButton_Click(object sender, RoutedEventArgs e) => ShowPayLaterPage();

    private void WeeklyCloseOutNavButton_Click(object sender, RoutedEventArgs e) => ShowWeeklyCloseOutPage();

    private void FollowUpsNavButton_Click(object sender, RoutedEventArgs e) => ShowFollowUpsPage();

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
            "First editable Money Pressure foundation. Enter manual weekly values, then recalculate safe-to-spend. This still uses shared Core calculation logic."));

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

        SetHeader("Agenda", "Agenda • v0.2 weekly pressure foundation");

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
            Text = "Keep the week visible without turning LifeOS into a full calendar app yet.",
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
            root.Children.Add(CreateEmptyTextBlock("No agenda items yet."));
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

        SetHeader("Pay Later", "Pay Later • v0.2 deferred pressure foundation");

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
            Text = "Deferred payments still create pressure. Keep them visible.",
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
            root.Children.Add(CreateEmptyTextBlock("No pay-later items yet."));
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

        SetHeader("Weekly Close-Out", "Weekly Close-Out • v0.2 weekly review foundation");

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
            root.Children.Add(CreateEmptyTextBlock("No weekly close-out entries yet."));
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

    private void ShowCommandCentre()
    {
        var summary = CommandCentreSummaryService.Create();

        SetHeader("Command Centre", $"Weekly pressure command centre • {summary.OverallPressureLabel}");

        var root = new StackPanel();

        root.Children.Add(CreateHeroPanel(
            "LifeOS Command Centre",
            "This is now reading real local LifeOS data from Money Pressure and Follow-Ups. Desktop proves the full model; mobile will pressure-test the optimized daily-use version."));

        var metricsPanel = new WrapPanel
        {
            Margin = new Thickness(0, 22, 0, 0)
        };

        metricsPanel.Children.Add(CreateDashboardCard("Overall pressure", summary.OverallPressureLabel, "This week"));
        metricsPanel.Children.Add(CreateDashboardCard("Safe to spend", FormatMoney(summary.MoneyPressure.SafeToSpend), summary.MoneyPressure.PressureLabel));
        metricsPanel.Children.Add(CreateDashboardCard("Pending income", FormatMoney(summary.MoneyPressure.PendingIncome), "Not safe yet"));
        metricsPanel.Children.Add(CreateDashboardCard("Open follow-ups", summary.FollowUps.TotalOpen.ToString(), "Waiting-on"));
        metricsPanel.Children.Add(CreateDashboardCard("Needs action", summary.FollowUps.NeedsActionCount.ToString(), "Action"));
        metricsPanel.Children.Add(CreateDashboardCard("Money-linked", summary.FollowUps.MoneyLinkedCount.ToString(), "Work/money"));

        root.Children.Add(metricsPanel);

        var actionPanel = CreateInfoPanel(
            "Next safest action",
            summary.NextSafestAction);

        actionPanel.Margin = new Thickness(0, 8, 0, 0);
        root.Children.Add(actionPanel);

        var reasonsText = string.Join(Environment.NewLine, summary.Reasons.Select(reason => $"• {reason}"));

        var reasonsPanel = CreateInfoPanel(
            "Why this week has pressure",
            reasonsText);

        reasonsPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(reasonsPanel);

        var timerPanel = CreateInfoPanel(
            "TimerAgent status",
            "TimerAgent remains a desktop-only utility for focused work, billable sessions, earned income, tax set-aside, and CSV logs. A later phase can read TimerAgent work-session data into this Command Centre.");

        timerPanel.Margin = new Thickness(0, 16, 0, 0);
        root.Children.Add(timerPanel);

        var guardrailPanel = CreateInfoPanel(
            "Phase 12 scope",
            "Command Centre now combines saved Money Pressure and Follow-Ups data. No agenda module, TimerAgent CSV import, database, bank sync, mobile app, or AI layer yet.");

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
                Text = "No follow-ups yet.",
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

