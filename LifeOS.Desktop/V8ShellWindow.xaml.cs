using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using LifeOS.Core;
using LifeOS.Shared.V8;
using LifeOS.Shared.Storage;
using LifeOS.Core.IntegrationInbox;
using LifeOS.Core.Forms;
using LifeOS.Core.ShellSearch;

namespace LifeOS.Desktop;

public partial class V8ShellWindow : Window
{
    private static readonly HashSet<string> ProofOnlyRoutes = new(StringComparer.OrdinalIgnoreCase)
    {
        "assistant",
        "automation-centre",
        "bills-payments",
        "desktop-release",
        "email-radar",
        "final-offline-os",
        "item-state-engine",
        "lifeos-spine",
        "memory",
        "money-profile",
        "os-navigation",
        "payment-calendar",
        "search-knowledge",
        "settings-safety",
        "universal-spine",
        "v11-document-intake",
        "v11-money-foundation",
        "v12-career-studio",
        "v13-grocery-planning"
    };

    private static readonly string[] WorkspaceOrder =
    {
        "Home",
        "Work",
        "Career",
        "Money",
        "Life",
        "Household",
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
    private WorkspaceSnapshot _snapshot;
    private double _workspaceScrollOffset;
    private string? _activeModuleRoute;
    private MainWindow? _embeddedLegacyModuleWindow;

    private bool IsCommandOpen => CommandOverlay.Visibility == Visibility.Visible;

    public V8ShellWindow()
    {
        LocalAppDataPath.SetPortfolioDemoMode(
            _preferences.ExperienceMode == V8ExperienceMode.PortfolioDemo);
        _snapshot = WorkspaceSnapshot.Load();
        WorkspaceCatalog.Validate(MainWindow.V8RouteIds);
        InitializeComponent();

        Title = $"LifeOS Desktop {ProductVersion.Display}";
        AboutVersionText.Text = Title;

        ProfileButton.Content = "CS \u25BE";
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
            HouseholdNav,
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
        if (_activeModuleRoute is not null)
        {
            CloseEmbeddedModule(restoreScroll: false);
        }

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
            .Select(module => $"{module.Title} - {module.Status}")
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

        if (e.Key == Key.Escape && _activeModuleRoute is not null)
        {
            CloseEmbeddedModule(restoreScroll: true);
            e.Handled = true;
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
        RefreshCommandResults();
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
        if (e.Key is Key.Down or Key.Up)
        {
            int count = CommandResultsListBox.Items.Count;
            if (count > 0)
            {
                int current = CommandResultsListBox.SelectedIndex;
                CommandResultsListBox.SelectedIndex = e.Key == Key.Down
                    ? Math.Min(current + 1, count - 1)
                    : Math.Max(current - 1, 0);
                CommandResultsListBox.ScrollIntoView(CommandResultsListBox.SelectedItem);
            }
            e.Handled = true;
            return;
        }

        if (e.Key != Key.Enter)
        {
            return;
        }

        if (CommandResultsListBox.SelectedItem is ShellSearchResultView selected)
        {
            ExecuteCommandResult(selected.Result);
        }
        else
        {
            CommandStatusText.Text = "No matching workspace, module or safe display command. Nothing ran.";
        }
        e.Handled = true;
    }

    private void CommandTextBox_TextChanged(object sender, TextChangedEventArgs e) =>
        RefreshCommandResults();

    private void CommandResultsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (CommandResultsListBox.SelectedItem is ShellSearchResultView selected)
            ExecuteCommandResult(selected.Result);
    }

    private void RefreshCommandResults()
    {
        if (CommandResultsListBox is null || CommandStatusText is null)
            return;

        IReadOnlyList<ShellSearchResultView> results = ShellSearchService
            .Search(CommandTextBox?.Text, BuildCommandCandidates(), maximumResults: 8)
            .Select(result => new ShellSearchResultView(
                result,
                result.Candidate.Label,
                $"{result.Candidate.Kind} • {result.MatchReason} • {result.Candidate.Description}"))
            .ToArray();
        CommandResultsListBox.ItemsSource = results;
        CommandResultsListBox.SelectedIndex = results.Count > 0 ? 0 : -1;
        CommandStatusText.Text = results.Count == 0
            ? "No matches. Nothing will run until you choose a listed result."
            : string.IsNullOrWhiteSpace(CommandTextBox?.Text)
                ? "Browse indexed shell destinations. No personal record content is indexed."
                : $"{results.Count} match{(results.Count == 1 ? string.Empty : "es")}. No action runs before selection.";
    }

    private static IReadOnlyList<ShellSearchCandidate> BuildCommandCandidates()
    {
        List<ShellSearchCandidate> candidates = [];
        foreach (string workspaceName in WorkspaceOrder)
        {
            WorkspaceDefinition workspace = WorkspaceCatalog.Get(workspaceName);
            candidates.Add(new ShellSearchCandidate(
                $"workspace-{workspaceName.ToLowerInvariant()}",
                workspaceName,
                workspace.Description,
                ShellSearchTargetKind.Workspace,
                Workspace: workspaceName,
                Keywords: [workspace.Subtitle, workspace.Eyebrow]));

            foreach (WorkspaceSectionDefinition section in workspace.Sections)
            {
                foreach (WorkspaceModuleDefinition module in section.Modules.Where(module => module.CanOpen))
                {
                    candidates.Add(new ShellSearchCandidate(
                        $"module-{module.Id}",
                        module.Title,
                        module.Description,
                        ShellSearchTargetKind.Module,
                        Workspace: workspaceName,
                        RouteId: module.RouteId,
                        Keywords: [module.Id, module.Status, section.Title]));
                }
            }
        }

        foreach ((string command, string description, string[] keywords) in SafeDisplayCommands())
        {
            candidates.Add(new ShellSearchCandidate(
                $"preference-{command.Replace(' ', '-').ToLowerInvariant()}",
                command,
                description,
                ShellSearchTargetKind.Preference,
                CommandText: command,
                Keywords: keywords));
        }
        return candidates;
    }

    private static IReadOnlyList<(string Command, string Description, string[] Keywords)> SafeDisplayCommands() =>
    [
        ("Theme light", "Apply the light appearance.", ["appearance", "colour"]),
        ("Theme dark", "Apply the dark appearance.", ["appearance", "colour"]),
        ("Theme system", "Follow the system appearance.", ["appearance", "automatic"]),
        ("Theme high contrast", "Apply the high-contrast appearance.", ["accessibility", "contrast"]),
        ("Accent purple", "Use the purple accent.", ["appearance", "colour"]),
        ("Accent blue", "Use the blue accent.", ["appearance", "colour"]),
        ("Accent teal", "Use the teal accent.", ["appearance", "colour"]),
        ("Density compact", "Use compact workspace spacing.", ["appearance", "spacing"]),
        ("Density comfortable", "Use comfortable workspace spacing.", ["appearance", "spacing"])
    ];

    private void ExecuteCommandResult(ShellSearchResult result)
    {
        ShellSearchCandidate candidate = result.Candidate;
        switch (candidate.Kind)
        {
            case ShellSearchTargetKind.Workspace when candidate.Workspace is not null:
                CloseCommand();
                NavigateTo(candidate.Workspace);
                break;
            case ShellSearchTargetKind.Module when
                candidate.Workspace is not null && candidate.RouteId is not null:
                CloseCommand();
                NavigateTo(candidate.Workspace);
                OpenModule(candidate.RouteId);
                break;
            case ShellSearchTargetKind.Preference when candidate.CommandText is not null:
                if (TryApplyPreferenceCommand(candidate.CommandText))
                    CloseCommand();
                break;
            default:
                CommandStatusText.Text = "That result is incomplete and was not run.";
                break;
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

        OpenModule(routeId);
    }

    private void OpenModule(string routeId)
    {
        if (routeId.StartsWith("workspace:", StringComparison.OrdinalIgnoreCase))
        {
            NavigateTo(routeId["workspace:".Length..]);
            return;
        }

        if (string.Equals(routeId, "integration-inbox", StringComparison.OrdinalIgnoreCase))
        {
            OpenIntegrationInbox();
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

        WorkspaceModuleDefinition? module = WorkspaceCatalog.Get(_activeWorkspace)
            .Sections
            .SelectMany(section => section.Modules)
            .FirstOrDefault(candidate => string.Equals(
                candidate.RouteId,
                routeId,
                StringComparison.OrdinalIgnoreCase));

        string title = module?.Title ?? routeId;
        string subtitle = module?.Description ??
            "This module remains inside its parent LifeOS workspace.";

        if (ProofOnlyRoutes.Contains(routeId) &&
            _preferences.ExperienceMode != V8ExperienceMode.PortfolioDemo)
        {
            ShowEmbeddedModule(
                routeId,
                title,
                subtitle,
                new PortfolioDemoBoundaryView(title, () =>
                {
                    CloseEmbeddedModule(restoreScroll: false);
                    NavigateTo("Settings");
                }));
            return;
        }

        if (string.Equals(routeId, "v11-document-intake", StringComparison.OrdinalIgnoreCase))
        {
            ShowEmbeddedModule(routeId, title, subtitle, new DocumentIntakeV11View());
            return;
        }

        if (string.Equals(routeId, "v11-money-foundation", StringComparison.OrdinalIgnoreCase))
        {
            ShowEmbeddedModule(routeId, title, subtitle, new MoneyV11View());
            return;
        }

        if (string.Equals(routeId, "v12-career-studio", StringComparison.OrdinalIgnoreCase))
        {
            ShowEmbeddedModule(routeId, title, subtitle, new CareerStudioView());
            return;
        }
        if (string.Equals(routeId, "career-cvs", StringComparison.OrdinalIgnoreCase))
        {
            ShowEmbeddedModule(
                routeId,
                title,
                subtitle,
                new CareerDocumentsStudioView(
                    _preferences.ExperienceMode == V8ExperienceMode.PortfolioDemo,
                    () =>
                    CloseEmbeddedModule(restoreScroll: true)));
            return;
        }
        if (string.Equals(routeId, "career-applications", StringComparison.OrdinalIgnoreCase))
        {
            ShowEmbeddedModule(
                routeId,
                title,
                subtitle,
                new CareerApplicationWorkspaceView(
                    _preferences.ExperienceMode == V8ExperienceMode.PortfolioDemo));
            return;
        }
        if (string.Equals(routeId, "local-data-recovery", StringComparison.OrdinalIgnoreCase))
        {
            ShowEmbeddedModule(
                routeId,
                title,
                subtitle,
                new LocalDataRecoveryView(
                    _preferences.ExperienceMode == V8ExperienceMode.PortfolioDemo));
            return;
        }
        if (string.Equals(routeId, "projects", StringComparison.OrdinalIgnoreCase))
        {
            ShowEmbeddedModule(
                routeId,
                title,
                subtitle,
                new ProjectsWorkspaceView(
                    _preferences.ExperienceMode == V8ExperienceMode.PortfolioDemo));
            return;
        }
        if (string.Equals(routeId, "work-time", StringComparison.OrdinalIgnoreCase))
        {
            ShowEmbeddedModule(
                routeId,
                title,
                subtitle,
                new WorkTimeWorkspaceView(
                    _preferences.ExperienceMode == V8ExperienceMode.PortfolioDemo));
            return;
        }
        if (string.Equals(routeId, "v13-grocery-planning", StringComparison.OrdinalIgnoreCase))
        {
            ShowEmbeddedModule(routeId, title, subtitle, new GroceryPlanningView());
            return;
        }

        MainWindow legacyModule = new()
        {
            Title = $"LifeOS — {title}"
        };
        legacyModule.OpenV8ModuleWindow(routeId);

        if (legacyModule.Content is not UIElement content)
        {
            legacyModule.Close();
            MessageBox.Show(
                "The selected module could not be hosted inside this workspace.",
                "LifeOS module host",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        legacyModule.Content = null;
        _embeddedLegacyModuleWindow = legacyModule;
        ShowEmbeddedModule(routeId, title, subtitle, content);
    }

    private void ShowEmbeddedModule(
        string routeId,
        string title,
        string subtitle,
        UIElement content)
    {
        _workspaceScrollOffset = WorkspaceScrollViewer.VerticalOffset;
        _activeModuleRoute = routeId;
        bool immersiveCareerDocuments =
            string.Equals(
                routeId,
                "career-cvs",
                StringComparison.OrdinalIgnoreCase) ||
            string.Equals(
                routeId,
                "career-applications",
                StringComparison.OrdinalIgnoreCase);

        ModuleBackButton.Content = $"← Back to {_activeWorkspace}";
        ModuleHostTitle.Text = title;
        ModuleHostSubtitle.Text = subtitle;
        ModuleHostContent.Content = content;
        ModuleHostRoot.Margin = immersiveCareerDocuments
            ? new Thickness(0)
            : new Thickness(24);
        ModuleBackButton.Visibility = immersiveCareerDocuments ? Visibility.Collapsed : Visibility.Visible;
        ModuleHostTitle.Visibility = immersiveCareerDocuments ? Visibility.Collapsed : Visibility.Visible;
        ModuleHostSubtitle.Visibility = immersiveCareerDocuments ? Visibility.Collapsed : Visibility.Visible;
        WorkspaceScrollViewer.VerticalScrollBarVisibility =
            immersiveCareerDocuments ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Auto;

        WorkspaceRoot.Visibility = Visibility.Collapsed;
        ModuleHostRoot.Visibility = Visibility.Visible;
        Dispatcher.BeginInvoke(
            DispatcherPriority.Loaded,
            new Action(() =>
            {
                WorkspaceScrollViewer.ScrollToTop();
                if (immersiveCareerDocuments)
                {
                    ModuleHostContent.Height = Math.Max(
                        680,
                        WorkspaceScrollViewer.ViewportHeight);
                }
            }));
    }

    private void ModuleBackButton_Click(object sender, RoutedEventArgs e) =>
        CloseEmbeddedModule(restoreScroll: true);

    private void CloseEmbeddedModule(bool restoreScroll)
    {
        if (_activeModuleRoute is null)
        {
            return;
        }

        ModuleHostContent.Content = null;
        ModuleHostContent.Height = double.NaN;
        ModuleHostRoot.Visibility = Visibility.Collapsed;
        ModuleHostRoot.Margin = new Thickness(24);
        ModuleBackButton.Visibility = Visibility.Visible;
        ModuleHostTitle.Visibility = Visibility.Visible;
        ModuleHostSubtitle.Visibility = Visibility.Visible;
        WorkspaceScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        WorkspaceRoot.Visibility = Visibility.Visible;

        _embeddedLegacyModuleWindow?.Close();
        _embeddedLegacyModuleWindow = null;
        _activeModuleRoute = null;

        if (restoreScroll)
        {
            Dispatcher.BeginInvoke(
                DispatcherPriority.Loaded,
                new Action(() => WorkspaceScrollViewer.ScrollToVerticalOffset(
                    _workspaceScrollOffset)));
        }
        else
        {
            WorkspaceScrollViewer.ScrollToTop();
        }
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
        ExperienceModeComboBox.ItemsSource = Enum.GetValues<V8ExperienceMode>();
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
        ExperienceModeComboBox.SelectedItem = _preferences.ExperienceMode;
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
}

internal sealed record ShellSearchResultView(
    ShellSearchResult Result,
    string Title,
    string Meta)
{
    public override string ToString() => Title;
}

    private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        ClearSettingsFeedback();
        FormValidationResult validation = V8SettingsFormValidator.Validate(new(
            ProfileNameTextBox.Text,
            ActiveContextTextBox.Text));
        if (!validation.IsValid)
        {
            ShowFieldIssues(ProfileNameErrorText, validation.ForField("profile-name"));
            ShowFieldIssues(ActiveContextErrorText, validation.ForField("active-context"));
            ShowSettingsProblem(new UserFacingProblem(
                "settings-validation-failed",
                "Review the highlighted settings",
                "The settings were not saved because one or more display fields are invalid.",
                "Correct the highlighted fields, then choose Save Settings again.",
                true));
            return;
        }

        V8Preferences candidate = new V8Preferences
        {
            Theme = ThemeComboBox.SelectedItem is V8Theme theme
            ? theme
            : V8Theme.Dark,
            Accent = AccentComboBox.SelectedItem is V8Accent accent
            ? accent
            : V8Accent.Purple,
            Density = DensityComboBox.SelectedItem is V8Density density
            ? density
            : V8Density.Comfortable,
            StartupMode = StartupComboBox.SelectedItem is V8StartupMode startup
            ? startup
            : V8StartupMode.Home,
            ExperienceMode = ExperienceModeComboBox.SelectedItem is V8ExperienceMode experienceMode
            ? experienceMode
            : V8ExperienceMode.Ordinary,
            EmergencyStopState = _preferences.EmergencyStopState,
            TextScale = IndexToTextScale(TextScaleComboBox.SelectedIndex),
            ReducedMotion = ReducedMotionCheckBox.IsChecked == true,
            ContextPanelAutoOpen = ContextAutoOpenCheckBox.IsChecked == true,
            ProfileName = ProfileNameTextBox.Text,
            ActiveContext = ActiveContextTextBox.Text,
            LastWorkspace = _activeWorkspace,
            ContextPanelOpen = _contextOpen
        }.Normalize();

        try
        {
            V8PreferenceStore.Save(candidate);
            LocalAppDataPath.SetPortfolioDemoMode(
                candidate.ExperienceMode == V8ExperienceMode.PortfolioDemo);
            _preferences = candidate;
            _snapshot = WorkspaceSnapshot.Load();
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
            ShowSettingsProblem(UserFacingProblemFactory.FromException(
                exception,
                "save settings"));
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

        ClearSettingsFeedback();
        V8Preferences reset = new V8Preferences().Normalize();
        try
        {
            V8PreferenceStore.Save(reset);
            _preferences = reset;
            LocalAppDataPath.SetPortfolioDemoMode(false);
            _snapshot = WorkspaceSnapshot.Load();
            ApplyPreferencesToUi();
            SetContextOpen(false, persist: false, returnFocusOnClose: false);
            NavigateTo("Home", persist: false);
            SettingsSaveStatusText.Text = "Approved defaults restored. Module records were not changed.";
        }
        catch (Exception exception) when (
            exception is System.IO.IOException or
            UnauthorizedAccessException)
        {
            ShowSettingsProblem(UserFacingProblemFactory.FromException(
                exception,
                "restore approved settings defaults"));
        }
    }

    private void ClearSettingsFeedback()
    {
        SettingsSaveStatusText.Text = string.Empty;
        SettingsProblemPanel.Visibility = Visibility.Collapsed;
        ProfileNameErrorText.Visibility = Visibility.Collapsed;
        ActiveContextErrorText.Visibility = Visibility.Collapsed;
        ProfileNameErrorText.Text = string.Empty;
        ActiveContextErrorText.Text = string.Empty;
    }

    private static void ShowFieldIssues(
        TextBlock target,
        IReadOnlyList<FormFieldIssue> issues)
    {
        target.Text = string.Join(" ", issues.Select(issue => issue.Message));
        target.Visibility = issues.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
    }

    private void ShowSettingsProblem(UserFacingProblem problem)
    {
        SettingsProblemTitleText.Text = $"{problem.Title} ({problem.Code})";
        SettingsProblemDetailText.Text = problem.Detail;
        SettingsProblemRecoveryText.Text = $"Next: {problem.RecoveryAction}";
        SettingsProblemPanel.Visibility = Visibility.Visible;
        SettingsSaveStatusText.Text = "Not saved.";
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
