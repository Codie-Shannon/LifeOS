using LifeOS.Core.Money;
using LifeOS.Shared.Money;
using LifeOS.Shared.Shell;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LifeOS.Core.FollowUps;
using LifeOS.Shared.FollowUps;

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

    public MainWindow()
    {
        InitializeComponent();
        ShowCommandCentre();
    }

    private void CommandCentreNavButton_Click(object sender, RoutedEventArgs e) => ShowCommandCentre();

    private void MoneyPressureNavButton_Click(object sender, RoutedEventArgs e) => ShowMoneyPressurePage();

    private void AgendaNavButton_Click(object sender, RoutedEventArgs e) => ShowModulePage(LifeOSModuleKind.Agenda);

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
        SetHeader("Command Centre", "Weekly pressure command centre • Desktop shell v0.1");

        var root = new StackPanel();

        root.Children.Add(CreateHeroPanel(
            "LifeOS Command Centre",
            "Desktop is the daily-use power version and proving ground. Mobile will use the same core LifeOS model and pressure-test the UX. TimerAgent remains a desktop-only utility that feeds LifeOS work/time/income data."));

        var wrap = new WrapPanel
        {
            Margin = new Thickness(0, 22, 0, 0)
        };

        wrap.Children.Add(CreateDashboardCard(
            "Money Pressure",
            "Safe-to-spend, income, bills, deductions, and weekly danger points.",
            "Shared core module"));

        wrap.Children.Add(CreateDashboardCard(
            "Agenda",
            "Appointments, tasks, fixed/flexible events, and time pressure.",
            "Shared core module"));

        wrap.Children.Add(CreateDashboardCard(
            "Follow-Ups",
            "People, client replies, waiting-on items, and money-linked follow-ups.",
            "Shared core module"));

        wrap.Children.Add(CreateDashboardCard(
            "Projects",
            "Portfolio builds, proof projects, shipped versions, and active focus.",
            "Shared core module"));

        wrap.Children.Add(CreateDashboardCard(
            "TimerAgent",
            "Desktop-only utility for billable work sessions, earned income, tax set-aside, and CSV work logs.",
            "Desktop-only utility"));

        wrap.Children.Add(CreateDashboardCard(
            "Settings",
            "Theme, storage, export, backup, and shared app preferences.",
            "Platform settings"));

        root.Children.Add(wrap);

        var guardrail = CreateInfoPanel(
            "Permanent platform rule",
            "Core features should reach both desktop and mobile. Experimental features start on desktop. Platform-specific features stay platform-specific.");
        guardrail.Margin = new Thickness(0, 8, 0, 0);
        root.Children.Add(guardrail);

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

