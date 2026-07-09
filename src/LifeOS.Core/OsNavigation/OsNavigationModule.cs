namespace LifeOS.Core.OsNavigation;

public sealed class OsNavigationModule
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public OsModuleCategory Category { get; set; } = OsModuleCategory.Command;

    public OsModuleStatus Status { get; set; } = OsModuleStatus.Active;

    public int SortOrder { get; set; }

    public bool IsPinned { get; set; }

    public bool IsVisible { get; set; } = true;

    public bool IsCoreWorkflow { get; set; }

    public string NavigationLabel { get; set; } = string.Empty;

    public string Purpose { get; set; } = string.Empty;

    public string NextAction { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
