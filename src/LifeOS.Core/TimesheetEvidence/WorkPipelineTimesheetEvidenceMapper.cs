using LifeOS.Core.WorkPipeline;

namespace LifeOS.Core.TimesheetEvidence;

public static class WorkPipelineTimesheetEvidenceMapper
{
    public static TimesheetEvidenceEntry CreateDraft(WorkPipelineItem item, TimesheetEvidenceType type, string evidenceNotes)
    {
        ArgumentNullException.ThrowIfNull(item);

        return new TimesheetEvidenceEntry
        {
            Date = DateOnly.FromDateTime(DateTime.Today),
            ClientOrProject = string.IsNullOrWhiteSpace(item.ClientOrCompany) ? item.Title : item.ClientOrCompany,
            WorkPipelineItemId = item.Id.ToString(),
            Type = type,
            Status = TimesheetEvidenceStatus.Draft,
            SuggestedHours = TimesheetEvidenceRules.SuggestHours(type),
            Description = TimesheetEvidenceRules.BuildClientSafeDescription(item.NextAction, item.LastOutcome),
            EvidenceNotes = evidenceNotes,
            ProofLink = item.LinkedProofNotes
        };
    }
}
