namespace LifeOS.Shared.Shell;

public static class LifeOSModuleCatalog
{
    public static IReadOnlyList<LifeOSModuleDefinition> Modules { get; } =
    [
        new LifeOSModuleDefinition
        {
            Kind = LifeOSModuleKind.CommandCentre,
            Title = "Command Centre",
            Badge = "v0.2",
            ShortDescription = "Weekly pressure command centre and desktop home screen.",
            DetailDescription = "The Command Centre brings money, agenda, pay-later, weekly close-out, follow-ups, work, proof, and pressure into one weekly view.",
            PlatformRole = "Desktop proves the full command-centre model. Mobile will later receive the optimized daily-use version.",
            NextBuildFocus = "Surface real saved local data from the modules that now exist.",
            IsBuilt = true,
            IsSharedCoreModule = true
        },

        new LifeOSModuleDefinition
        {
            Kind = LifeOSModuleKind.MoneyPressure,
            Title = "Money Pressure",
            Badge = "Built",
            ShortDescription = "Safe-to-spend, income, bills, deductions, and weekly danger points.",
            DetailDescription = "Shared LifeOS module for safe-to-spend, manual balance, income, bills, deductions, and weekly pressure.",
            PlatformRole = "Shared core module. Desktop builds and tests the full model first; mobile receives the cleaner daily-use flow.",
            NextBuildFocus = "Keep manual local-first pressure tracking stable.",
            IsBuilt = true,
            IsSharedCoreModule = true
        },

        new LifeOSModuleDefinition
        {
            Kind = LifeOSModuleKind.Agenda,
            Title = "Agenda",
            Badge = "v0.2 foundation",
            ShortDescription = "Appointments, tasks, due dates, fixed commitments, and time pressure.",
            DetailDescription = "Agenda tracks what matters this week: tasks, appointments, deadlines, follow-ups, payments, fixed commitments, and date-based pressure.",
            PlatformRole = "Shared core module. Desktop tests the full weekly model first; mobile later gets quick daily glance.",
            NextBuildFocus = "Track open, overdue, due-today, this-week, and high-pressure agenda items.",
            IsBuilt = true,
            IsSharedCoreModule = true
        },

        new LifeOSModuleDefinition
        {
            Kind = LifeOSModuleKind.PayLater,
            Title = "Pay Later",
            Badge = "v0.2 foundation",
            ShortDescription = "Deferred payments, due dates, and payment pressure.",
            DetailDescription = "Pay Later tracks deferred obligations and pressure so future money is not treated as free money.",
            PlatformRole = "Shared money-pressure module. Desktop proves the detail; mobile can later show what is due soon.",
            NextBuildFocus = "Track due-this-week, overdue, and high-pressure pay-later items.",
            IsBuilt = true,
            IsSharedCoreModule = true
        },

        new LifeOSModuleDefinition
        {
            Kind = LifeOSModuleKind.WeeklyCloseOut,
            Title = "Weekly Close-Out",
            Badge = "v0.2 foundation",
            ShortDescription = "What got done, what moved, what is still waiting, and next week pressure.",
            DetailDescription = "Weekly Close-Out converts chaos into a simple weekly record: done, moved, waiting-on, next-week pressure, and notes.",
            PlatformRole = "Shared reflection/operating-loop module. Desktop captures the full close-out first.",
            NextBuildFocus = "Capture a weekly close-out entry and surface whether the current week is closed out.",
            IsBuilt = true,
            IsSharedCoreModule = true
        },

        new LifeOSModuleDefinition
        {
            Kind = LifeOSModuleKind.FollowUps,
            Title = "Follow-Ups",
            Badge = "Built",
            ShortDescription = "People, client replies, waiting-on items, and money-linked follow-ups.",
            DetailDescription = "Shared LifeOS module for people, organisations, client replies, waiting-on items, expected dates, and money-linked follow-ups.",
            PlatformRole = "Shared core module. Desktop can show richer client/work context; mobile should make follow-ups quick and obvious.",
            NextBuildFocus = "Track person/org, context, due date, status, money-linked flag, and next action.",
            IsBuilt = true,
            IsSharedCoreModule = true
        },

        new LifeOSModuleDefinition
        {
            Kind = LifeOSModuleKind.Projects,
            Title = "Projects",
            Badge = "Planned",
            ShortDescription = "Portfolio builds, proof projects, shipped versions, and active focus.",
            DetailDescription = "Planned shared LifeOS module for active builds, client proofs, portfolio work, shipped versions, and project pressure.",
            PlatformRole = "Shared core module. Connects build work to proof, case studies, client opportunities, and business momentum.",
            NextBuildFocus = "Later: project list with status, next action, proof link, and pressure note.",
            IsSharedCoreModule = true
        },

        new LifeOSModuleDefinition
        {
            Kind = LifeOSModuleKind.TimerAgent,
            Title = "TimerAgent",
            Badge = "Desktop-only utility",
            ShortDescription = "Desktop-only utility for billable work sessions, earned income, tax set-aside, and CSV work logs.",
            DetailDescription = "TimerAgent tracks focused work, billable sessions, earned income, tax set-aside, and CSV logs. Future LifeOS versions can read TimerAgent work data into weekly pressure summaries.",
            PlatformRole = "Desktop-only utility. Mobile may view summaries later, but tray/hotkey/overlay remain Windows desktop features.",
            NextBuildFocus = "Later: read TimerAgent CSV/work-session data into weekly pressure summary.",
            IsBuilt = true,
            IsDesktopOnly = true
        },

        new LifeOSModuleDefinition
        {
            Kind = LifeOSModuleKind.Settings,
            Title = "Settings",
            Badge = "Platform settings",
            ShortDescription = "Theme, storage, export, backup, and shared app preferences.",
            DetailDescription = "Planned shared settings for theme, local storage, export, backup, privacy controls, and desktop/mobile preferences.",
            PlatformRole = "Shared platform module.",
            NextBuildFocus = "Theme/storage/export settings later.",
            IsSharedCoreModule = true
        }
    ];

    public static LifeOSModuleDefinition GetModule(LifeOSModuleKind kind)
    {
        return Modules.First(module => module.Kind == kind);
    }
}
