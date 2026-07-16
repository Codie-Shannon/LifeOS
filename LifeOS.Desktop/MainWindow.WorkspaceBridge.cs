using System;
using System.Collections.Generic;
using LifeOS.Shared.Shell;

namespace LifeOS.Desktop;

public partial class MainWindow
{
    private static readonly string[] Group44WorkspaceRouteIds =
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
        "weekly-close-out",
        "work-pipeline",
        "work-sessions",
    };

    public static IReadOnlyCollection<string> WorkspaceRouteIds =>
        Group44WorkspaceRouteIds;

    public void OpenWorkspaceModule(string routeId)
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
                    "The requested workspace route is not allowlisted.");
        }
    }
}
