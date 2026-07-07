namespace LifeOS.Core.TimesheetEvidence;

public sealed class TimesheetEvidenceSummary
{
    public int TotalEntries { get; init; }

    public int ReadyForTimesheetCount { get; init; }

    public int DraftCount { get; init; }

    public decimal SuggestedHoursTotal { get; init; }

    public IReadOnlyList<TimesheetEvidenceEntry> ReadyEntries { get; init; } = [];

    public IReadOnlyList<string> Reasons { get; init; } = [];
}
