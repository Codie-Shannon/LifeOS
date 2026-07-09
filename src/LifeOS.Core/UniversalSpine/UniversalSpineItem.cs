namespace LifeOS.Core.UniversalSpine;

public sealed class UniversalSpineItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public UniversalSpineItemKind Kind { get; set; } = UniversalSpineItemKind.Work;

    public UniversalSpineSignalStatus Status { get; set; } = UniversalSpineSignalStatus.Active;

    public int Priority { get; set; } = 3;

    public string SourceModule { get; set; } = string.Empty;

    public string NextAction { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public DateOnly? ReviewDate { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
