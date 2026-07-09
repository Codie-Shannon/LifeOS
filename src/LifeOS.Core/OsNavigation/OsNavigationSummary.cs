namespace LifeOS.Core.OsNavigation;

public sealed class OsNavigationSummary
{
    public string Version { get; set; } = string.Empty;

    public string ShellMode { get; set; } = string.Empty;

    public int TotalModules { get; set; }

    public int VisibleModules { get; set; }

    public int CoreModules { get; set; }

    public int PinnedModules { get; set; }

    public int ReviewNeededModules { get; set; }

    public int PlannedModules { get; set; }

    public int HiddenModules { get; set; }

    public int GroupCount { get; set; }

    public int PrimaryWorkspaceCount { get; set; }

    public bool SidebarScrollEnabled { get; set; }

    public bool MajorUiReshapeDeferred { get; set; }

    public bool ExternalIntegrationsEnabled { get; set; }

    public IReadOnlyList<string> Reasons { get; set; } = [];

    public IReadOnlyList<OsNavigationModule> Pinned { get; set; } = [];

    public IReadOnlyList<OsNavigationModule> Core { get; set; } = [];

    public IReadOnlyList<OsNavigationModule> ReviewNeeded { get; set; } = [];

    public IReadOnlyList<OsNavigationModule> Planned { get; set; } = [];

    public IReadOnlyList<OsNavigationGroup> Groups { get; set; } = [];
}
