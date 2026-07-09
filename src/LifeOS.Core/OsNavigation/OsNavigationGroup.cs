namespace LifeOS.Core.OsNavigation;

public sealed class OsNavigationGroup
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public OsModuleCategory Category { get; set; } = OsModuleCategory.Command;

    public int SortOrder { get; set; }

    public bool IsPrimaryWorkspace { get; set; }

    public string Purpose { get; set; } = string.Empty;

    public List<Guid> ModuleIds { get; set; } = [];
}
