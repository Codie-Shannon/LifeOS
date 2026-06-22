namespace LifeOS.Shared.Shell;

public static class LifeOSModuleCatalog
{
    public static IReadOnlyList<LifeOSModuleDefinition> Modules { get; } =
    [
        new LifeOSModuleDefinition
        {
            Kind = LifeOSModuleKind.CommandCentre,
            Title = "Command Centre",
            Badge = "Shell v0.1",
            ShortDescription = "Weekly pressure command centre and desktop home screen.",
            DetailDescription = "The Command Centre brings money, work, agenda, follow-ups, projects, and pressure into one weekly view.",
            PlatformRole = "Desktop proves the full command-centre model. Mobile will later receive the optimized daily-use version.",
            NextBuildFocus = "Show a real weekly pressure summary once Money Pressure, Agenda, and Follow-Ups have data.",
            IsBuilt = true,
            IsSharedCoreModule = true
        },

        new LifeOSModuleDefinition
        {
            Kind = LifeOSModuleKind.MoneyPressure,
            Title = "Money Pressure",
            Badge = "Next major module",
            ShortDescription = "Safe-to-spend, income, bills, deductions, and weekly danger points.",
            DetailDescription = "Planned shared LifeOS module for safe-to-spend, manual balance, income, bills, pay-later, deductions, and weekly pressure. This will be shared between desktop and mobile.",
            PlatformRole = "Shared core module. Desktop should build and test the full model first; mobile receives the cleaner daily-use flow.",
            NextBuildFocus = "Manual balance, income items, bills due, deductions, and conservative safe-to-spend calculation.",
            IsSharedCoreModule = true
        },

        new LifeOSModuleDefinition
        {
            Kind = LifeOSModuleKind.Agenda,
            Title = "Agenda",
            Badge = "Shared core module",
            ShortDescription = "Appointments, tasks, fixed/flexible events, and time pressure.",
            DetailDescription = "Planned shared LifeOS module for appointments, tasks, payments, follow-ups, fixed/flexible commitments, and date-based pressure. Desktop will test the full model first. Mobile will receive the optimized daily-use version.",
            PlatformRole = "Shared core module. Agenda should become date > time > event, but presented calmly instead of as a cluttered calendar grid.",
            NextBuildFocus = "Create agenda item model and simple desktop list view later.",
            IsSharedCoreModule = true
        },

        new LifeOSModuleDefinition
        {
            Kind = LifeOSModuleKind.FollowUps,
            Title = "Follow-Ups",
            Badge = "Shared core module",
            ShortDescription = "People, client replies, waiting-on items, and money-linked follow-ups.",
            DetailDescription = "Planned shared LifeOS module for people, organisations, client replies, waiting-on items, expected dates, and money-linked follow-ups.",
            PlatformRole = "Shared core module. Desktop can show richer client/work context; mobile should make follow-ups quick and obvious.",
            NextBuildFocus = "Track person/org, context, due date, status, money-linked flag, and next action.",
            IsSharedCoreModule = true
        },

        new LifeOSModuleDefinition
        {
            Kind = LifeOSModuleKind.Projects,
            Title = "Projects",
            Badge = "Shared core module",
            ShortDescription = "Portfolio builds, proof projects, shipped versions, and active focus.",
            DetailDescription = "Planned shared LifeOS module for active builds, client proofs, portfolio work, shipped versions, and project pressure.",
            PlatformRole = "Shared core module. Connects build work to proof, case studies, client opportunities, and business momentum.",
            NextBuildFocus = "Basic project list with status, next action, proof link, and pressure note.",
            IsSharedCoreModule = true
        },

        new LifeOSModuleDefinition
        {
            Kind = LifeOSModuleKind.TimerAgent,
            Title = "TimerAgent",
            Badge = "Desktop-only utility",
            ShortDescription = "Desktop-only utility for billable work sessions, earned income, tax set-aside, and CSV work logs.",
            DetailDescription = "TimerAgent is already shipped as the first desktop-only LifeOS utility. It tracks focused work, billable sessions, earned income, tax set-aside, and CSV logs. Future LifeOS versions will read TimerAgent work data into weekly pressure summaries.",
            PlatformRole = "Desktop-only utility. Mobile may view summaries later, but the tray, global shortcut, and compact overlay remain Windows desktop features.",
            NextBuildFocus = "Later: read TimerAgent CSV/work-session data into the LifeOS weekly pressure summary.",
            IsBuilt = true,
            IsDesktopOnly = true
        },

        new LifeOSModuleDefinition
        {
            Kind = LifeOSModuleKind.Settings,
            Title = "Settings",
            Badge = "Platform settings",
            ShortDescription = "Theme, storage, export, backup, and shared app preferences.",
            DetailDescription = "Planned shared settings for Light / Dark / System theme, local storage, export and backup, privacy controls, and desktop/mobile preferences.",
            PlatformRole = "Shared platform module. Settings should support both desktop and mobile without trapping preferences in one UI.",
            NextBuildFocus = "Theme mode definition: Light, Dark, System. Storage/export settings later.",
            IsSharedCoreModule = true
        }
    ];

    public static LifeOSModuleDefinition GetModule(LifeOSModuleKind kind)
    {
        return Modules.First(module => module.Kind == kind);
    }
}