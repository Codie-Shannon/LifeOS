using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using LifeOS.Modules.Timer.Models;
using LifeOS.Modules.Timer.Services;
using LifeOS.Modules.Timer.Storage;
using LifeOS.TimerAgent.Services;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

namespace LifeOS.TimerAgent;

public partial class MainWindow : Window
{
    private readonly TimerService _timerService;
    private readonly TimerCsvLogWriter _logWriter;
    private readonly DispatcherTimer _uiTimer;

    private GlobalHotKeyService? _hotKeyService;
    private TrayIconService? _trayIconService;
    private bool _allowClose;

    public MainWindow()
    {
        InitializeComponent();

        Loaded += MainWindow_Loaded;
        PreviewKeyDown += MainWindow_PreviewKeyDown;

        _timerService = new TimerService();

        var appDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LifeOS",
            "TimerAgent");

        var logFilePath = Path.Combine(appDataFolder, "timer-log.csv");
        _logWriter = new TimerCsvLogWriter(logFilePath);

        LogPathDisplay.Text =
            $"Shortcut: Ctrl + Alt + Space to show/hide\nLog file: {logFilePath}";

        _uiTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        _uiTimer.Tick += (_, _) => RefreshTimerUi();

        RefreshTimerUi();
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

    private void SetupTrayIcon()
    {
        _trayIconService = new TrayIconService();

        _trayIconService.ShowRequested += (_, _) =>
        {
            Dispatcher.Invoke(ShowTimerWindow);
        };

        _trayIconService.HideRequested += (_, _) =>
        {
            Dispatcher.Invoke(Hide);
        };

        _trayIconService.ExitRequested += (_, _) =>
        {
            Dispatcher.Invoke(ExitApplication);
        };
    }

    private void SetupGlobalHotKey()
    {
        try
        {
            _hotKeyService = new GlobalHotKeyService();

            _hotKeyService.HotKeyPressed += (_, _) =>
            {
                Dispatcher.Invoke(ToggleWindowVisibility);
            };

            _hotKeyService.Register(
                this,
                hotKeyId: 9001,
                key: Key.Space,
                modifiers: HotKeyModifiers.Control | HotKeyModifiers.Alt | HotKeyModifiers.NoRepeat);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
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

    private void ExitApplication()
    {
        _allowClose = true;
        Close();
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
        _hotKeyService?.Dispose();
        _trayIconService?.Dispose();

        base.OnClosed(e);
    }

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        if (!TryReadMoneyFields(out var hourlyRate, out var taxPercent))
        {
            return;
        }

        try
        {
            _timerService.Start(
                ClientTextBox.Text,
                ProjectTextBox.Text,
                WorkTypeTextBox.Text,
                BillableCheckBox.IsChecked == true,
                hourlyRate,
                taxPercent,
                NotesTextBox.Text);

            _uiTimer.Start();

            SetInputEnabled(false);
            RefreshTimerUi();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Timer start failed",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private void PauseResumeButton_Click(object sender, RoutedEventArgs e)
    {
        if (_timerService.State == TimerState.Running)
        {
            _timerService.Pause();
        }
        else if (_timerService.State == TimerState.Paused)
        {
            _timerService.Resume();
        }

        RefreshTimerUi();
    }

    private void StopButton_Click(object sender, RoutedEventArgs e)
    {
        var currentSession = _timerService.CurrentSession;

        if (currentSession is not null)
        {
            currentSession.Notes = NotesTextBox.Text.Trim();
        }

        var completedSession = _timerService.Stop();

        if (completedSession is not null)
        {
            try
            {
                var logEntry = TimerLogEntry.FromSession(completedSession);
                _logWriter.Append(logEntry);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"The timer stopped, but the log could not be saved.\n\n{ex.Message}",
                    "Log save failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        _uiTimer.Stop();

        SetInputEnabled(true);
        RefreshTimerUi();

        MessageBox.Show(
            "Timer session saved to CSV.",
            "Life OS TimerAgent",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
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

    private void RefreshTimerUi()
    {
        var duration = _timerService.GetCurrentDuration();
        TimerDisplay.Text = duration.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);

        StateDisplay.Text = _timerService.State.ToString();

        var earned = _timerService.GetCurrentEarnedAmount();
        var tax = _timerService.GetCurrentTaxSetAside();
        var safeAfterTax = earned - tax;

        EarnedDisplay.Text = $"Earned: {earned:C}";
        TaxDisplay.Text = $"Tax set-aside: {tax:C}";
        SafeAfterTaxDisplay.Text = $"Safe after tax: {safeAfterTax:C}";

        StartButton.IsEnabled = _timerService.State == TimerState.Stopped;
        PauseResumeButton.IsEnabled = _timerService.State is TimerState.Running or TimerState.Paused;
        StopButton.IsEnabled = _timerService.State is TimerState.Running or TimerState.Paused;

        PauseResumeButton.Content = _timerService.State == TimerState.Paused
            ? "Resume"
            : "Pause";

        if (_timerService.CurrentSession is not null)
        {
            var client = string.IsNullOrWhiteSpace(_timerService.CurrentSession.ClientName)
                ? "No client"
                : _timerService.CurrentSession.ClientName;

            _trayIconService?.UpdateTooltip(
                $"Life OS TimerAgent - {client} - {TimerDisplay.Text}");
        }
        else
        {
            _trayIconService?.UpdateTooltip("Life OS TimerAgent - Stopped");
        }
    }

    private bool TryReadMoneyFields(out decimal hourlyRate, out decimal taxPercent)
    {
        hourlyRate = 0m;
        taxPercent = 0m;

        if (!decimal.TryParse(RateTextBox.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out hourlyRate))
        {
            MessageBox.Show(
                "Enter a valid hourly rate.",
                "Invalid rate",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            RateTextBox.Focus();
            return false;
        }

        if (hourlyRate < 0)
        {
            MessageBox.Show(
                "Hourly rate cannot be negative.",
                "Invalid rate",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            RateTextBox.Focus();
            return false;
        }

        if (!decimal.TryParse(TaxTextBox.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out taxPercent))
        {
            MessageBox.Show(
                "Enter a valid tax percentage.",
                "Invalid tax percentage",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            TaxTextBox.Focus();
            return false;
        }

        if (taxPercent < 0 || taxPercent > 100)
        {
            MessageBox.Show(
                "Tax percentage must be between 0 and 100.",
                "Invalid tax percentage",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            TaxTextBox.Focus();
            return false;
        }

        return true;
    }

    private void SetInputEnabled(bool isEnabled)
    {
        ClientTextBox.IsEnabled = isEnabled;
        ProjectTextBox.IsEnabled = isEnabled;
        WorkTypeTextBox.IsEnabled = isEnabled;
        RateTextBox.IsEnabled = isEnabled;
        TaxTextBox.IsEnabled = isEnabled;
        BillableCheckBox.IsEnabled = isEnabled;
    }
}