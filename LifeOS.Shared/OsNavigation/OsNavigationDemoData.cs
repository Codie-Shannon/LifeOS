using LifeOS.Core.OsNavigation;

namespace LifeOS.Shared.OsNavigation;

public static class OsNavigationDemoData
{
    public static OsNavigationProfile CreateDefaultProfile()
    {
        var modules = new List<OsNavigationModule>
        {
            Module("Command Centre", OsModuleCategory.Command, OsModuleStatus.Core, 10, true, true, "Command Centre", "Main operating dashboard.", "Use it as the first stop before opening modules."),
            Module("Money Pressure", OsModuleCategory.Money, OsModuleStatus.Core, 20, true, true, "Money Pressure", "Safe-to-spend and pressure state.", "Keep expected money separate from safe money."),
            Module("Money Timeline", OsModuleCategory.Money, OsModuleStatus.Active, 30, false, true, "Money Timeline", "Money timing view.", "Use for timing pressure, not accounting."),
            Module("Agenda", OsModuleCategory.Daily, OsModuleStatus.Active, 40, false, true, "Agenda", "Day and scheduled pressure.", "Keep visible for daily planning."),
            Module("Pay Later", OsModuleCategory.Money, OsModuleStatus.Active, 50, false, true, "Pay Later", "Deferred payment pressure.", "Keep separate from safe spending."),
            Module("Weekly Close-Out", OsModuleCategory.Command, OsModuleStatus.Active, 60, false, true, "Weekly Close-Out", "Weekly reset/review.", "Use to close loops."),
            Module("Work Sessions", OsModuleCategory.Work, OsModuleStatus.Core, 70, true, true, "Work Sessions", "Work logging foundation.", "Keep linked to proof and paid work."),
            Module("Paid Work Centre", OsModuleCategory.Work, OsModuleStatus.Core, 80, true, true, "Paid Work Centre", "Paid work, money, and proof gate.", "Keep expected money unsafe until paid."),
            Module("Proof Tracker", OsModuleCategory.Proof, OsModuleStatus.Core, 90, true, true, "Proof Tracker", "Proof capture and reuse.", "Link proof to work and release notes."),
            Module("Follow-Ups", OsModuleCategory.Relationship, OsModuleStatus.Active, 100, false, true, "Follow-Ups", "Follow-up pressure.", "Manual review before sending."),
            Module("Work Pipeline", OsModuleCategory.Work, OsModuleStatus.Active, 110, false, true, "Work Pipeline", "Pipeline state.", "Keep pipeline value separate from safe money."),
            Module("Daily Operating Flow", OsModuleCategory.Daily, OsModuleStatus.Core, 120, true, true, "Daily Operating Flow", "Day-control workspace.", "Pick the next safe action."),
            Module("Daily State", OsModuleCategory.Daily, OsModuleStatus.Active, 130, false, true, "Daily State", "Passive waiting/scheduled state.", "Keep separate from active daily flow."),
            Module("Timesheet Evidence", OsModuleCategory.Proof, OsModuleStatus.Active, 140, false, true, "Timesheet Evidence", "Timesheet proof context.", "Convert work into client-safe entries."),
            Module("Evidence Vault", OsModuleCategory.Proof, OsModuleStatus.Core, 150, true, true, "Evidence Vault", "Local proof metadata.", "Keep screenshots and docs demo-safe."),
            Module("Relationship Radar", OsModuleCategory.Relationship, OsModuleStatus.Core, 160, true, true, "Relationship Radar", "Waiting-on and do-not-chase state.", "Avoid over-chasing."),
            Module("Projects", OsModuleCategory.Work, OsModuleStatus.ReviewNeeded, 170, false, true, "Projects", "Project shell carried forward.", "Needs stronger v3 module definition."),
            Module("TimerAgent", OsModuleCategory.Work, OsModuleStatus.ReviewNeeded, 180, false, true, "TimerAgent", "Timer background module.", "Needs review against work-session flow."),
            Module("Universal Spine", OsModuleCategory.System, OsModuleStatus.Core, 190, true, true, "Universal Spine", "Cross-module local links.", "Use as the context spine before search/AI."),
            Module("Desktop Release", OsModuleCategory.Release, OsModuleStatus.Core, 200, true, true, "Desktop Release", "Release readiness checkpoint.", "Keep docs/screenshots/tag discipline visible."),
            Module("Settings / Safety", OsModuleCategory.System, OsModuleStatus.Core, 210, true, true, "Settings / Safety", "Safety and preference guardrails.", "Keep local-only, manual review, privacy and expected-money protection on."),
            Module("Advanced Search", OsModuleCategory.Knowledge, OsModuleStatus.Planned, 220, false, false, "Advanced Search", "Future search layer.", "Planned for Group 08, not complete in v3.0."),
            Module("Knowledge Base", OsModuleCategory.Knowledge, OsModuleStatus.Planned, 230, false, false, "Knowledge Base", "Future knowledge/archive layer.", "Planned after core navigation is stable.")
        };

        return new OsNavigationProfile
        {
            Version = "v3.0",
            ShellMode = "Offline OS navigation",
            SidebarScrollEnabled = true,
            WorkspaceGroupsEnabled = true,
            CoreModulesProtected = true,
            MajorUiReshapeDeferred = true,
            ExternalIntegrationsEnabled = false,
            Notes = "OS navigation and core module map for the offline foundation.",
            Modules = modules,
            Groups =
            [
                Group("Command workspace", OsModuleCategory.Command, 10, true, "Start here: command, weekly close-out, release state.", modules, "Command Centre", "Weekly Close-Out", "Desktop Release"),
                Group("Money and paid work", OsModuleCategory.Money, 20, true, "Safe money, expected money, pay-later pressure, and paid-work proof.", modules, "Money Pressure", "Money Timeline", "Pay Later", "Paid Work Centre", "Work Pipeline"),
                Group("Proof and evidence", OsModuleCategory.Proof, 30, true, "Proof tracker, evidence vault, timesheet evidence, and screenshot safety.", modules, "Proof Tracker", "Evidence Vault", "Timesheet Evidence"),
                Group("Daily operating flow", OsModuleCategory.Daily, 40, true, "Daily operating blocks, agenda pressure, and passive daily state.", modules, "Daily Operating Flow", "Agenda", "Daily State"),
                Group("Relationships and follow-ups", OsModuleCategory.Relationship, 50, true, "Waiting-on, do-not-chase, and manual follow-up review.", modules, "Relationship Radar", "Follow-Ups"),
                Group("System spine and safety", OsModuleCategory.System, 60, true, "Universal spine, settings, safety, and navigation structure.", modules, "Universal Spine", "Settings / Safety", "Projects", "TimerAgent"),
                Group("Planned knowledge layer", OsModuleCategory.Knowledge, 70, false, "Future advanced search and knowledge modules.", modules, "Advanced Search", "Knowledge Base")
            ]
        };
    }

