namespace LifeOS.Core.OsNavigation;

public sealed class OsNavigationProfile
{
    public string Version { get; set; } = "v3.0";

    public string ShellMode { get; set; } = "Offline OS navigation";

    public bool SidebarScrollEnabled { get; set; } = true;

    public bool WorkspaceGroupsEnabled { get; set; } = true;

    public bool CoreModulesProtected { get; set; } = true;

    public bool MajorUiReshapeDeferred { get; set; } = true;

    public bool ExternalIntegrationsEnabled { get; set; }

    public string Notes { get; set; } = "OS navigation and core module map for the offline foundation.";

    public List<OsNavigationModule> Modules { get; set; } = [];

    public List<OsNavigationGroup> Groups { get; set; } = [];
}
