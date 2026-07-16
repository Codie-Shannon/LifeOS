using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LifeOS.Desktop;

public partial class V8ShellWindow : Window
{
    private static readonly string[] WorkspaceOrder =
    {
        "Home", "Work", "Career", "Money", "Life", "Projects", "Assistant", "Settings"
    };

    private readonly Dictionary<string, (string Subtitle, string Description)> _workspaceCopy = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Home"] = ("Balanced operating overview", "Today's pressure, work and money in one calm view."),
        ["Work"] = ("Clients, follow-ups and paid work", "The permanent home for clients, waiting-on items, timesheets, invoices and work records. Existing v7 modules remain available while migration is staged."),
        ["Career"] = ("Career stays separate from Work", "Profile, CVs, applications, interviews and career follow-ups will live here without mixing them into client delivery."),
        ["Money"] = ("Income, bills and weekly control", "Weekly money, expected income, bills, payment timing and pressure signals will be reorganised here."),
        ["Life"] = ("Personal operating systems", "Goals, routines, family, household, road trips and personal administration belong here."),
        ["Projects"] = ("Milestones, evidence and delivery", "Active projects, milestones, proof and later DevOps views will share this workspace."),
        ["Assistant"] = ("Source-backed, review-first help", "The existing assistant, plans, evidence boundaries and explicit memory remain preserved underneath this shell."),
        ["Settings"] = ("System behaviour only", "Appearance, startup, accessibility, profiles, privacy, sync, automation safety and diagnostics will be completed in Group 45.")
    };

    private string _activeWorkspace = "Home";
    private bool _contextOpen;
    private bool _lightTheme;
    private bool _stopArmed;
    private IInputElement? _focusBeforeCommand;

    private bool IsCommandOpen =>
        CommandOverlay.Visibility == Visibility.Visible;

    public V8ShellWindow()
    {
        InitializeComponent();

        Loaded += Window_Loaded;
        SizeChanged += Window_SizeChanged;

        NavigateTo("Home");
    }

    private void Window_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        ApplyResponsiveLayout();
    }

    private void Window_SizeChanged(
        object sender,
        SizeChangedEventArgs e)
    {
        ApplyResponsiveLayout();
    }

    private void ApplyResponsiveLayout()
    {
        if (!IsLoaded) return;

        bool compactWidth = ActualWidth <= 1120;
        bool veryCompactWidth = ActualWidth <= 1000;
        bool compactHeight = ActualHeight <= 800;

        WorkspaceTitle.FontSize =
            compactWidth
                ? 16
                : 17;

        WorkspaceSubtitle.Visibility =
            compactWidth
                ? Visibility.Collapsed
                : Visibility.Visible;

        CommandButton.Width =
            veryCompactWidth
                ? 190
                : compactWidth
                    ? 230
                    : 360;

        CommandButton.MaxWidth = CommandButton.Width;

        TextBlock? commandLabel =
            FindVisualChildren<TextBlock>(CommandButton)
                .FirstOrDefault(
                    text =>
                        text.Text.StartsWith(
                            "Search",
                            StringComparison.Ordinal));

        if (commandLabel is not null)
        {
            commandLabel.Text =
                compactWidth
                    ? "Search"
                    : "Search or run a command";
        }

        TextBlock? commandShortcut =
            FindVisualChildren<TextBlock>(CommandButton)
                .FirstOrDefault(
                    text =>
                        string.Equals(
                            text.Text,
                            "Ctrl+K",
                            StringComparison.Ordinal));

        if (commandShortcut is not null)
        {
            commandShortcut.Visibility =
                veryCompactWidth
                    ? Visibility.Collapsed
                    : Visibility.Visible;

            commandShortcut.Margin =
                compactWidth
                    ? new Thickness(8, 0, 0, 0)
                    : new Thickness(16, 0, 0, 0);
        }

        if (StopButton.Parent is Panel topBarActions)
        {
            foreach (Button button in
                     topBarActions.Children.OfType<Button>())
            {
                button.MinWidth = 0;

                button.Padding =
                    compactWidth
                        ? new Thickness(8, 6, 8, 6)
                        : new Thickness(12, 8, 12, 8);

                button.Margin =
                    compactWidth
                        ? new Thickness(2, 0, 2, 0)
                        : new Thickness(4, 0, 4, 0);

                button.FontSize =
                    compactWidth
                        ? 12
                        : 13;
            }
        }

        Button[] navigationButtons =
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

        foreach (Button button in navigationButtons)
        {
            button.Height =
                compactHeight
                    ? 56
                    : 62;

            button.Margin =
                compactHeight
                    ? new Thickness(0, 1, 0, 1)
                    : new Thickness(0, 3, 0, 3);

            button.Padding =
                compactHeight
                    ? new Thickness(0, 2, 0, 2)
                    : new Thickness(0);

            button.HorizontalContentAlignment =
                HorizontalAlignment.Center;

            button.VerticalContentAlignment =
                VerticalAlignment.Center;

            TextBlock[] navigationText =
                FindVisualChildren<TextBlock>(button)
                    .ToArray();

            if (navigationText.Length >= 2)
            {
                TextBlock icon = navigationText[0];
                TextBlock label = navigationText[1];

                icon.FontSize =
                    compactHeight
                        ? 18
                        : 19;

                icon.Margin =
                    compactHeight
                        ? new Thickness(0, 0, 0, 1)
                        : new Thickness(0, 0, 0, 2);

                label.FontSize =
                    compactHeight
                        ? 10
                        : 11;

                label.Margin = new Thickness(0);
            }
        }

        if (HomeNav.Parent is StackPanel navigationStack)
        {
            navigationStack.Margin =
                compactHeight
                    ? new Thickness(8, 0, 8, 0)
                    : new Thickness(8, 8, 8, 12);
        }

        ScrollViewer? railScrollViewer =
            FindVisualChildren<ScrollViewer>(this)
                .FirstOrDefault(
                    viewer =>
                        FindVisualChildren<Button>(viewer)
                            .Any(
                                button =>
                                    ReferenceEquals(
                                        button,
                                        HomeNav)));

        if (railScrollViewer is not null)
        {
            railScrollViewer.VerticalScrollBarVisibility =
                compactHeight
                    ? ScrollBarVisibility.Disabled
                    : ScrollBarVisibility.Auto;

            railScrollViewer.HorizontalScrollBarVisibility =
                ScrollBarVisibility.Disabled;
        }

        if (HomeWorkspace.Parent is FrameworkElement workspaceHost)
        {
            workspaceHost.Margin =
                compactWidth
                    ? new Thickness(14)
                    : new Thickness(24);
        }

        if (HomeWorkspace.ColumnDefinitions.Count >= 2)
        {
            HomeWorkspace.ColumnDefinitions[0].Width =
                new GridLength(
                    compactWidth ? 1.65 : 2,
                    GridUnitType.Star);

            HomeWorkspace.ColumnDefinitions[1].Width =
                new GridLength(
                    1,
                    GridUnitType.Star);
        }

        Grid? primaryColumn =
            HomeWorkspace.Children
                .OfType<Grid>()
                .FirstOrDefault(
                    element =>
                        Grid.GetRow(element) == 1 &&
                        Grid.GetColumn(element) == 0);

        if (primaryColumn is not null)
        {
            primaryColumn.Margin =
                compactWidth
                    ? new Thickness(0, 0, 7, 0)
                    : new Thickness(0, 0, 12, 0);
        }

        StackPanel? secondaryColumn =
            HomeWorkspace.Children
                .OfType<StackPanel>()
                .FirstOrDefault(
                    element =>
                        Grid.GetRow(element) == 1 &&
                        Grid.GetColumn(element) == 1);

        if (secondaryColumn is not null)
        {
            secondaryColumn.Margin =
                compactWidth
                    ? new Thickness(7, 0, 0, 0)
                    : new Thickness(12, 0, 0, 0);
        }

        foreach (TextBlock textBlock in
                 FindVisualChildren<TextBlock>(HomeWorkspace))
        {
            textBlock.TextWrapping = TextWrapping.Wrap;
        }

        if (compactWidth && _contextOpen)
        {
            _contextOpen = false;
            ContextColumn.Width = new GridLength(0);
        }
    }
    private void WorkspaceNav_Click(object sender, RoutedEventArgs e)
    {
        if (IsCommandOpen) return;

        if (sender is Button { Tag: string workspace })
        {
            NavigateTo(workspace);
        }
    }

    private void NavigateTo(string workspace)
    {
        if (!_workspaceCopy.TryGetValue(workspace, out var copy)) return;
        _activeWorkspace = workspace;
        WorkspaceTitle.Text = workspace;
        WorkspaceSubtitle.Text = copy.Subtitle;
        ContextTitle.Text = $"{workspace} context";
        ContextBody.Text = copy.Description;
        HomeWorkspace.Visibility = workspace == "Home" ? Visibility.Visible : Visibility.Collapsed;
        PlaceholderWorkspace.Visibility = workspace == "Home" ? Visibility.Collapsed : Visibility.Visible;
        PlaceholderTitle.Text = workspace;
        PlaceholderDescription.Text = copy.Description;
        PlaceholderEyebrow.Text = workspace == "Settings" ? "GROUP 45 COMPLETION BOUNDARY" : "GROUP 43 WORKSPACE FOUNDATION";
        UpdateNavigationSelection();
    }

    private void UpdateNavigationSelection()
    {
        foreach (var button in FindVisualChildren<Button>(this).Where(b => b.Tag is string))
        {
            bool selected = string.Equals(button.Tag as string, _activeWorkspace, StringComparison.OrdinalIgnoreCase);
            button.Background = selected ? (System.Windows.Media.Brush)FindResource("LifeOS.Brush.AccentSoft") : System.Windows.Media.Brushes.Transparent;
            button.BorderBrush = selected ? (System.Windows.Media.Brush)FindResource("LifeOS.Brush.Accent") : System.Windows.Media.Brushes.Transparent;
        }
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
    {
        if (parent is null) yield break;
        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            if (child is T typed) yield return typed;
            foreach (T descendant in FindVisualChildren<T>(child)) yield return descendant;
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
                return;
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
        if (!IsCommandOpen) return;

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
        if (e.Key != Key.Enter) return;

        string command = CommandTextBox.Text.Trim();

        string? workspace = WorkspaceOrder.FirstOrDefault(
            candidate => string.Equals(
                candidate,
                command,
                StringComparison.OrdinalIgnoreCase));

        if (workspace is not null)
        {
            CloseCommand();
            NavigateTo(workspace);
            e.Handled = true;
            return;
        }

        if (command.Equals("Theme light", StringComparison.OrdinalIgnoreCase))
        {
            ApplyTheme(true);
            CloseCommand();
            e.Handled = true;
            return;
        }

        if (command.Equals("Theme dark", StringComparison.OrdinalIgnoreCase))
        {
            ApplyTheme(false);
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
        _lightTheme = light;
        ResourceDictionary theme = new() { Source = new Uri(light ? "Resources/Themes/Theme.Light.xaml" : "Resources/Themes/Theme.Dark.xaml", UriKind.Relative) };
        var dictionaries = Application.Current.Resources.MergedDictionaries;
        if (dictionaries.Count == 0) dictionaries.Add(theme);
        else if (dictionaries.Count == 1) dictionaries.Add(theme);
        else dictionaries[dictionaries.Count - 1] = theme;
        UpdateNavigationSelection();
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

        ContextColumn.Width =
            _contextOpen
                ? new GridLength(320)
                : new GridLength(0);
    }

    private void OpenLegacy_Click(object sender, RoutedEventArgs e) => OpenLegacy();

    private static void OpenLegacy()
    {
        MainWindow legacyWindow = new() { Owner = Application.Current.MainWindow };
        legacyWindow.Show();
    }

    private void OpenWork_Click(object sender, RoutedEventArgs e) => NavigateTo("Work");
    private void OpenMoney_Click(object sender, RoutedEventArgs e) => NavigateTo("Money");
    private void ReviewButton_Click(object sender, RoutedEventArgs e) { ContextTitle.Text = "Combined review"; ContextBody.Text = "Four review items are collected across work, money, assistant and system safety."; if (!_contextOpen) ContextButton_Click(sender, e); }
    private void StatusButton_Click(object sender, RoutedEventArgs e) { ContextTitle.Text = "System and sync status"; ContextBody.Text = "Local data is healthy. Companion sync is connected. No pending transfer requires intervention."; if (!_contextOpen) ContextButton_Click(sender, e); }
    private void ProfileButton_Click(object sender, RoutedEventArgs e) { ContextTitle.Text = "Active context"; ContextBody.Text = "Profile: Codie Shannon. Context: Personal. Local-first and review-first boundaries remain active."; if (!_contextOpen) ContextButton_Click(sender, e); }

    private void StopButton_Click(object sender, RoutedEventArgs e)
    {
        _stopArmed = !_stopArmed;
        StopButton.Content = _stopArmed ? "STOP ARMED" : "Stop";
        StopButton.BorderBrush = (System.Windows.Media.Brush)FindResource(_stopArmed ? "LifeOS.Brush.Danger" : "LifeOS.Brush.Border");
        StopButton.ToolTip = _stopArmed ? "Emergency Stop is armed. Press again to return to idle." : "Emergency Stop is idle.";
    }
}
