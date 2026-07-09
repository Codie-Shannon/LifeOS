namespace LifeOS.Core.SearchKnowledge;

public sealed class KnowledgeItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public KnowledgeItemKind Kind { get; set; } = KnowledgeItemKind.Note;

    public KnowledgeItemStatus Status { get; set; } = KnowledgeItemStatus.Active;

    public KnowledgeSourceType SourceType { get; set; } = KnowledgeSourceType.ManualEntry;

    public int Priority { get; set; } = 3;

    public string SourceModule { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string SearchText { get; set; } = string.Empty;

    public string NextAction { get; set; } = string.Empty;

    public DateOnly? ReviewDate { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
