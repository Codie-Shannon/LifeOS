using LifeOS.Core.TimesheetEvidence;
using LifeOS.Core.WorkPipeline;

namespace LifeOS.Core.EvidenceVault;

public static class EvidenceVaultLinkHelper
{
    public static EvidenceVaultItem FromWorkPipeline(WorkPipelineItem item, EvidenceVaultItemType type, string summary)
    {
        ArgumentNullException.ThrowIfNull(item);

        return new EvidenceVaultItem
        {
            Title = $"{item.Title} evidence",
            Type = type,
            Status = EvidenceVaultStatus.Captured,
            EvidenceDate = DateOnly.FromDateTime(DateTime.Today),
            ProjectOrClient = string.IsNullOrWhiteSpace(item.ClientOrCompany) ? item.Title : item.ClientOrCompany,
            WorkPipelineItemId = item.Id.ToString(),
            Summary = summary,
            Notes = item.LinkedProofNotes,
            NeedsReview = true
        };
    }

    public static EvidenceVaultItem FromTimesheetEvidence(TimesheetEvidenceEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        return new EvidenceVaultItem
        {
            Title = $"Timesheet proof: {entry.ClientOrProject}",
            Type = EvidenceVaultItemType.TimesheetDescription,
            Status = EvidenceVaultStatus.NeedsReview,
            EvidenceDate = entry.Date,
            ProjectOrClient = entry.ClientOrProject,
            WorkPipelineItemId = entry.WorkPipelineItemId,
            SourcePathOrReference = entry.ProofLink,
            Summary = entry.Description,
            Notes = entry.EvidenceNotes,
            NeedsReview = true
        };
    }
}
