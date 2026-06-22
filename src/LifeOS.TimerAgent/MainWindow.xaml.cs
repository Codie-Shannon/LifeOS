using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using LifeOS.Core.Models;
using LifeOS.Modules.Timer.Models;
using LifeOS.Modules.Timer.Services;
using LifeOS.Modules.Timer.Storage;
using LifeOS.TimerAgent.Services;
using LifeOS.TimerAgent.Storage;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using WpfMessageBox = System.Windows.MessageBox;

namespace LifeOS.TimerAgent;

public partial class MainWindow : Window
{
    private enum TimerAgentPage
    {
        List,
        Timer,
        Contacts,
        TaskDetails,
        ContactDetails
    }

    private readonly TimerManager _timerManager = new();
    private readonly List<ContactProfile> _contacts = new();
    private readonly TimerCsvLogWriter _logWriter;
    private readonly TimerAgentStateStore _stateStore;
    private readonly DispatcherTimer _uiTimer;

    private static readonly TimeSpan WorkDayCutoff = TimeSpan.FromHours(4);
    private static readonly TimeSpan NewWorkDayGap = TimeSpan.FromHours(2);

    private GlobalHotKeyService? _hotKeyService;
    private TrayIconService? _trayIconService;
    private bool _allowClose;
    private TimedTask? _editingTask;
    private ContactProfile? _editingContact;
    private ContactProfile? _selectedContact;

    private const double ManagerWindowWidth = 560;
    private const double ManagerWindowHeight = 720;
    private const double ManagerWindowMinWidth = 500;
    private const double ManagerWindowMinHeight = 640;
    private const double CompactWindowWidth = 410;
    private const double CompactWindowHeight = 170;
    private const double CompactWindowMinWidth = 360;
    private const double CompactWindowMinHeight = 130;

    public MainWindow()
    {
        InitializeComponent();

        Loaded += MainWindow_Loaded;
        PreviewKeyDown += MainWindow_PreviewKeyDown;

        var appDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LifeOS",
            "TimerAgent");

        var logFilePath = Path.Combine(appDataFolder, "timer-log.csv");
        var stateFilePath = Path.Combine(appDataFolder, "timeragent-state.json");

        _logWriter = new TimerCsvLogWriter(logFilePath);
        _stateStore = new TimerAgentStateStore(stateFilePath);

        _uiTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _uiTimer.Tick += (_, _) => RefreshUi();

        LoadStateOrSeedDemoData();
        SetupDropdowns();
        RefreshLists();
        ShowPage(TimerAgentPage.List);

