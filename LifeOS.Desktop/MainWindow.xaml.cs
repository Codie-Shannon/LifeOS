using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LifeOS.Shared.Shell;
using LifeOS.Shared.Money;

namespace LifeOS.Desktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        ShowCommandCentre();
    }

    private void CommandCentreNavButton_Click(object sender, RoutedEventArgs e) => ShowCommandCentre();

    private void MoneyPressureNavButton_Click(object sender, RoutedEventArgs e) => ShowMoneyPressurePage();

    private void AgendaNavButton_Click(object sender, RoutedEventArgs e) => ShowModulePage(LifeOSModuleKind.Agenda);

    private void FollowUpsNavButton_Click(object sender, RoutedEventArgs e) => ShowModulePage(LifeOSModuleKind.FollowUps);

    private void ProjectsNavButton_Click(object sender, RoutedEventArgs e) => ShowModulePage(LifeOSModuleKind.Projects);

    private void TimerAgentNavButton_Click(object sender, RoutedEventArgs e) => ShowModulePage(LifeOSModuleKind.TimerAgent);

    private void SettingsNavButton_Click(object sender, RoutedEventArgs e) => ShowModulePage(LifeOSModuleKind.Settings);

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
        var summary = MoneyPressureDemoData.CreateSummary();

        SetHeader(module.Title, $"{module.Title} • {module.Badge}");

        var root = new StackPanel();

        root.Children.Add(CreateHeroPanel(
            "Money Pressure",
            "First real shared LifeOS module foundation. This page uses shared Core calculations with demo/manual-style data."));

        var metricsPanel = new WrapPanel
        {
            Margin = new Thickness(0, 22, 0, 0)
        };

        metricsPanel.Children.Add(CreateDashboardCard("Safe to spend", FormatMoney(summary.SafeToSpend), summary.PressureLabel));
        metricsPanel.Children.Add(CreateDashboardCard("Current balance", FormatMoney(summary.CurrentBalance), "Manual entry"));
        metricsPanel.Children.Add(CreateDashboardCard("Paid income", FormatMoney(summary.ConfirmedPaidIncome), "Counted as safe"));
        metricsPanel.Children.Add(CreateDashboardCard("Pending income", FormatMoney(summary.PendingIncome), "Not safe yet"));
        metricsPanel.Children.Add(CreateDashboardCard("Bills due", FormatMoney(summary.BillsDue), "This week"));
        metricsPanel.Children.Add(CreateDashboardCard("Deductions", FormatMoney(summary.DeductionsDue), "Active"));

        root.Children.Add(metricsPanel);

        var reasonsText = string.Join(Environment.NewLine, summary.Reasons.Select(reason => $"• {reason}"));

        var reasonsPanel = CreateInfoPanel("Why this week has pressure", reasonsText);
        reasonsPanel.Margin = new Thickness(0, 8, 0, 0);
        root.Children.Add(reasonsPanel);

        var guardrailPanel = CreateInfoPanel(
            "Phase 8 scope",
            "This is demo/manual-style data only. No bank sync, invoices, database, mobile app, or full money module yet. The goal is proving the shared safe-to-spend calculation path.");

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

