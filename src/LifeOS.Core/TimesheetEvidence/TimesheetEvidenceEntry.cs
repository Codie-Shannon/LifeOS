namespace LifeOS.Core.TimesheetEvidence;

public sealed class TimesheetEvidenceEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public string ClientOrProject { get; set; } = string.Empty;

    public string WorkPipelineItemId { get; set; } = string.Empty;

    public TimesheetEvidenceType Type { get; set; } = TimesheetEvidenceType.Investigation;

    public TimesheetEvidenceStatus Status { get; set; } = TimesheetEvidenceStatus.Draft;

    public decimal SuggestedHours { get; set; }

    public string Description { get; set; } = string.Empty;

    public string EvidenceNotes { get; set; } = string.Empty;

    public string ProofLink { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
