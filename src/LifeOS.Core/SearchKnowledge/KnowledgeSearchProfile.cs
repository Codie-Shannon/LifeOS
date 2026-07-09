namespace LifeOS.Core.SearchKnowledge;

public sealed class KnowledgeSearchProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string QueryHint { get; set; } = string.Empty;

    public string Scope { get; set; } = string.Empty;

    public bool IsPinned { get; set; }

    public bool IsManualOnly { get; set; } = true;

    public bool UsesExternalIndex { get; set; }

    public string Notes { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
