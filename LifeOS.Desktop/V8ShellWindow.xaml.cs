using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace LifeOS.Desktop;

public partial class V8ShellWindow : Window
{
    private static readonly string[] WorkspaceOrder =
    {
        "Home", "Work", "Career", "Money", "Life", "Projects", "Assistant", "Settings"
    };

    private string _activeWorkspace = "Home";
    private bool _compactDensity;
    private bool _contextOpen;
    private bool _stopArmed;
    private IInputElement? _focusBeforeCommand;
    private WorkspaceSnapshot _snapshot = WorkspaceSnapshot.Load();

    private bool IsCommandOpen =>
        CommandOverlay.Visibility == Visibility.Visible;

    public V8ShellWindow()
    {
        WorkspaceCatalog.Validate(MainWindow.WorkspaceRouteIds);

        InitializeComponent();

        Loaded += Window_Loaded;
        SizeChanged += Window_SizeChanged;

        NavigateTo("Home");
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        ApplyResponsiveLayout();
        ApplyDensity();
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        ApplyResponsiveLayout();
        ApplyDensity();
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

        bool compactWidth = ActualWidth <= 1120;
        bool veryCompactWidth = ActualWidth <= 1000;
        bool compactHeight = ActualHeight <= 800;

        WorkspaceTitle.FontSize = compactWidth ? 16 : 17;
        WorkspaceSubtitle.Visibility = compactWidth
            ? Visibility.Collapsed
            : Visibility.Visible;

        CommandButton.Width = veryCompactWidth
            ? 190
            : compactWidth
                ? 230
                : 360;
        CommandButton.MaxWidth = CommandButton.Width;

        TextBlock? commandLabel = FindVisualChildren<TextBlock>(CommandButton)
            .FirstOrDefault(text =>
                text.Text.StartsWith("Search", StringComparison.Ordinal));

        if (commandLabel is not null)
        {
            commandLabel.Text = compactWidth
                ? "Search"
                : "Search or run a command";
        }

        TextBlock? commandShortcut = FindVisualChildren<TextBlock>(CommandButton)
            .FirstOrDefault(text =>
                string.Equals(text.Text, "Ctrl+K", StringComparison.Ordinal));

        if (commandShortcut is not null)
        {
            commandShortcut.Visibility = veryCompactWidth
                ? Visibility.Collapsed
                : Visibility.Visible;
            commandShortcut.Margin = compactWidth
                ? new Thickness(8, 0, 0, 0)
                : new Thickness(16, 0, 0, 0);
        }

        if (StopButton.Parent is Panel topBarActions)
        {
            foreach (Button button in topBarActions.Children.OfType<Button>())
            {
                button.MinWidth = 0;
                button.Padding = compactWidth
                    ? new Thickness(8, 6, 8, 6)
                    : new Thickness(12, 8, 12, 8);
                button.Margin = compactWidth
                    ? new Thickness(2, 0, 2, 0)
                    : new Thickness(4, 0, 4, 0);
                button.FontSize = compactWidth ? 12 : 13;
            }
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
            button.HorizontalContentAlignment = HorizontalAlignment.Center;
            button.VerticalContentAlignment = VerticalAlignment.Center;

            TextBlock[] navigationText = FindVisualChildren<TextBlock>(button).ToArray();
            if (navigationText.Length >= 2)
            {
                TextBlock icon = navigationText[0];
                TextBlock label = navigationText[1];

                icon.FontSize = compactHeight ? 18 : 19;
                icon.Margin = compactHeight
                    ? new Thickness(0, 0, 0, 1)
                    : new Thickness(0, 0, 0, 2);
                label.FontSize = compactHeight ? 10 : 11;
                label.Margin = new Thickness(0);
            }
        }

        if (HomeNav.Parent is StackPanel navigationStack)
        {
            navigationStack.Margin = compactHeight
                ? new Thickness(8, 0, 8, 0)
                : new Thickness(8, 8, 8, 12);
        }

        ScrollViewer? railScrollViewer = FindVisualChildren<ScrollViewer>(this)
            .FirstOrDefault(viewer =>
                FindVisualChildren<Button>(viewer)
                    .Any(button => ReferenceEquals(button, HomeNav)));

        if (railScrollViewer is not null)
        {
            railScrollViewer.VerticalScrollBarVisibility = compactHeight
                ? ScrollBarVisibility.Disabled
                : ScrollBarVisibility.Auto;
            railScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
        }

        if (compactWidth && _contextOpen)
        {
            _contextOpen = false;
            ContextColumn.Width = new GridLength(0);
        }
    }

