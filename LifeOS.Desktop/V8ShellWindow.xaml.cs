using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using LifeOS.Shared.V8;
using LifeOS.Core.IntegrationInbox;

namespace LifeOS.Desktop;

public partial class V8ShellWindow : Window
{
    private static readonly string[] WorkspaceOrder =
    {
        "Home",
        "Work",
        "Career",
        "Money",
        "Life",
        "Projects",
        "Assistant",
        "Settings"
    };

    private V8Preferences _preferences = V8PreferenceStore.Load();
    private string _activeWorkspace = "Home";
    private bool _contextOpen;
    private IInputElement? _focusBeforeCommand;
    private IInputElement? _focusBeforeContext;
    private IntegrationControlCentreView? _integrationControlCentreView;
    private IntegrationInboxView? _integrationInboxView;
    private MicrosoftProviderView? _microsoftProviderView;
    private Group49MicrosoftFilesView? _group49MicrosoftFilesView;
    private Group50TeamsView? _group50TeamsView;
    private Group51GoogleWorkspaceView? _group51GoogleWorkspaceView;
    private WorkspaceSnapshot _snapshot = WorkspaceSnapshot.Load();

    private bool IsCommandOpen => CommandOverlay.Visibility == Visibility.Visible;

    public V8ShellWindow()
    {
        WorkspaceCatalog.Validate(MainWindow.V8RouteIds);
        InitializeComponent();

        Loaded += Window_Loaded;
        SizeChanged += Window_SizeChanged;

        ConfigureSettingsControls();
        ApplyPreferencesToUi();
        UpdateIntegrationReviewCount();

        string startupWorkspace = _preferences.StartupMode == V8StartupMode.LastUsed
            ? _preferences.LastWorkspace
            : "Home";

        NavigateTo(startupWorkspace, persist: false);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        ApplyResponsiveLayout();
        ApplyDensity();

        if (_preferences.ContextPanelOpen && ActualWidth > 1120)
        {
            SetContextOpen(true, persist: false, returnFocusOnClose: false);
        }
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        ApplyResponsiveLayout();
        ApplyDensity();
    }

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        _preferences.LastWorkspace = _activeWorkspace;
        _preferences.ContextPanelOpen = _contextOpen;

