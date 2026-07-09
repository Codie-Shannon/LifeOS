using LifeOS.Core.RelationshipRadar;

namespace LifeOS.Shared.RelationshipRadar;

public static class RelationshipRadarDemoData
{
    public static List<RelationshipRadarProfile> CreateDefaultProfiles()
    {
        return
        [
            new RelationshipRadarProfile
            {
                Name = "Client Contact A",
                RoleOrContext = "Workshop proof review",
                Status = RelationshipRadarStatus.WaitingOnThem,
                WaitingOn = RelationshipWaitingOn.Them,
                LastContactDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-3)),
                NextFollowUpDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
                LinkedWork = "Workshop Proof Project",
                NextAction = "Wait until the review window has passed before chasing.",
                Notes = "Safe fictional demo relationship.",
                DoNotChase = true
            },
            new RelationshipRadarProfile
            {
                Name = "Project Reviewer",
                RoleOrContext = "Client portal cleanup",
                Status = RelationshipRadarStatus.FollowUpDue,
                WaitingOn = RelationshipWaitingOn.Me,
                LastContactDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-7)),
                NextFollowUpDate = DateOnly.FromDateTime(DateTime.Today),
                LinkedWork = "Client Portal Cleanup",
                NextAction = "Send concise update with proof link.",
                Notes = "No real company names, people, emails, or private URLs."
            },
            new RelationshipRadarProfile
            {
                Name = "Payment Contact",
                RoleOrContext = "Invoice/payment follow-up",
                Status = RelationshipRadarStatus.Active,
                WaitingOn = RelationshipWaitingOn.Unknown,
                LastContactDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-2)),
                NextFollowUpDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
                LinkedWork = "Door Invoice OCR Proof",
                NextAction = "Keep warm. Expected money is not safe money until paid.",
                Notes = "Safe fictional money-pressure example."
            }
        ];
    }
}
