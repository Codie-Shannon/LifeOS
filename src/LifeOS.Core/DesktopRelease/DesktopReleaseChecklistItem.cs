namespace LifeOS.Core.DesktopRelease;

public sealed class DesktopReleaseChecklistItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public DesktopReleaseCheckArea Area { get; set; } = DesktopReleaseCheckArea.CoreWorkflow;

    public DesktopReleaseCheckStatus Status { get; set; } = DesktopReleaseCheckStatus.ReviewNeeded;

    public int Priority { get; set; } = 3;

    public string Notes { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