    private void ApplyDensity()
    {
        if (!IsLoaded)
        {
            return;
        }

        bool useCompact = _compactDensity || ActualWidth <= 980;

        WorkspaceRoot.Margin = useCompact
            ? new Thickness(16)
            : new Thickness(24);

        DensityButton.Content = _compactDensity
            ? "Use Comfortable"
            : "Use Compact";

        DensityStatusText.Text = _compactDensity
            ? "Compact density"
            : "Comfortable density";

        foreach (Border border in FindVisualChildren<Border>(WorkspaceRoot))
        {
            if (string.Equals(border.Tag as string, "WorkspaceMetricCard", StringComparison.Ordinal))
            {
                border.Width = useCompact ? 184 : 220;
                border.Padding = useCompact
                    ? new Thickness(12)
                    : new Thickness(16);
                border.Margin = useCompact
                    ? new Thickness(0, 0, 8, 8)
                    : new Thickness(0, 0, 12, 12);
            }
            else if (string.Equals(border.Tag as string, "WorkspaceModuleCard", StringComparison.Ordinal))
            {
                border.Width = useCompact ? 292 : 348;
                border.Padding = useCompact
                    ? new Thickness(14)
                    : new Thickness(18);
                border.Margin = useCompact
                    ? new Thickness(0, 0, 8, 8)
                    : new Thickness(0, 0, 12, 12);
            }
        }
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

    private void NavigateTo(string workspace)
    {
        WorkspaceDefinition definition;

        try
        {
            definition = WorkspaceCatalog.Get(workspace);
        }
        catch (InvalidOperationException)
        {
            return;
        }

        _activeWorkspace = definition.Name;
        _snapshot = WorkspaceSnapshot.Load();

        WorkspaceTitle.Text = definition.Name;
        WorkspaceSubtitle.Text = definition.Subtitle;
        WorkspaceEyebrow.Text = definition.Eyebrow;
        WorkspaceDisplayTitle.Text = definition.DisplayTitle;
        WorkspaceDescription.Text = definition.Description;

        MetricItems.ItemsSource = definition.Metrics
            .Select(metric => new WorkspaceMetricView(
                metric.Label,
                _snapshot.Resolve(metric),
                metric.Detail))
            .ToArray();

        SectionItems.ItemsSource = definition.Sections;

        ContextTitle.Text = $"{definition.Name} context";
        ContextBody.Text = definition.ContextSummary;
        ContextModulesList.ItemsSource = definition.Sections
            .SelectMany(section => section.Modules)
            .Select(module => $"{module.Title} — {module.Status}")
            .ToArray();

        WorkspaceScrollViewer.ScrollToTop();
        UpdateNavigationSelection();

        Dispatcher.BeginInvoke(
            DispatcherPriority.Loaded,
            new Action(ApplyDensity));
    }

    private void UpdateNavigationSelection()
    {
        foreach (Button button in GetNavigationButtons())
        {
            bool selected = string.Equals(
                button.Tag as string,
                _activeWorkspace,
                StringComparison.OrdinalIgnoreCase);

            button.Background = selected
                ? (Brush)FindResource("LifeOS.Brush.AccentSoft")
                : Brushes.Transparent;
            button.BorderBrush = selected
                ? (Brush)FindResource("LifeOS.Brush.Accent")
                : Brushes.Transparent;
        }
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent)
        where T : DependencyObject
    {
        if (parent is null)
        {
            yield break;
        }

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

    private void CommandButton_Click(object sender, RoutedEventArgs e) =>
        OpenCommand();

    private void CloseCommand_Click(object sender, RoutedEventArgs e) =>
        CloseCommand();

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

        if (command.Equals("Theme light", StringComparison.OrdinalIgnoreCase))
        {
            ApplyTheme(light: true);
            CloseCommand();
            e.Handled = true;
            return;
        }

        if (command.Equals("Theme dark", StringComparison.OrdinalIgnoreCase))
        {
            ApplyTheme(light: false);
            CloseCommand();
            e.Handled = true;
            return;
        }

        if (command.Equals("Density compact", StringComparison.OrdinalIgnoreCase))
        {
            SetDensity(compact: true);
            CloseCommand();
            e.Handled = true;
            return;
        }

        if (command.Equals("Density comfortable", StringComparison.OrdinalIgnoreCase))
        {
            SetDensity(compact: false);
            CloseCommand();
            e.Handled = true;
            return;
        }

        if (command.Equals("Legacy", StringComparison.OrdinalIgnoreCase))
        {
            CloseCommand();
            OpenLegacy();
            e.Handled = true;
        }
    }

    private void ApplyTheme(bool light)
    {
        ResourceDictionary theme = new()
        {
            Source = new Uri(
                light
                    ? "Resources/Themes/Theme.Light.xaml"
                    : "Resources/Themes/Theme.Dark.xaml",
                UriKind.Relative)
        };

        var dictionaries =
            Application.Current.Resources.MergedDictionaries;

        if (dictionaries.Count <= 1)
        {
            dictionaries.Add(theme);
        }
        else
        {
            dictionaries[dictionaries.Count - 1] = theme;
        }

        UpdateNavigationSelection();
        ApplyDensity();
    }

    private void DensityButton_Click(object sender, RoutedEventArgs e) =>
        SetDensity(!_compactDensity);

    private void SetDensity(bool compact)
    {
        _compactDensity = compact;
        ApplyDensity();
    }

    private void ContextButton_Click(object sender, RoutedEventArgs e)
    {
        if (ActualWidth <= 1120)
        {
            _contextOpen = false;
            ContextColumn.Width = new GridLength(0);
            return;
        }

        _contextOpen = !_contextOpen;
        ContextColumn.Width = _contextOpen
            ? new GridLength(320)
            : new GridLength(0);
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
            Owner = this
        };

        moduleWindow.OpenWorkspaceModule(routeId);
        moduleWindow.Show();
    }