        _uiTimer.Start();
        RefreshUi();
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        SetupTrayIcon();
        SetupGlobalHotKey();
    }

    private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            ExitApplication();
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_allowClose)
        {
            e.Cancel = true;
            Hide();
            return;
        }

        base.OnClosing(e);
    }

    protected override void OnClosed(EventArgs e)
    {
        SaveState();
        _hotKeyService?.Dispose();
        _trayIconService?.Dispose();
        base.OnClosed(e);
    }

    private void LoadStateOrSeedDemoData()
    {
        var state = _stateStore.Load();

        if (state.Contacts.Count > 0 || state.TimedTasks.Count > 0)
        {
            _contacts.Clear();
            _contacts.AddRange(state.Contacts.Where(contact => contact.IsActive));

            _selectedContact = state.SelectedContactId is null
                ? _contacts.FirstOrDefault()
                : _contacts.FirstOrDefault(contact => contact.Id == state.SelectedContactId.Value) ?? _contacts.FirstOrDefault();

            _timerManager.ReplaceTasks(state.TimedTasks, state.SelectedTaskId);
            return;
        }

        SeedDemoData();
        SaveState();
    }

    private void SeedDemoData()
    {
        if (_contacts.Count > 0)
        {
            return;
        }

        var aie = new ContactProfile
        {
            DisplayName = "AIE",
            Type = ContactType.Client,
            DefaultHourlyRate = 35m,
            DefaultTaxSetAsidePercent = 20m,
            DefaultWorkType = "Workflow/Admin"
        };

        var totalDoor = new ContactProfile
        {
            DisplayName = "Total Door",
            Type = ContactType.Client,
            DefaultHourlyRate = 30m,
            DefaultTaxSetAsidePercent = 20m,
            DefaultWorkType = "OCR/Cin7"
        };

        var lifeOs = new ContactProfile
        {
            DisplayName = "Life OS",
            Type = ContactType.Organisation,
            DefaultHourlyRate = 0m,
            DefaultTaxSetAsidePercent = 0m,
            DefaultWorkType = "Product build"
        };

        _contacts.Add(aie);
        _contacts.Add(totalDoor);
        _contacts.Add(lifeOs);
        _selectedContact = aie;

        _timerManager.CreateTask(
            name: "INFO mailbox outline",
            contactId: aie.Id,
            contactName: aie.DisplayName,
            projectName: "INFO mailbox",
            workType: aie.DefaultWorkType,
            taskType: TimedTaskType.Work,
            mode: TimedTaskMode.Exclusive,
            isBillable: true,
            hourlyRate: aie.DefaultHourlyRate,
            taxSetAsidePercent: aie.DefaultTaxSetAsidePercent,
            notes: string.Empty);

        _timerManager.CreateTask(
            name: "Supplier bills / Cin7 proof",
            contactId: totalDoor.Id,
            contactName: totalDoor.DisplayName,
            projectName: "Supplier bills",
            workType: totalDoor.DefaultWorkType,
            taskType: TimedTaskType.Work,
            mode: TimedTaskMode.Exclusive,
            isBillable: true,
            hourlyRate: totalDoor.DefaultHourlyRate,
            taxSetAsidePercent: totalDoor.DefaultTaxSetAsidePercent,
            notes: string.Empty);

        _timerManager.CreateTask(
            name: "TimerAgent UI refactor",
            contactId: lifeOs.Id,
            contactName: lifeOs.DisplayName,
            projectName: "TimerAgent",
            workType: lifeOs.DefaultWorkType,
            taskType: TimedTaskType.Work,
            mode: TimedTaskMode.Exclusive,
            isBillable: false,
            hourlyRate: 0m,
            taxSetAsidePercent: 0m,
            notes: string.Empty);
    }

    private void SaveState()
    {
        var state = new TimerAgentAppState
        {
            Contacts = _contacts.ToList(),
            TimedTasks = _timerManager.GetAllTasksIncludingArchived().ToList(),
            SelectedContactId = _selectedContact?.Id,
            SelectedTaskId = _timerManager.SelectedTask?.Id
        };

        _stateStore.Save(state);
    }

    private void SetupDropdowns()
    {
        TaskTypeComboBox.ItemsSource = Enum.GetValues(typeof(TimedTaskType));
        TaskModeComboBox.ItemsSource = Enum.GetValues(typeof(TimedTaskMode));
        ContactTypeComboBox.ItemsSource = Enum.GetValues(typeof(ContactType));
    }

    private void SetupTrayIcon()
    {
        if (_trayIconService is not null)
        {
            return;
        }

        _trayIconService = new TrayIconService();
        _trayIconService.ShowRequested += (_, _) => Dispatcher.Invoke(ShowTimerWindow);
        _trayIconService.HideRequested += (_, _) => Dispatcher.Invoke(Hide);
        _trayIconService.ExitRequested += (_, _) => Dispatcher.Invoke(ExitApplication);
    }

    private void SetupGlobalHotKey()
    {
        if (_hotKeyService is not null)
        {
            return;
        }

        try
        {
            _hotKeyService = new GlobalHotKeyService();
            _hotKeyService.HotKeyPressed += (_, _) => Dispatcher.Invoke(ToggleWindowVisibility);
            _hotKeyService.Register(
                this,
                hotKeyId: 9001,
                key: Key.Space,
                modifiers: HotKeyModifiers.Control | HotKeyModifiers.Alt | HotKeyModifiers.NoRepeat);
        }
        catch (Exception ex)
        {
            WpfMessageBox.Show(
                $"TimerAgent is running, but the global shortcut could not be registered.\n\n{ex.Message}",
                "Shortcut unavailable",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private void ShowTimerWindow()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
        Topmost = false;
        Topmost = true;
    }

    private void ToggleWindowVisibility()
    {
        if (IsVisible && WindowState != WindowState.Minimized)
        {
            Hide();
            return;
        }

        ShowTimerWindow();
    }

    private void ExitApplication()
    {
        _allowClose = true;
        Close();
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void HideWindowButton_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    private void ListNavButton_Click(object sender, RoutedEventArgs e)
    {
        ShowPage(TimerAgentPage.List);
    }

    private void TimerNavButton_Click(object sender, RoutedEventArgs e)
    {
        ShowPage(TimerAgentPage.Timer);
    }

    private void FullTimerButton_Click(object sender, RoutedEventArgs e)
    {
        ShowPage(TimerAgentPage.List);
    }

    private void ContactsNavButton_Click(object sender, RoutedEventArgs e)
    {
        ShowPage(TimerAgentPage.Contacts);
    }

    private void ShowPage(TimerAgentPage page)
    {
        var isCompactTimer = page == TimerAgentPage.Timer;

        ListPage.Visibility = page == TimerAgentPage.List ? Visibility.Visible : Visibility.Collapsed;
        TimerPage.Visibility = isCompactTimer ? Visibility.Visible : Visibility.Collapsed;
        ContactsPage.Visibility = page == TimerAgentPage.Contacts ? Visibility.Visible : Visibility.Collapsed;
        TaskDetailsPage.Visibility = page == TimerAgentPage.TaskDetails ? Visibility.Visible : Visibility.Collapsed;
        ContactDetailsPage.Visibility = page == TimerAgentPage.ContactDetails ? Visibility.Visible : Visibility.Collapsed;

        ApplyWindowMode(isCompactTimer);
        RefreshUi();
    }

    private void ApplyWindowMode(bool isCompactTimer)
    {
        if (isCompactTimer)
        {
            ManagerHeader.Visibility = Visibility.Collapsed;
            ManagerNav.Visibility = Visibility.Collapsed;
            StatusStripBorder.Visibility = Visibility.Collapsed;

            ContentRoot.Margin = new Thickness(8);
            HeaderSpacerRow.Height = new GridLength(0);
            NavSpacerRow.Height = new GridLength(0);
            StatusSpacerRow.Height = new GridLength(0);

            MinWidth = CompactWindowMinWidth;
            MinHeight = CompactWindowMinHeight;
            Width = CompactWindowWidth;
            Height = CompactWindowHeight;
            ResizeMode = ResizeMode.NoResize;
            Topmost = true;
            return;
        }

        ManagerHeader.Visibility = Visibility.Visible;
        ManagerNav.Visibility = Visibility.Visible;
        StatusStripBorder.Visibility = Visibility.Visible;

        ContentRoot.Margin = new Thickness(16);
        HeaderSpacerRow.Height = new GridLength(10);
        NavSpacerRow.Height = new GridLength(10);
        StatusSpacerRow.Height = new GridLength(10);

        MinWidth = ManagerWindowMinWidth;
        MinHeight = ManagerWindowMinHeight;
        Width = ManagerWindowWidth;
        Height = ManagerWindowHeight;
        ResizeMode = ResizeMode.CanResize;
        Topmost = true;
    }

    private void RefreshLists()
    {
        var selectedTaskId = (TimedTasksListBox.SelectedItem as TimedTask)?.Id ?? _timerManager.SelectedTask?.Id;
        var selectedContactId = (ContactsListBox.SelectedItem as ContactProfile)?.Id ?? _selectedContact?.Id;

        TimedTasksListBox.ItemsSource = null;
        TimedTasksListBox.ItemsSource = _timerManager.Tasks;

        ContactsListBox.ItemsSource = null;
        ContactsListBox.ItemsSource = _contacts.Where(contact => contact.IsActive).ToList();

        TaskContactComboBox.ItemsSource = null;
        TaskContactComboBox.ItemsSource = _contacts.Where(contact => contact.IsActive).ToList();

        if (selectedTaskId is not null)
        {
            TimedTasksListBox.SelectedItem = _timerManager.Tasks.FirstOrDefault(task => task.Id == selectedTaskId.Value);
        }

        if (selectedContactId is not null)
        {
            ContactsListBox.SelectedItem = _contacts.FirstOrDefault(contact => contact.Id == selectedContactId.Value);
        }
    }

    private void RefreshUi()
    {
        var selectedTask = _timerManager.SelectedTask;

        if (selectedTask is null)
        {
            ActiveTaskNameText.Text = "No timed task selected";
            ActiveTaskMetaText.Text = "Select a task from the list";
            TimerDisplay.Text = "00:00:00";
            StateDisplay.Text = "Stopped";
            CompactMoneyDisplay.Text = "$0.00 earned · $0.00 tax · $0.00 safe";

            SmartTimerActionButton.Content = "Start";
            SmartTimerActionButton.IsEnabled = false;
            StopButton.IsEnabled = false;

            ListStartPauseButton.Content = "Start";
            ListStartPauseButton.IsEnabled = false;
            ListStopButton.IsEnabled = false;
        }
        else
        {
            var duration = selectedTask.GetCurrentDuration();
            var earned = selectedTask.GetEarnedAmount();
            var tax = selectedTask.GetTaxSetAside();
            var safe = selectedTask.GetSafeAfterTax();

            ActiveTaskNameText.Text = selectedTask.DisplayName;
            ActiveTaskMetaText.Text = $"{selectedTask.TaskType} · {selectedTask.Mode} · {selectedTask.WorkType}";
            TimerDisplay.Text = FormatDuration(duration);
            StateDisplay.Text = selectedTask.State.ToString();
            CompactMoneyDisplay.Text = $"{earned:C} earned · {tax:C} tax · {safe:C} safe";

            SmartTimerActionButton.Content = selectedTask.State switch
            {
                TimedTaskState.Running => "Pause",
                TimedTaskState.Paused => "Resume",
                _ => "Start"
            };
            SmartTimerActionButton.IsEnabled = selectedTask.State is TimedTaskState.Stopped or TimedTaskState.Running or TimedTaskState.Paused;
            StopButton.IsEnabled = selectedTask.State is TimedTaskState.Running or TimedTaskState.Paused;

            ListStartPauseButton.IsEnabled = selectedTask.State is TimedTaskState.Stopped or TimedTaskState.Running or TimedTaskState.Paused;
            ListStopButton.IsEnabled = selectedTask.State is TimedTaskState.Running or TimedTaskState.Paused;
            ListStartPauseButton.Content = selectedTask.State switch
            {
                TimedTaskState.Running => "Pause",
                TimedTaskState.Paused => "Resume",
                _ => "Start"
            };
        }

        RefreshTotals();
        RefreshStatusStrip();
    }

    private void RefreshTotals()
    {
        var selectedTask = _timerManager.SelectedTask;

        if (selectedTask is null)
        {
            TodayTotalsText.Text = "0h · $0.00";
            WeekTotalsText.Text = "0h · $0.00";
            AllTotalsText.Text = "0h · $0.00";
            return;
        }

        var allLogEntries = _logWriter.ReadAll().ToList();
        var selectedTaskEntries = allLogEntries
            .Where(entry => entry.TimedTaskId == selectedTask.Id)
            .ToList();

        var currentWorkDate = GetCurrentWorkDate(allLogEntries);
        var weekStartDate = DateOnly.FromDateTime(GetStartOfWeek(currentWorkDate.ToDateTime(TimeOnly.MinValue)));

        var currentWorkDayEntries = selectedTaskEntries
            .Where(entry => GetEntryWorkDate(entry) == currentWorkDate)
            .ToList();

        var weekEntries = selectedTaskEntries
            .Where(entry => GetEntryWorkDate(entry) >= weekStartDate)
            .ToList();

        var currentWorkDayMinutes = currentWorkDayEntries.Sum(entry => entry.DurationMinutes);
        var weekMinutes = weekEntries.Sum(entry => entry.DurationMinutes);
        var allMinutes = selectedTaskEntries.Sum(entry => entry.DurationMinutes);

        var currentWorkDayEarned = currentWorkDayEntries.Sum(entry => entry.EarnedAmount);
        var weekEarned = weekEntries.Sum(entry => entry.EarnedAmount);
        var allEarned = selectedTaskEntries.Sum(entry => entry.EarnedAmount);

        if (selectedTask.State is TimedTaskState.Running or TimedTaskState.Paused)
        {
            var currentMinutes = selectedTask.GetCurrentDuration().TotalMinutes;
            var currentEarned = selectedTask.GetEarnedAmount();
            var taskWorkDate = GetWorkDate(selectedTask.StartedAt.LocalDateTime);

            allMinutes += currentMinutes;
            allEarned += currentEarned;

            if (taskWorkDate == currentWorkDate || selectedTask.State == TimedTaskState.Running)
            {
                currentWorkDayMinutes += currentMinutes;
                currentWorkDayEarned += currentEarned;
            }

            if (taskWorkDate >= weekStartDate)
            {
                weekMinutes += currentMinutes;
                weekEarned += currentEarned;
            }
        }

        TodayTotalsText.Text = $"{FormatHours(currentWorkDayMinutes)} · {currentWorkDayEarned:C}";
        WeekTotalsText.Text = $"{FormatHours(weekMinutes)} · {weekEarned:C}";
        AllTotalsText.Text = $"{FormatHours(allMinutes)} · {allEarned:C}";
    }

    private void RefreshStatusStrip()
    {
        var active = _timerManager.ActiveExclusiveTask;
        var selected = _timerManager.SelectedTask;

        if (active is not null && selected is not null && active.Id != selected.Id)
        {
            StatusStripText.Text = $"Active: {active.DisplayName} · {FormatDuration(active.GetCurrentDuration())} | Selected: {selected.DisplayName}";
            _trayIconService?.UpdateTooltip($"TimerAgent - {active.DisplayName} - {FormatDuration(active.GetCurrentDuration())}");
            return;
        }

        if (active is not null)
        {
            StatusStripText.Text = $"Active: {active.DisplayName} · {FormatDuration(active.GetCurrentDuration())}";
            _trayIconService?.UpdateTooltip($"TimerAgent - {active.DisplayName} - {FormatDuration(active.GetCurrentDuration())}");
            return;
        }

        var runningParallel = _timerManager.RunningTasks.FirstOrDefault();
        if (runningParallel is not null)
        {
            StatusStripText.Text = $"Running: {runningParallel.DisplayName} · {FormatDuration(runningParallel.GetCurrentDuration())}";
            _trayIconService?.UpdateTooltip($"TimerAgent - {runningParallel.DisplayName}");
            return;
        }

        StatusStripText.Text = selected is null ? "No active timed task" : $"Selected: {selected.DisplayName}";
        _trayIconService?.UpdateTooltip("TimerAgent - Stopped");
    }

    private void TimedTasksListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TimedTasksListBox.SelectedItem is TimedTask task)
        {
            _timerManager.SelectTask(task.Id);
            SaveState();
            RefreshUi();
        }
    }

    private void ContactsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ContactsListBox.SelectedItem is ContactProfile contact)
        {
            _selectedContact = contact;
        }
    }

    private void ListStartPauseButton_Click(object sender, RoutedEventArgs e)
    {
        var task = GetSelectedListTaskOrWarn();
        if (task is null)
        {
            return;
        }

        if (task.State == TimedTaskState.Running)
        {
            _timerManager.PauseTask(task.Id);
        }
        else if (task.State == TimedTaskState.Paused)
        {
            _timerManager.ResumeTask(task.Id);
        }
        else
        {
            _timerManager.StartTask(task.Id);
        }

        SaveState();
        RefreshLists();
        RefreshUi();
    }

    private void ListStopButton_Click(object sender, RoutedEventArgs e)
    {
        var task = GetSelectedListTaskOrWarn();
        if (task is null)
        {
            return;
        }

        StopTaskAndWriteLog(task);
        SaveState();
        RefreshLists();
        RefreshUi();
    }

    private TimedTask? GetSelectedListTaskOrWarn()
    {
        if (TimedTasksListBox.SelectedItem is TimedTask task)
        {
            _timerManager.SelectTask(task.Id);
            return task;
        }

        var selectedTask = _timerManager.SelectedTask;
        if (selectedTask is not null)
        {
            return selectedTask;
        }

        ShowWarning("Select a timed task first.");
        return null;
    }

    private void SelectTaskButton_Click(object sender, RoutedEventArgs e)
    {
        if (TimedTasksListBox.SelectedItem is not TimedTask task)
        {
            ShowWarning("Select a timed task first.");
            return;
        }

        _timerManager.SelectTask(task.Id);
        SaveState();
        ShowPage(TimerAgentPage.Timer);
    }

    private void SmartTimerActionButton_Click(object sender, RoutedEventArgs e)
    {
        var task = _timerManager.SelectedTask;
        if (task is null)
        {
            ShowWarning("Select a timed task first.");
            return;
        }

        if (task.State == TimedTaskState.Running)
        {
            _timerManager.PauseTask(task.Id);
        }
        else if (task.State == TimedTaskState.Paused)
        {
            _timerManager.ResumeTask(task.Id);
        }
        else
        {
            _timerManager.StartTask(task.Id);
        }

        SaveState();
        RefreshLists();
        RefreshUi();
    }

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        var task = _timerManager.SelectedTask;
        if (task is null)
        {
            ShowWarning("Select a timed task first.");
            return;
        }

        if (task.State == TimedTaskState.Paused)
        {
            _timerManager.ResumeTask(task.Id);
        }
        else
        {
            _timerManager.StartTask(task.Id);
        }

        SaveState();
        RefreshLists();
        ShowPage(TimerAgentPage.Timer);
    }

    private void PauseResumeButton_Click(object sender, RoutedEventArgs e)
    {
        var task = _timerManager.SelectedTask;
        if (task is null)
        {
            return;
        }

        if (task.State == TimedTaskState.Running)
        {
            _timerManager.PauseTask(task.Id);
        }
        else if (task.State == TimedTaskState.Paused)
        {
            _timerManager.ResumeTask(task.Id);
        }

        SaveState();
        RefreshLists();
        RefreshUi();
    }

    private void StopButton_Click(object sender, RoutedEventArgs e)
    {
        var task = _timerManager.SelectedTask;
        if (task is null)
        {
            return;
        }

        StopTaskAndWriteLog(task);
        SaveState();
        RefreshLists();
        RefreshUi();
    }

    private void StopTaskAndWriteLog(TimedTask task)
    {
        if (task.State is not (TimedTaskState.Running or TimedTaskState.Paused))
        {
            return;
        }

        var completedTask = _timerManager.StopTask(task.Id);
        if (completedTask is null)
        {
            return;
        }

        try
        {
            var logEntry = TimerLogEntry.FromTimedTask(completedTask);
            _logWriter.Append(logEntry);
        }
        catch (Exception ex)
        {
            WpfMessageBox.Show(
                $"The task stopped, but the log could not be saved.\n\n{ex.Message}",
                "Log save failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void CreateTaskButton_Click(object sender, RoutedEventArgs e)
    {
        _editingTask = null;

        var defaultContact = _selectedContact ?? _contacts.FirstOrDefault();

        TaskDetailsTitle.Text = "Create timed task";
        TaskNameTextBox.Text = string.Empty;
        TaskContactComboBox.SelectedItem = defaultContact;
        TaskProjectTextBox.Text = string.Empty;
        TaskWorkTypeTextBox.Text = defaultContact?.DefaultWorkType ?? string.Empty;
        TaskTypeComboBox.SelectedItem = TimedTaskType.Work;
        TaskModeComboBox.SelectedItem = TimedTaskMode.Exclusive;
        TaskRateTextBox.Text = (defaultContact?.DefaultHourlyRate ?? 35m).ToString(CultureInfo.CurrentCulture);
        TaskTaxTextBox.Text = (defaultContact?.DefaultTaxSetAsidePercent ?? 20m).ToString(CultureInfo.CurrentCulture);
        TaskBillableCheckBox.IsChecked = true;

        ShowPage(TimerAgentPage.TaskDetails);
    }

    private void EditTaskButton_Click(object sender, RoutedEventArgs e)
    {
        var task = _timerManager.SelectedTask;
        if (task is null)
        {
            ShowWarning("Select a timed task first.");
            return;
        }

        _editingTask = task;

        TaskDetailsTitle.Text = "Edit timed task";
        TaskNameTextBox.Text = task.Name;
        TaskContactComboBox.SelectedItem = _contacts.FirstOrDefault(contact => contact.Id == task.ContactId);
        TaskProjectTextBox.Text = task.ProjectName;
        TaskWorkTypeTextBox.Text = task.WorkType;
        TaskTypeComboBox.SelectedItem = task.TaskType;
        TaskModeComboBox.SelectedItem = task.Mode;
        TaskRateTextBox.Text = task.HourlyRate.ToString(CultureInfo.CurrentCulture);
        TaskTaxTextBox.Text = task.TaxSetAsidePercent.ToString(CultureInfo.CurrentCulture);
        TaskBillableCheckBox.IsChecked = task.IsBillable;

        ShowPage(TimerAgentPage.TaskDetails);
    }

    private void SaveTaskButton_Click(object sender, RoutedEventArgs e)
    {
        if (!decimal.TryParse(TaskRateTextBox.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out var rate))
        {
            ShowWarning("Enter a valid hourly rate.");
            return;
        }

        if (!decimal.TryParse(TaskTaxTextBox.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out var taxPercent))
        {
            ShowWarning("Enter a valid tax percentage.");
            return;
        }

        var contact = TaskContactComboBox.SelectedItem as ContactProfile;
        var taskType = TaskTypeComboBox.SelectedItem is TimedTaskType selectedTaskType
            ? selectedTaskType
            : TimedTaskType.Work;
        var mode = TaskModeComboBox.SelectedItem is TimedTaskMode selectedMode
            ? selectedMode
            : TimedTaskMode.Exclusive;

        if (_editingTask is null)
        {
            _timerManager.CreateTask(
                name: TaskNameTextBox.Text,
                contactId: contact?.Id,
                contactName: contact?.DisplayName ?? string.Empty,
                projectName: TaskProjectTextBox.Text,
                workType: TaskWorkTypeTextBox.Text,
                taskType: taskType,
                mode: mode,
                isBillable: TaskBillableCheckBox.IsChecked == true,
                hourlyRate: rate,
                taxSetAsidePercent: taxPercent,
                notes: string.Empty);
        }
        else
        {
            _editingTask.Name = TaskNameTextBox.Text.Trim();
            _editingTask.ContactId = contact?.Id;
            _editingTask.ContactName = contact?.DisplayName ?? string.Empty;
            _editingTask.ProjectName = TaskProjectTextBox.Text.Trim();
            _editingTask.WorkType = TaskWorkTypeTextBox.Text.Trim();
            _editingTask.TaskType = taskType;
            _editingTask.Mode = mode;
            _editingTask.IsBillable = TaskBillableCheckBox.IsChecked == true;
            _editingTask.HourlyRate = rate;
            _editingTask.TaxSetAsidePercent = taxPercent;
        }

        _editingTask = null;
        SaveState();
        RefreshLists();
        ShowPage(TimerAgentPage.List);
    }

    private void ArchiveTaskButton_Click(object sender, RoutedEventArgs e)
    {
        var task = _timerManager.SelectedTask ?? TimedTasksListBox.SelectedItem as TimedTask;
        if (task is null)
        {
            ShowWarning("Select a timed task first.");
            return;
        }

        if (task.State is TimedTaskState.Running or TimedTaskState.Paused)
        {
            ShowWarning("Stop the task before archiving it.");
            return;
        }

        _timerManager.ArchiveTask(task.Id);
        SaveState();
        RefreshLists();
        RefreshUi();
        ShowPage(TimerAgentPage.List);
    }

    private void CreateContactButton_Click(object sender, RoutedEventArgs e)
    {
        _editingContact = null;

        ContactDetailsTitle.Text = "Create contact";
        ContactNameTextBox.Text = string.Empty;
        ContactTypeComboBox.SelectedItem = ContactType.Client;
        ContactDefaultWorkTypeTextBox.Text = string.Empty;
        ContactDefaultRateTextBox.Text = "35";
        ContactDefaultTaxTextBox.Text = "20";

        ShowPage(TimerAgentPage.ContactDetails);
    }

    private void EditContactButton_Click(object sender, RoutedEventArgs e)
    {
        if (ContactsListBox.SelectedItem is not ContactProfile contact)
        {
            ShowWarning("Select a contact first.");
            return;
        }

        _editingContact = contact;

        ContactDetailsTitle.Text = "Edit contact";
        ContactNameTextBox.Text = contact.DisplayName;
        ContactTypeComboBox.SelectedItem = contact.Type;
        ContactDefaultWorkTypeTextBox.Text = contact.DefaultWorkType;
        ContactDefaultRateTextBox.Text = contact.DefaultHourlyRate.ToString(CultureInfo.CurrentCulture);
        ContactDefaultTaxTextBox.Text = contact.DefaultTaxSetAsidePercent.ToString(CultureInfo.CurrentCulture);

        ShowPage(TimerAgentPage.ContactDetails);
    }

    private void SaveContactButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ContactNameTextBox.Text))
        {
            ShowWarning("Enter a contact name.");
            return;
        }

        if (!decimal.TryParse(ContactDefaultRateTextBox.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out var rate))
        {
            ShowWarning("Enter a valid default rate.");
            return;
        }

        if (!decimal.TryParse(ContactDefaultTaxTextBox.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out var tax))
        {
            ShowWarning("Enter a valid default tax percentage.");
            return;
        }

        var type = ContactTypeComboBox.SelectedItem is ContactType selectedType
            ? selectedType
            : ContactType.Other;

        if (_editingContact is null)
        {
            var contact = new ContactProfile
            {
                DisplayName = ContactNameTextBox.Text.Trim(),
                Type = type,
                DefaultWorkType = ContactDefaultWorkTypeTextBox.Text.Trim(),
                DefaultHourlyRate = rate,
                DefaultTaxSetAsidePercent = tax
            };

            _contacts.Add(contact);
            _selectedContact = contact;
        }
        else
        {
            _editingContact.DisplayName = ContactNameTextBox.Text.Trim();
            _editingContact.Type = type;
            _editingContact.DefaultWorkType = ContactDefaultWorkTypeTextBox.Text.Trim();
            _editingContact.DefaultHourlyRate = rate;
            _editingContact.DefaultTaxSetAsidePercent = tax;
            _editingContact.UpdatedAt = DateTimeOffset.Now;
            _selectedContact = _editingContact;
        }

        _editingContact = null;
        SaveState();
        RefreshLists();
        ShowPage(TimerAgentPage.Contacts);
    }

    private void SelectContactButton_Click(object sender, RoutedEventArgs e)
    {
        if (ContactsListBox.SelectedItem is not ContactProfile contact)
        {
            ShowWarning("Select a contact first.");
            return;
        }

        _selectedContact = contact;
        CreateTaskButton_Click(sender, e);
    }

    private void ArchiveContactButton_Click(object sender, RoutedEventArgs e)
    {
        if (ContactsListBox.SelectedItem is not ContactProfile contact)
        {
            ShowWarning("Select a contact first.");
            return;
        }

        contact.IsActive = false;

        if (_selectedContact?.Id == contact.Id)
        {
            _selectedContact = _contacts.FirstOrDefault(activeContact => activeContact.IsActive);
        }

        SaveState();
        RefreshLists();
        RefreshUi();
    }

    private void CancelDetailsButton_Click(object sender, RoutedEventArgs e)
    {
        _editingTask = null;
        _editingContact = null;
        ShowPage(TimerAgentPage.List);
    }

    private void TaskContactComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TaskContactComboBox.SelectedItem is not ContactProfile contact)
        {
            return;
        }

        // When editing an existing task, do not auto-overwrite saved task values.
        if (_editingTask is not null)
        {
            return;
        }

        // When creating a new task, changing the contact should apply that contact's defaults.
        TaskRateTextBox.Text = contact.DefaultHourlyRate.ToString(CultureInfo.CurrentCulture);
        TaskTaxTextBox.Text = contact.DefaultTaxSetAsidePercent.ToString(CultureInfo.CurrentCulture);
        TaskWorkTypeTextBox.Text = contact.DefaultWorkType;
    }

    private static DateOnly GetCurrentWorkDate(IReadOnlyCollection<TimerLogEntry> logEntries)
    {
        var now = DateTime.Now;
        var defaultWorkDate = GetWorkDate(now);

        if (now.TimeOfDay < WorkDayCutoff)
        {
            return defaultWorkDate;
        }

        var lastEndedAt = logEntries
            .Select(GetEntryEndDateTime)
            .Where(endedAt => endedAt <= now)
            .OrderByDescending(endedAt => endedAt)
            .FirstOrDefault();

        if (lastEndedAt != default && now - lastEndedAt <= NewWorkDayGap)
        {
            return GetWorkDate(lastEndedAt);
        }

        return defaultWorkDate;
    }

    private static DateOnly GetEntryWorkDate(TimerLogEntry entry)
    {
        return GetWorkDate(GetEntryStartDateTime(entry));
    }

    private static DateOnly GetWorkDate(DateTime dateTime)
    {
        return DateOnly.FromDateTime(dateTime.Subtract(WorkDayCutoff));
    }

    private static DateTime GetEntryStartDateTime(TimerLogEntry entry)
    {
        return entry.Date.ToDateTime(entry.StartTime);
    }

    private static DateTime GetEntryEndDateTime(TimerLogEntry entry)
    {
        var start = GetEntryStartDateTime(entry);
        var end = entry.Date.ToDateTime(entry.EndTime);

        if (end < start)
        {
            end = end.AddDays(1);
        }

        return end;
    }

    private static DateTime GetStartOfWeek(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.Date.AddDays(-diff);
    }

    private static string FormatDuration(TimeSpan duration)
    {
        return duration.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);
    }

    private static string FormatHours(double minutes)
    {
        var hours = minutes / 60d;
        return $"{hours:0.##}h";
    }

    private void ShowWarning(string message)
    {
        WpfMessageBox.Show(
            message,
            "Life OS TimerAgent",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
    }
}


