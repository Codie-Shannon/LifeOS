namespace LifeOS.Core.TimesheetEvidence;

public sealed class ProofLinkRequirement
{
    public string Source { get; init; } = string.Empty;

    public string Reason { get; init; } = string.Empty;

    public bool NeedsProof { get; init; }

    public bool NeedsTimesheetDescription { get; init; }
}
