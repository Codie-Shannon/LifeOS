namespace LifeOS.Core.UniversalSpine;

public sealed class UniversalSpineLink
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid FromItemId { get; set; }

    public Guid ToItemId { get; set; }

    public UniversalSpineLinkType LinkType { get; set; } = UniversalSpineLinkType.RelatesTo;

    public string Label { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
