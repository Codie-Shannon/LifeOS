using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LifeOS.Shared.Shell;

namespace LifeOS.Desktop;

public partial class MainWindow
{
    private static readonly string[] FinalV8RouteIds =
    {
        "agenda",
        "assistant",
        "automation-centre",
        "bills-payments",
        "command-centre",
        "daily-operating-flow",
        "daily-state",
        "desktop-release",
        "email-radar",
        "evidence-vault",
        "final-offline-os",
        "follow-ups",
        "integration-inbox",
        "item-state-engine",
        "lifeos-spine",
        "memory",
        "money-pressure",
        "money-profile",
        "money-timeline",
        "os-navigation",
        "paid-work-centre",
        "pay-later",
        "payment-calendar",
        "projects",
        "proof-tracker",
        "receipt-evidence",
        "relationship-radar",
        "search-knowledge",
        "settings-safety",
        "timer-agent",
        "timesheet-evidence",
        "universal-spine",
        "v11-money-foundation",
        "weekly-close-out",
        "work-pipeline",
        "work-sessions"
    };

    public static IReadOnlyCollection<string> V8RouteIds => FinalV8RouteIds;

    public void OpenV8ModuleWindow(string routeId)
    {
        OpenV8Module(routeId);
        DetachFromCurrentParent(MainContentControl);

        Grid root = new();
        root.SetResourceReference(Panel.BackgroundProperty, "LifeOS.Brush.Window");
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        Border header = new()
        {
            Padding = new Thickness(24, 18, 24, 18),
            BorderThickness = new Thickness(0, 0, 0, 1)
        };
        header.SetResourceReference(Border.BackgroundProperty, "LifeOS.Brush.TopBar");
        header.SetResourceReference(Border.BorderBrushProperty, "LifeOS.Brush.Border");

        StackPanel headerText = new();
        TextBlock title = new()
        {
            Text = FormatRouteTitle(routeId),
            FontSize = 24,
            FontWeight = FontWeights.SemiBold
        };
        title.SetResourceReference(TextBlock.ForegroundProperty, "LifeOS.Brush.TextPrimary");

        TextBlock subtitle = new()
        {
            Text = "Canonical LifeOS module â€” opened from the permanent v8 workspace",
            Margin = new Thickness(0, 4, 0, 0),
            FontSize = 13
        };
        subtitle.SetResourceReference(TextBlock.ForegroundProperty, "LifeOS.Brush.TextSecondary");

        headerText.Children.Add(title);
        headerText.Children.Add(subtitle);
        header.Child = headerText;
        root.Children.Add(header);

        Grid.SetRow(MainContentControl, 1);
        root.Children.Add(MainContentControl);

        Content = root;
        Title = $"LifeOS â€” {FormatRouteTitle(routeId)}";
        Width = 1500;
        Height = 860;
        MinWidth = 900;
        MinHeight = 650;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
    }

    private static void DetachFromCurrentParent(FrameworkElement element)
    {
        switch (element.Parent)
        {
            case Panel panel:
                panel.Children.Remove(element);
                break;
            case Decorator decorator:
                decorator.Child = null;
                break;
            case ContentControl contentControl:
                contentControl.Content = null;
                break;
            case null:
                break;
            default:
                throw new InvalidOperationException(
                    $"Cannot detach {element.Name} from {element.Parent.GetType().Name}.");
        }
    }

    private static string FormatRouteTitle(string routeId)
    {
        TextInfo textInfo = CultureInfo.InvariantCulture.TextInfo;
        return textInfo.ToTitleCase(routeId.Replace('-', ' '));
    }

    public void OpenV8Module(string routeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(routeId);

        switch (routeId)
        {
            case "agenda":
                ShowAgendaPage();
                break;
            case "assistant":
                ShowAssistantPage();
                break;
            case "automation-centre":
                ShowAutomationCentrePage();
                break;
            case "bills-payments":
                ShowBillsPaymentsPage();
                break;
            case "command-centre":
                ShowCommandCentre();
                break;
            case "daily-operating-flow":
                ShowDailyOperatingFlowPage();
                break;
            case "daily-state":
                ShowDailyStatePage();
                break;
            case "desktop-release":
                ShowDesktopReleasePage();
                break;
            case "email-radar":
                ShowEmailRadarPage();
                break;
            case "evidence-vault":
                ShowEvidenceVaultPage();
                break;
            case "final-offline-os":
                ShowFinalOfflineOsPage();
                break;
            case "follow-ups":
                ShowFollowUpsPage();
                break;
            case "integration-inbox":
                ShowIntegrationInboxPage();
                break;
            case "item-state-engine":
                ShowItemStateEnginePage();
                break;
            case "lifeos-spine":
                ShowLifeOsSpinePage();
                break;
            case "memory":
                ShowMemoryPage();
                break;
            case "money-pressure":
                ShowMoneyPressurePage();
                break;
            case "money-profile":
                ShowMoneyProfilePage();
                break;
            case "money-timeline":
                ShowMoneyTimelinePage();
                break;
            case "os-navigation":
                ShowOsNavigationPage();
                break;
            case "paid-work-centre":
                ShowPaidWorkCentrePage();
                break;
            case "pay-later":
                ShowPayLaterPage();
                break;
            case "payment-calendar":
                ShowPaymentCalendarPage();
                break;
            case "projects":
                ShowModulePage(LifeOSModuleKind.Projects);
                break;
            case "proof-tracker":
                ShowProofTrackerPage();
                break;
            case "receipt-evidence":
                ShowReceiptEvidencePage();
                break;
            case "relationship-radar":
                ShowRelationshipRadarPage();
                break;
            case "search-knowledge":
                ShowSearchKnowledgePage();
                break;
            case "settings-safety":
                ShowSettingsSafetyThemePage();
                break;
            case "timer-agent":
                ShowModulePage(LifeOSModuleKind.TimerAgent);
                break;
            case "timesheet-evidence":
                ShowTimesheetEvidencePage();
                break;
            case "universal-spine":
                ShowUniversalSpinePage();
                break;
            case "weekly-close-out":
                ShowWeeklyCloseOutPage();
                break;
            case "work-pipeline":
                ShowWorkPipelinePage();
                break;
            case "work-sessions":
                ShowWorkSessionsPage();
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(routeId),
                    routeId,
                    "The requested v8 module route is not allowlisted.");
        }
    }
}
