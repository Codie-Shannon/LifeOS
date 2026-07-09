namespace LifeOS.Core.OsNavigation;

public static class OsNavigationCalculator
{
    public static OsNavigationSummary Calculate(OsNavigationProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var modules = profile.Modules ?? [];
        var groups = profile.Groups ?? [];

        var visible = modules.Where(module => module.IsVisible).ToList();
        var core = modules.Where(module => module.IsCoreWorkflow || module.Status == OsModuleStatus.Core).ToList();
        var pinned = modules.Where(module => module.IsPinned && module.IsVisible).ToList();
        var reviewNeeded = modules.Where(module => module.Status == OsModuleStatus.ReviewNeeded).ToList();
        var planned = modules.Where(module => module.Status == OsModuleStatus.Planned).ToList();
        var hidden = modules.Where(module => !module.IsVisible || module.Status == OsModuleStatus.Hidden).ToList();

        var reasons = new List<string>();

        if (profile.SidebarScrollEnabled)
        {
            reasons.Add("Sidebar scrolling is enabled so lower modules remain reachable as the OS grows.");
        }

        if (profile.WorkspaceGroupsEnabled)
        {
            reasons.Add("Workspace groups are enabled locally so the growing module list has an operating map.");
        }

        if (profile.CoreModulesProtected)
        {
            reasons.Add("Core modules are protected; command, money, proof, daily flow, relationship, release, safety, and spine areas remain visible.");
        }

        if (profile.MajorUiReshapeDeferred)
        {
            reasons.Add("Major UI/workspace reshape remains deferred until after integrations and AI assistant workflows prove real usage.");
        }

        if (profile.ExternalIntegrationsEnabled)
        {
            reasons.Add("External integrations are on. This should not be used for the offline foundation lane without review.");
        }
        else
        {
            reasons.Add("External integrations remain off; v3.0 is still local-first.");
        }

        if (reviewNeeded.Count > 0)
        {
            reasons.Add($"{reviewNeeded.Count} module(s) need review before final v3.0 screenshots/docs.");
        }

        if (planned.Count > 0)
        {
            reasons.Add($"{planned.Count} module(s) are planned/future and should not be shown as complete.");
        }

        return new OsNavigationSummary
        {
            Version = profile.Version,
            ShellMode = profile.ShellMode,
            TotalModules = modules.Count,
            VisibleModules = visible.Count,
            CoreModules = core.Count,
            PinnedModules = pinned.Count,
            ReviewNeededModules = reviewNeeded.Count,
            PlannedModules = planned.Count,
            HiddenModules = hidden.Count,
            GroupCount = groups.Count,
            PrimaryWorkspaceCount = groups.Count(group => group.IsPrimaryWorkspace),
            SidebarScrollEnabled = profile.SidebarScrollEnabled,
            MajorUiReshapeDeferred = profile.MajorUiReshapeDeferred,
            ExternalIntegrationsEnabled = profile.ExternalIntegrationsEnabled,
            Reasons = reasons,
            Pinned = pinned.OrderBy(module => module.SortOrder).ToList(),
            Core = core.OrderBy(module => module.SortOrder).ToList(),
            ReviewNeeded = reviewNeeded.OrderBy(module => module.SortOrder).ToList(),
            Planned = planned.OrderBy(module => module.SortOrder).ToList(),
            Groups = groups.OrderBy(group => group.SortOrder).ToList()
        };
    }

    public static string FormatCategory(OsModuleCategory category)
    {
        return category switch
        {
            OsModuleCategory.Command => "Command",
            OsModuleCategory.Money => "Money",
            OsModuleCategory.Work => "Work",
            OsModuleCategory.Proof => "Proof",
            OsModuleCategory.Relationship => "Relationship",
            OsModuleCategory.Daily => "Daily",
            OsModuleCategory.Release => "Release",
            OsModuleCategory.System => "System",
            OsModuleCategory.Knowledge => "Knowledge",
            _ => category.ToString()
        };
    }

    public static string FormatStatus(OsModuleStatus status)
    {
        return status switch
        {
            OsModuleStatus.Core => "Core",
            OsModuleStatus.Active => "Active",
            OsModuleStatus.ReviewNeeded => "Review needed",
            OsModuleStatus.Planned => "Planned",
            OsModuleStatus.Hidden => "Hidden",
            OsModuleStatus.Archived => "Archived",
            _ => status.ToString()
        };
    }
}