        try
        {
            V8PreferenceStore.Save(_preferences);
        }
        catch (Exception exception) when (
            exception is System.IO.IOException or
            UnauthorizedAccessException)
        {
            // Closing must remain safe even when the local preference file is unavailable.
        }
    }

    private Button[] GetNavigationButtons() =>
        new[]
        {
            HomeNav,
            WorkNav,
            CareerNav,
            MoneyNav,
            LifeNav,
            ProjectsNav,
            AssistantNav,
            SettingsNav
        };

    private void ApplyResponsiveLayout()
    {
        if (!IsLoaded)
        {
            return;
        }

        bool compactWidth = ActualWidth <= 1220 || _contextOpen;
        bool veryCompactWidth = ActualWidth <= 1020 || (_contextOpen && ActualWidth <= 1450);
        bool compactHeight = ActualHeight <= 800;

        WorkspaceTitle.FontSize = compactWidth ? 16 : 17;
        WorkspaceSubtitle.Visibility = compactWidth ? Visibility.Collapsed : Visibility.Visible;

        CommandButton.Width = veryCompactWidth ? 180 : compactWidth ? 250 : 400;
        CommandButton.MaxWidth = CommandButton.Width;
        CommandLabel.Text = compactWidth ? "Search" : "Search or run a command";
        CommandShortcut.Visibility = veryCompactWidth ? Visibility.Collapsed : Visibility.Visible;
        CommandShortcut.Margin = compactWidth
            ? new Thickness(8, 0, 0, 0)
            : new Thickness(16, 0, 0, 0);

        ContextButton.Visibility = veryCompactWidth ? Visibility.Collapsed : Visibility.Visible;
        ProfileButton.Content = veryCompactWidth ? GetProfileInitials() : $"{GetProfileInitials()} ▾";

        foreach (Button button in TopBarActions.Children.OfType<Button>())
        {
            button.MinWidth = 0;
            button.Padding = compactWidth
                ? new Thickness(7, 6, 7, 6)
                : new Thickness(12, 8, 12, 8);
            button.Margin = compactWidth
                ? new Thickness(2, 0, 2, 0)
                : new Thickness(4, 0, 4, 0);
            button.FontSize = compactWidth ? 12 : 13;
        }

        foreach (Button button in GetNavigationButtons())
        {
            button.Height = compactHeight ? 56 : 62;
            button.Margin = compactHeight
                ? new Thickness(0, 1, 0, 1)
                : new Thickness(0, 3, 0, 3);
            button.Padding = compactHeight
                ? new Thickness(0, 2, 0, 2)
                : new Thickness(0);

            TextBlock[] navigationText = FindVisualChildren<TextBlock>(button).ToArray();

            if (navigationText.Length >= 2)
            {
                navigationText[0].FontSize = compactHeight ? 18 : 19;
                navigationText[1].FontSize = compactHeight ? 10 : 11;
            }
        }

        NavigationStack.Margin = compactHeight
            ? new Thickness(8, 0, 8, 0)
            : new Thickness(8, 8, 8, 12);

        if (ActualWidth <= 1120 && _contextOpen)
        {
            SetContextOpen(false, persist: false, returnFocusOnClose: false);
        }
    }

    private void ApplyDensity()
    {
        if (!IsLoaded)
        {
            return;
        }

        bool compact = _preferences.Density == V8Density.Compact || ActualWidth <= 980;
        WorkspaceRoot.Margin = compact ? new Thickness(16) : new Thickness(24);
        DensityButton.Content = _preferences.Density == V8Density.Compact
            ? "Use Comfortable"
            : "Use Compact";
        DensityStatusText.Text = _preferences.Density == V8Density.Compact
            ? "Compact density"
            : "Comfortable density";

        foreach (Border border in FindVisualChildren<Border>(WorkspaceRoot))
        {
            if (string.Equals(border.Tag as string, "WorkspaceMetricCard", StringComparison.Ordinal))
            {
                border.Width = compact ? 184 : 220;
                border.Padding = compact ? new Thickness(12) : new Thickness(16);
                border.Margin = compact
                    ? new Thickness(0, 0, 8, 8)
                    : new Thickness(0, 0, 12, 12);
            }
            else if (string.Equals(border.Tag as string, "WorkspaceModuleCard", StringComparison.Ordinal))
            {
                border.Width = compact ? 292 : 348;
                border.Padding = compact ? new Thickness(14) : new Thickness(18);
                border.Margin = compact
                    ? new Thickness(0, 0, 8, 8)
                    : new Thickness(0, 0, 12, 12);
            }
        }

        _integrationControlCentreView?.ApplyDensity(compact);
        _integrationInboxView?.ApplyDensity(compact);
        _microsoftProviderView?.ApplyDensity(compact);
    }

    private void WorkspaceNav_Click(object sender, RoutedEventArgs e)
    {
        if (IsCommandOpen)
        {
            return;
        }

        if (sender is Button { Tag: string workspace })
        {
            NavigateTo(workspace);
        }
    }

    private void NavigateTo(string? requestedWorkspace, bool persist = true)
    {
        if (!WorkspaceCatalog.TryGet(requestedWorkspace, out WorkspaceDefinition definition))
        {
            definition = WorkspaceCatalog.Get("Home");
        }

        _activeWorkspace = definition.Name;
        _snapshot = WorkspaceSnapshot.Load();

        WorkspaceTitle.Text = definition.Name;
        WorkspaceSubtitle.Text = definition.Subtitle;
        WorkspaceEyebrow.Text = definition.Eyebrow;
        WorkspaceDisplayTitle.Text = definition.Name == "Home"
            ? $"Good afternoon, {GetFirstName()}."
            : definition.DisplayTitle;
        WorkspaceDescription.Text = definition.Description;

        MetricItems.ItemsSource = definition.Metrics
            .Select(metric => new WorkspaceMetricView(
                metric.Label,
                _snapshot.Resolve(metric),
                metric.Detail))
            .ToArray();

        SectionItems.ItemsSource = definition.Sections;
        SectionItems.Visibility = definition.Name == "Settings"
            ? Visibility.Collapsed
            : Visibility.Visible;
        SettingsRoot.Visibility = definition.Name == "Settings"
            ? Visibility.Visible
            : Visibility.Collapsed;
        ShowSettingsOverview(scrollToTop: false);

        ContextTitle.Text = $"{definition.Name} context";
        ContextBody.Text = definition.ContextSummary;
        ContextModulesList.ItemsSource = definition.Sections
            .SelectMany(section => section.Modules)
            .Select(module => $"{module.Title} — {module.Status}")
            .ToArray();

        WorkspaceScrollViewer.ScrollToTop();
        UpdateNavigationSelection();

        _preferences.LastWorkspace = definition.Name;

        if (persist)
        {
            SavePreferencesSilently();
        }

        Dispatcher.BeginInvoke(
            DispatcherPriority.Loaded,
            new Action(ApplyDensity));
    }

    private void UpdateNavigationSelection()
    {
        Brush accent = (Brush)FindResource("LifeOS.Brush.Accent");
        Brush accentSoft = (Brush)FindResource("LifeOS.Brush.AccentSoft");

        foreach (Button button in GetNavigationButtons())
        {
            bool selected = string.Equals(
                button.Tag as string,
                _activeWorkspace,
                StringComparison.OrdinalIgnoreCase);

            button.Background = selected ? accentSoft : Brushes.Transparent;
            button.BorderBrush = selected ? accent : Brushes.Transparent;
        }
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent)
        where T : DependencyObject
    {
        for (int index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, index);

            if (child is T typed)
            {
                yield return typed;
            }

            foreach (T descendant in FindVisualChildren<T>(child))
            {
                yield return descendant;
            }
        }
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        bool controlK =
            (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
            e.Key == Key.K;
        bool workspaceShortcut =
            (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt &&
            e.Key is >= Key.D1 and <= Key.D8;

        if (IsCommandOpen)
        {
            if (e.Key == Key.Escape)
            {
                CloseCommand();
                e.Handled = true;
                return;
            }

            if (controlK)
            {
                Keyboard.Focus(CommandTextBox);
                e.Handled = true;
                return;
            }

            if (workspaceShortcut)
            {
                e.Handled = true;
            }

            return;
        }

        if (e.Key == Key.Escape && IsSettingsSubpageOpen)
        {
            ShowSettingsOverview(scrollToTop: true);
            e.Handled = true;
            return;
        }

        if (controlK)
        {
            OpenCommand();
            e.Handled = true;
            return;
        }

        if (workspaceShortcut)
        {
            NavigateTo(WorkspaceOrder[(int)e.Key - (int)Key.D1]);
            e.Handled = true;
        }
    }

    private void CommandButton_Click(object sender, RoutedEventArgs e) => OpenCommand();

    private void CloseCommand_Click(object sender, RoutedEventArgs e) => CloseCommand();

    private void OpenCommand()
    {
        if (IsCommandOpen)
        {
            Keyboard.Focus(CommandTextBox);
            return;
        }

        _focusBeforeCommand = Keyboard.FocusedElement;
        CommandOverlay.Visibility = Visibility.Visible;
        CommandTextBox.Clear();
        Keyboard.Focus(CommandTextBox);
    }

    private void CloseCommand()
    {
        if (!IsCommandOpen)
        {
            return;
        }

        CommandOverlay.Visibility = Visibility.Collapsed;
        IInputElement? previousFocus = _focusBeforeCommand;
        _focusBeforeCommand = null;

        if (previousFocus is UIElement previousElement &&
            previousElement.IsVisible &&
            previousElement.IsEnabled)
        {
            Keyboard.Focus(previousElement);
            return;
        }

        Keyboard.Focus(CommandButton);
    }

    private void CommandTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        string command = CommandTextBox.Text.Trim();
        string? workspace = WorkspaceOrder.FirstOrDefault(candidate =>
            string.Equals(candidate, command, StringComparison.OrdinalIgnoreCase));

        if (workspace is not null)
        {
            CloseCommand();
            NavigateTo(workspace);
            e.Handled = true;
            return;
        }

        bool handled = TryApplyPreferenceCommand(command);

        if (handled)
        {
            CloseCommand();
            e.Handled = true;
        }
    }

    private bool TryApplyPreferenceCommand(string command)
    {
        if (command.Equals("Theme light", StringComparison.OrdinalIgnoreCase))
        {
            SetTheme(V8Theme.Light);
            return true;
        }

        if (command.Equals("Theme dark", StringComparison.OrdinalIgnoreCase))
        {
            SetTheme(V8Theme.Dark);
            return true;
        }

        if (command.Equals("Theme system", StringComparison.OrdinalIgnoreCase))
        {
            SetTheme(V8Theme.System);
            return true;
        }

        if (command.Equals("Theme high contrast", StringComparison.OrdinalIgnoreCase) ||
            command.Equals("High contrast", StringComparison.OrdinalIgnoreCase))
        {
            SetTheme(V8Theme.HighContrast);
            return true;
        }

        if (command.Equals("Accent purple", StringComparison.OrdinalIgnoreCase))
        {
            SetAccent(V8Accent.Purple);
            return true;
        }

        if (command.Equals("Accent blue", StringComparison.OrdinalIgnoreCase))
        {
            SetAccent(V8Accent.Blue);
            return true;
        }

        if (command.Equals("Accent teal", StringComparison.OrdinalIgnoreCase))
        {
            SetAccent(V8Accent.Teal);
            return true;
        }

        if (command.Equals("Density compact", StringComparison.OrdinalIgnoreCase))
        {
            SetDensity(V8Density.Compact);
            return true;
        }

        if (command.Equals("Density comfortable", StringComparison.OrdinalIgnoreCase))
        {
            SetDensity(V8Density.Comfortable);
            return true;
        }

        return false;
    }

    private void DensityButton_Click(object sender, RoutedEventArgs e)
    {
        SetDensity(_preferences.Density == V8Density.Compact
            ? V8Density.Comfortable
            : V8Density.Compact);
    }

    private void SetDensity(V8Density density)
    {
        _preferences.Density = density;
        DensityComboBox.SelectedItem = density;
        SavePreferencesSilently();
        ApplyDensity();
    }

    private void SetTheme(V8Theme theme)
    {
        _preferences.Theme = theme;
        ThemeComboBox.SelectedItem = theme;
        SavePreferencesSilently();
        V8ThemeManager.Apply(_preferences);
        UpdateNavigationSelection();
        ApplyDensity();
    }

    private void SetAccent(V8Accent accent)
    {
        _preferences.Accent = accent;
        AccentComboBox.SelectedItem = accent;
        SavePreferencesSilently();
        V8ThemeManager.Apply(_preferences);
        UpdateNavigationSelection();
    }

    private void ContextButton_Click(object sender, RoutedEventArgs e)
    {
        if (ActualWidth <= 1120)
        {
            SetContextOpen(false, persist: true, returnFocusOnClose: false);
            return;
        }

        SetContextOpen(!_contextOpen, persist: true, returnFocusOnClose: true);
    }

    private void CloseContextButton_Click(object sender, RoutedEventArgs e) =>
        SetContextOpen(false, persist: true, returnFocusOnClose: true);

    private void SetContextOpen(bool open, bool persist, bool returnFocusOnClose)
    {
        if (open && ActualWidth <= 1120)
        {
            open = false;
        }

        if (open && !_contextOpen)
        {
            _focusBeforeContext = Keyboard.FocusedElement;
        }

        _contextOpen = open;
        ContextColumn.Width = open ? new GridLength(340) : new GridLength(0);
        ContextButton.Content = open ? "Hide context" : "Context";

        if (IsLoaded)
        {
            ApplyResponsiveLayout();
        }

        if (persist)
        {
            _preferences.ContextPanelOpen = open;
            SavePreferencesSilently();
        }

        if (!open && returnFocusOnClose)
        {
            RestoreContextFocus();
        }
    }

    private void RestoreContextFocus()
    {
        IInputElement? previousFocus = _focusBeforeContext;
        _focusBeforeContext = null;

        if (previousFocus is UIElement previousElement &&
            previousElement.IsVisible &&
            previousElement.IsEnabled)
        {
            Keyboard.Focus(previousElement);
            return;
        }

        Keyboard.Focus(ContextButton);
    }

    private bool IsSettingsSubpageOpen =>
        SettingsSubpageHost.Visibility == Visibility.Visible;

    private void OpenIntegrationControlCentre_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!string.Equals(
                _activeWorkspace,
                "Settings",
                StringComparison.OrdinalIgnoreCase))
        {
            NavigateTo("Settings");
        }

        _integrationControlCentreView ??=
            CreateIntegrationControlCentreView();
        _integrationControlCentreView.ApplyDensity(
            _preferences.Density == V8Density.Compact ||
            ActualWidth <= 980);

        ShowSettingsSubpage(_integrationControlCentreView);
    }

    private IntegrationControlCentreView
        CreateIntegrationControlCentreView()
    {
        IntegrationControlCentreView view = new(
            _preferences.Density == V8Density.Compact ||
            ActualWidth <= 980);
        view.BackRequested += (_, _) =>
            ShowSettingsOverview(scrollToTop: true);
        return view;
    }

    private void OpenIntegrationInbox_Click(
        object sender,
        RoutedEventArgs e) =>
        OpenIntegrationInbox();

    private void OpenIntegrationInbox()
    {
        if (!string.Equals(
                _activeWorkspace,
                "Settings",
                StringComparison.OrdinalIgnoreCase))
        {
            NavigateTo("Settings");
        }

        _integrationInboxView ??= CreateIntegrationInboxView();
        _integrationInboxView.ApplyDensity(
            _preferences.Density == V8Density.Compact ||
            ActualWidth <= 980);

        ShowSettingsSubpage(_integrationInboxView);
        UpdateIntegrationReviewCount(
            _integrationInboxView.CurrentReviewCount);
    }

    private IntegrationInboxView CreateIntegrationInboxView()
    {
        IntegrationInboxView view = new(
            _preferences.Density == V8Density.Compact ||
            ActualWidth <= 980);
        view.BackRequested += (_, _) =>
            ShowSettingsOverview(scrollToTop: true);
        view.ReviewCountChanged +=
            count => UpdateIntegrationReviewCount(count);
        return view;
    }

    private void OpenMicrosoftProvider_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!string.Equals(
                _activeWorkspace,
                "Settings",
                StringComparison.OrdinalIgnoreCase))
        {
            NavigateTo("Settings");
        }

        _microsoftProviderView ??=
            CreateMicrosoftProviderView();
        _microsoftProviderView.ApplyDensity(
            _preferences.Density == V8Density.Compact ||
            ActualWidth <= 980);

        ShowSettingsSubpage(_microsoftProviderView);
    }

    private MicrosoftProviderView CreateMicrosoftProviderView()
    {
        MicrosoftProviderView view = new(
            _preferences.Density == V8Density.Compact ||
            ActualWidth <= 980);
        view.BackRequested += (_, _) =>
            ShowSettingsOverview(scrollToTop: true);
        view.ReviewCountChanged +=
            count => UpdateIntegrationReviewCount(count);
        return view;
    }


    private void OpenGroup49MicrosoftFiles_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!string.Equals(
                _activeWorkspace,
                "Settings",
                StringComparison.OrdinalIgnoreCase))
        {
            NavigateTo("Settings");
        }

        _group49MicrosoftFilesView ??=
            CreateGroup49MicrosoftFilesView();

        ShowSettingsSubpage(_group49MicrosoftFilesView);
    }

    private Group49MicrosoftFilesView CreateGroup49MicrosoftFilesView()
    {
        Group49MicrosoftFilesView view = new();
        view.BackRequested += (_, _) =>
            ShowSettingsOverview(scrollToTop: true);
        return view;
    }


    private void OpenGroup50Teams_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!string.Equals(
                _activeWorkspace,
                "Settings",
                StringComparison.OrdinalIgnoreCase))
        {
            NavigateTo("Settings");
        }

        _group50TeamsView ??= CreateGroup50TeamsView();
        ShowSettingsSubpage(_group50TeamsView);
    }

    private Group50TeamsView CreateGroup50TeamsView()
    {
        Group50TeamsView view = new();
        view.BackRequested += (_, _) =>
            ShowSettingsOverview(scrollToTop: true);
        return view;
    }


    private void OpenGroup51GoogleWorkspace_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!string.Equals(
                _activeWorkspace,
                "Settings",
                StringComparison.OrdinalIgnoreCase))
        {
            NavigateTo("Settings");
        }

        _group51GoogleWorkspaceView ??=
            CreateGroup51GoogleWorkspaceView();
        ShowSettingsSubpage(_group51GoogleWorkspaceView);
    }

    private Group51GoogleWorkspaceView CreateGroup51GoogleWorkspaceView()
    {
        Group51GoogleWorkspaceView view = new();
        view.BackRequested += (_, _) =>
            ShowSettingsOverview(scrollToTop: true);
        return view;
    }


    private void ShowSettingsSubpage(
        UserControl subpage)
    {
        SettingsOverviewPanel.Visibility = Visibility.Collapsed;
        SettingsSubpageHost.Content = subpage;
        SettingsSubpageHost.Visibility = Visibility.Visible;
        WorkspaceHeaderGrid.Visibility = Visibility.Collapsed;
        MetricItems.Visibility = Visibility.Collapsed;
        WorkspaceScrollViewer.ScrollToTop();
        Keyboard.Focus(subpage);
    }

    private void ShowSettingsOverview(bool scrollToTop)
    {
        SettingsSubpageHost.Visibility = Visibility.Collapsed;
        SettingsOverviewPanel.Visibility = Visibility.Visible;
        WorkspaceHeaderGrid.Visibility = Visibility.Visible;
        MetricItems.Visibility = Visibility.Visible;

        if (scrollToTop)
        {
            WorkspaceScrollViewer.ScrollToTop();
            Keyboard.Focus(SettingsNav);
        }
    }

    private void UpdateIntegrationReviewCount(
        int? knownCount = null)
    {
        int count;

        if (knownCount.HasValue)
        {
            count = knownCount.Value;
        }
        else
        {
            try
            {
                IntegrationInboxV9State state =
                    Group49IntegrationInboxMigration.LoadOrCreateProofState(
                        DateTimeOffset.UtcNow);
                count = new IntegrationInboxV9Service(state)
                    .GetReviewCount();
            }
            catch (Exception exception) when (
                exception is System.IO.IOException or
                UnauthorizedAccessException)
            {
                count = 0;
            }
        }

        ReviewButton.Content = $"Review {count}";
    }

    private void OpenModule_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string routeId } ||
            string.IsNullOrWhiteSpace(routeId))
        {
            return;
        }

        if (routeId.StartsWith("workspace:", StringComparison.OrdinalIgnoreCase))
        {
            NavigateTo(routeId["workspace:".Length..]);
            return;
        }

        if (string.Equals(
                routeId,
                "integration-inbox",
                StringComparison.OrdinalIgnoreCase))
        {
            OpenIntegrationInbox();
            return;
        }

        if (string.Equals(
                routeId,
                "v11-money-foundation",
                StringComparison.OrdinalIgnoreCase))
        {
            new MoneyV11Window { Owner = this }.Show();
            return;
        }

        if (!WorkspaceCatalog.IsRouteAllowed(_activeWorkspace, routeId))
        {
            MessageBox.Show(
                "This module is not assigned to the active workspace.",
                "LifeOS workspace boundary",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        MainWindow moduleWindow = new()
        {
            Owner = this,
            Title = $"LifeOS — {routeId}"
        };

        moduleWindow.OpenV8ModuleWindow(routeId);
        moduleWindow.Show();
    }

    private void ReviewButton_Click(
        object sender,
        RoutedEventArgs e) =>
        OpenIntegrationInbox();

    private void StatusButton_Click(object sender, RoutedEventArgs e)
    {
        OpenContext(
            "System and sync status",
            "Local data is healthy. Companion and integration state remain review-first. No pending transfer mutates a workspace automatically.",
            forceOpen: true);
    }

    private void ProfileButton_Click(object sender, RoutedEventArgs e)
    {
        OpenContext(
            "Profile and active context",
            $"Profile: {_preferences.ProfileName}. Active context: {_preferences.ActiveContext}. Workspace assignments do not change trust or evidence boundaries.",
            forceOpen: true);
    }

    private void StopButton_Click(object sender, RoutedEventArgs e)
    {
        _preferences.EmergencyStopState = _preferences.EmergencyStopState switch
        {
            V8EmergencyStopState.Idle => V8EmergencyStopState.Armed,
            V8EmergencyStopState.Armed => V8EmergencyStopState.Stopped,
            _ => V8EmergencyStopState.Idle
        };

        SavePreferencesSilently();
        UpdateStopVisual();

        string body = _preferences.EmergencyStopState switch
        {
            V8EmergencyStopState.Armed =>
                "Emergency Stop is armed. Press Stop again to enter the stopped state. No work resumes automatically.",
            V8EmergencyStopState.Stopped =>
                "Emergency Stop is active and the shell is stopped. Records and evidence remain intact. Press again only to return the shell control to idle; work still requires review.",
            _ =>
                "Emergency Stop is idle. Guarded work remains manual, foreground-only and review-bound."
        };

        OpenContext("Emergency Stop", body, forceOpen: true);
    }

    private void OpenContext(string title, string body, bool forceOpen)
    {
        ContextTitle.Text = title;
        ContextBody.Text = body;

        bool shouldOpen = forceOpen || _preferences.ContextPanelAutoOpen;

        if (shouldOpen && ActualWidth > 1120)
        {
            SetContextOpen(true, persist: true, returnFocusOnClose: true);
        }
    }

    private void ConfigureSettingsControls()
    {
        ThemeComboBox.ItemsSource = Enum.GetValues<V8Theme>();
        AccentComboBox.ItemsSource = Enum.GetValues<V8Accent>();
        DensityComboBox.ItemsSource = Enum.GetValues<V8Density>();
        StartupComboBox.ItemsSource = Enum.GetValues<V8StartupMode>();
        TextScaleComboBox.ItemsSource = new[] { "100%", "110%", "125%", "140%" };
    }

    private void ApplyPreferencesToUi()
    {
        _preferences.Normalize();
        V8ThemeManager.Apply(_preferences);

        ThemeComboBox.SelectedItem = _preferences.Theme;
        AccentComboBox.SelectedItem = _preferences.Accent;
        DensityComboBox.SelectedItem = _preferences.Density;
        StartupComboBox.SelectedItem = _preferences.StartupMode;
        TextScaleComboBox.SelectedIndex = TextScaleToIndex(_preferences.TextScale);
        ReducedMotionCheckBox.IsChecked = _preferences.ReducedMotion;
        ContextAutoOpenCheckBox.IsChecked = _preferences.ContextPanelAutoOpen;
        ProfileNameTextBox.Text = _preferences.ProfileName;
        ActiveContextTextBox.Text = _preferences.ActiveContext;

        UpdateStopVisual();
        UpdateProfileVisual();
    }

    private void UpdateStopVisual()
    {
        StopButton.Content = _preferences.EmergencyStopState switch
        {
            V8EmergencyStopState.Armed => "STOP ARMED",
            V8EmergencyStopState.Stopped => "STOPPED",
            _ => "Stop"
        };

        string brushKey = _preferences.EmergencyStopState switch
        {
            V8EmergencyStopState.Armed => "LifeOS.Brush.Warning",
            V8EmergencyStopState.Stopped => "LifeOS.Brush.Danger",
            _ => "LifeOS.Brush.Border"
        };

        StopButton.BorderBrush = (Brush)FindResource(brushKey);
        StopButton.ToolTip = $"Emergency Stop: {_preferences.EmergencyStopState}";
        SettingsStopStatusText.Text = $"Emergency Stop: {_preferences.EmergencyStopState}";
    }

    private void UpdateProfileVisual()
    {
        ProfileButton.Content = ActualWidth <= 1020
            ? GetProfileInitials()
            : $"{GetProfileInitials()} ▾";
    }

    private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        _preferences.Theme = ThemeComboBox.SelectedItem is V8Theme theme
            ? theme
            : V8Theme.Dark;
        _preferences.Accent = AccentComboBox.SelectedItem is V8Accent accent
            ? accent
            : V8Accent.Purple;
        _preferences.Density = DensityComboBox.SelectedItem is V8Density density
            ? density
            : V8Density.Comfortable;
        _preferences.StartupMode = StartupComboBox.SelectedItem is V8StartupMode startup
            ? startup
            : V8StartupMode.Home;
        _preferences.TextScale = IndexToTextScale(TextScaleComboBox.SelectedIndex);
        _preferences.ReducedMotion = ReducedMotionCheckBox.IsChecked == true;
        _preferences.ContextPanelAutoOpen = ContextAutoOpenCheckBox.IsChecked == true;
        _preferences.ProfileName = ProfileNameTextBox.Text;
        _preferences.ActiveContext = ActiveContextTextBox.Text;
        _preferences.LastWorkspace = _activeWorkspace;
        _preferences.ContextPanelOpen = _contextOpen;
        _preferences.Normalize();

        try
        {
            V8PreferenceStore.Save(_preferences);
            ApplyPreferencesToUi();
            ApplyDensity();
            UpdateNavigationSelection();
            SettingsSaveStatusText.Text = "Saved locally. Approved settings are active.";
            NavigateTo(_activeWorkspace, persist: false);
        }
        catch (Exception exception) when (
            exception is System.IO.IOException or
            UnauthorizedAccessException)
        {
            SettingsSaveStatusText.Text = "Could not save the local preference file.";
        }
    }

    private void ResetSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBoxResult result = MessageBox.Show(
            "Reset shell preferences to approved defaults? Module records will not be changed.",
            "Reset LifeOS v8 settings",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        _preferences = new V8Preferences().Normalize();
        V8PreferenceStore.Save(_preferences);
        ApplyPreferencesToUi();
        SetContextOpen(false, persist: false, returnFocusOnClose: false);
        NavigateTo("Home", persist: false);
        SettingsSaveStatusText.Text = "Approved defaults restored. Module records were not changed.";
    }

    private void SavePreferencesSilently()
    {
        try
        {
            V8PreferenceStore.Save(_preferences);
        }
        catch (Exception exception) when (
            exception is System.IO.IOException or
            UnauthorizedAccessException)
        {
            // Shell operation remains available when local preference persistence is unavailable.
        }
    }

    private string GetFirstName()
    {
        string normalized = _preferences.ProfileName.Trim();
        int separator = normalized.IndexOf(' ');
        return separator > 0 ? normalized[..separator] : normalized;
    }

    private string GetProfileInitials()
    {
        string[] parts = _preferences.ProfileName
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length == 0)
        {
            return "CS";
        }

        return string.Concat(parts.Take(2).Select(part => char.ToUpperInvariant(part[0])));
    }

    private static int TextScaleToIndex(double scale)
    {
        if (Math.Abs(scale - 1.1) < 0.001)
        {
            return 1;
        }

        if (Math.Abs(scale - 1.25) < 0.001)
        {
            return 2;
        }

        return Math.Abs(scale - 1.4) < 0.001 ? 3 : 0;
    }

    private static double IndexToTextScale(int index) => index switch
    {
        1 => 1.1,
        2 => 1.25,
        3 => 1.4,
        _ => 1.0
    };
}
