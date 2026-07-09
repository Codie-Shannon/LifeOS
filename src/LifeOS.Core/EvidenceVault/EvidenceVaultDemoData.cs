namespace LifeOS.Core.EvidenceVault;

public static class EvidenceVaultDemoData
{
    public static List<EvidenceVaultItem> CreateDefaultItems()
    {
        return
        [
            new EvidenceVaultItem
            {
                Title = "Workshop proof screenshot",
                Type = EvidenceVaultItemType.Screenshot,
                Status = EvidenceVaultStatus.NeedsReview,
                EvidenceDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
                ProjectOrClient = "Workshop Proof Project",
                SourcePathOrReference = "Screenshots/workshop-proof-command-centre.png",
                Summary = "Shows the proof workflow state before sending an update.",
                Notes = "Safe fictional demo record.",
                NeedsReview = true
            },
            new EvidenceVaultItem
            {
                Title = "Client-safe follow-up draft",
                Type = EvidenceVaultItemType.EmailMessage,
                Status = EvidenceVaultStatus.Captured,
                EvidenceDate = DateOnly.FromDateTime(DateTime.Today),
                ProjectOrClient = "Cloud Admin Follow-up",
                SourcePathOrReference = "Manual reference: draft follow-up note",
                Summary = "Captures wording and context before a follow-up is sent.",
                Notes = "No real names or contact details."
            },
            new EvidenceVaultItem
            {
                Title = "Code checkpoint commit",
                Type = EvidenceVaultItemType.CodeCommit,
                Status = EvidenceVaultStatus.Linked,
                EvidenceDate = DateOnly.FromDateTime(DateTime.Today),
                ProjectOrClient = "Client Portal Cleanup",
                SourcePathOrReference = "git commit reference placeholder",
                Summary = "Links implementation proof to a work item.",
                Notes = "Expected money is not safe money until paid."
            }
        ];
    }
}
