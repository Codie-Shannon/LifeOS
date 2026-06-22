using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LifeOS.Desktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        ShowCommandCentre();
    }

    private void CommandCentreNavButton_Click(object sender, RoutedEventArgs e) => ShowCommandCentre();
    private void MoneyPressureNavButton_Click(object sender, RoutedEventArgs e) => ShowPlaceholderPage(
        "Money Pressure",
        "Planned shared LifeOS module for safe-to-spend, manual balance, income, bills, pay-later, deductions, and weekly pressure.",
        "This will be shared between desktop and mobile.");

    private void AgendaNavButton_Click(object sender, RoutedEventArgs e) => ShowPlaceholderPage(
        "Agenda",
        "Planned shared LifeOS module for appointments, tasks, payments, follow-ups, fixed/flexible commitments, and date-based pressure.",
        "Desktop will test the full model first. Mobile will receive the optimized daily-use version.");

    private void FollowUpsNavButton_Click(object sender, RoutedEventArgs e) => ShowPlaceholderPage(
        "Follow-Ups",
        "Planned shared LifeOS module for people, organisations, client replies, waiting-on items, expected dates, and money-linked follow-ups.",
        "This module turns mental waiting-on clutter into a clear next-action list.");

    private void ProjectsNavButton_Click(object sender, RoutedEventArgs e) => ShowPlaceholderPage(
        "Projects",
        "Planned shared LifeOS module for active builds, client proofs, portfolio work, shipped versions, and project pressure.",
        "This module will help connect work output to proof, case studies, and business momentum.");

    private void TimerAgentNavButton_Click(object sender, RoutedEventArgs e) => ShowPlaceholderPage(
        "TimerAgent",
        "TimerAgent is already shipped as the first desktop-only LifeOS utility.",
        "It tracks focused work, billable sessions, earned income, tax set-aside, and CSV logs. Future LifeOS versions will read TimerAgent work data into weekly pressure summaries.");

    private void SettingsNavButton_Click(object sender, RoutedEventArgs e) => ShowPlaceholderPage(
        "Settings",
        "Planned shared settings for Light / Dark / System theme, local storage, export and backup, privacy controls, and desktop/mobile preferences.",
        "Theme support belongs at the LifeOS platform layer, not inside one individual module.");

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
}