    private static OsNavigationModule Module(
        string title,
        OsModuleCategory category,
        OsModuleStatus status,
        int sortOrder,
        bool pinned,
        bool coreWorkflow,
        string navigationLabel,
        string purpose,
        string nextAction)
    {
        return new OsNavigationModule
        {
            Title = title,
            Category = category,
            Status = status,
            SortOrder = sortOrder,
            IsPinned = pinned,
            IsVisible = status != OsModuleStatus.Hidden && status != OsModuleStatus.Planned,
            IsCoreWorkflow = coreWorkflow,
            NavigationLabel = navigationLabel,
            Purpose = purpose,
            NextAction = nextAction,
            Notes = "Fictional/demo-safe module map.",
            UpdatedAt = DateTime.Now
        };
    }

    private static OsNavigationGroup Group(
        string title,
        OsModuleCategory category,
        int sortOrder,
        bool primary,
        string purpose,
        IReadOnlyCollection<OsNavigationModule> modules,
        params string[] moduleTitles)
    {
        return new OsNavigationGroup
        {
            Title = title,
            Category = category,
            SortOrder = sortOrder,
            IsPrimaryWorkspace = primary,
            Purpose = purpose,
            ModuleIds = modules
                .Where(module => moduleTitles.Contains(module.Title, StringComparer.OrdinalIgnoreCase))
                .Select(module => module.Id)
                .ToList()
        };
    }
}