    private void OpenLegacy()
    {
        MainWindow legacyWindow = new()
        {
            Owner = this
        };

        legacyWindow.Show();
    }

    private void ReviewButton_Click(object sender, RoutedEventArgs e)
    {
        ContextTitle.Text = "Combined review";
        ContextBody.Text =
            "Review items remain collected across Work, Money, Assistant and system safety. Opening a review does not edit the active workspace.";

        if (!_contextOpen)
        {
            ContextButton_Click(sender, e);
        }
    }

    private void StatusButton_Click(object sender, RoutedEventArgs e)
    {
        ContextTitle.Text = "System and sync status";
        ContextBody.Text =
            "Local data remains authoritative. Companion and integration state stay review-first, and no pending transfer mutates a workspace automatically.";

        if (!_contextOpen)
        {
            ContextButton_Click(sender, e);
        }
    }

    private void ProfileButton_Click(object sender, RoutedEventArgs e)
    {
        ContextTitle.Text = "Active context";
        ContextBody.Text =
            "Profile: Codie Shannon. Context: Personal. Workspace assignments do not change the underlying trust, review or evidence boundaries.";

        if (!_contextOpen)
        {
            ContextButton_Click(sender, e);
        }
    }

    private void StopButton_Click(object sender, RoutedEventArgs e)
    {
        _stopArmed = !_stopArmed;
        StopButton.Content = _stopArmed
            ? "STOP ARMED"
            : "Stop";
        StopButton.BorderBrush = (Brush)FindResource(
            _stopArmed
                ? "LifeOS.Brush.Danger"
                : "LifeOS.Brush.Border");
        StopButton.ToolTip = _stopArmed
            ? "Emergency Stop is armed. Press again to return to idle."
            : "Emergency Stop is idle.";
    }
}
