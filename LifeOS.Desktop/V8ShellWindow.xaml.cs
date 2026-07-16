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
        NavigateTo("Home");
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
        _contextOpen = !_contextOpen;
        ContextColumn.Width = _contextOpen ? new GridLength(320) : new GridLength(0);
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
