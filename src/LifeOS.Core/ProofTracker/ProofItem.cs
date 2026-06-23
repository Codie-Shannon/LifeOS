namespace LifeOS.Core.ProofTracker;

public sealed class ProofItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Project { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public ProofType Type { get; set; } = ProofType.Screenshot;

    public ProofStatus Status { get; set; } = ProofStatus.Draft;

    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public string Description { get; set; } = string.Empty;

    public string LinkOrPath { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
