namespace LifeOS.Core.CareerStudio;

public static class CareerApplicationProofData
{
    public static IReadOnlyList<CareerApplication> Build(CareerStudioProof proof, DateTimeOffset now)
    {
        var service = new CareerApplicationService();
        var created = service.CreateFromApprovedOpportunity(proof.Opportunities[0], true, now.AddDays(-3)).Application;
        var checklist = created.Checklist.Select(item =>
            item.Type is ChecklistItemType.Cv or ChecklistItemType.RequiredForms or ChecklistItemType.Portfolio
                ? item with { IsComplete = true, CompletedUtc = now.AddDays(-2), EvidenceLinkId = "doc-redacted-proof" }
                : item).ToArray();

        var timeline = created.Timeline.Concat(
        [
            new ApplicationStageEvent(now.AddDays(-2), ApplicationState.Preparing, ApplicationState.ReadyToSubmit, "Required checklist items completed explicitly."),
            new ApplicationStageEvent(now.AddDays(-1), ApplicationState.ReadyToSubmit, ApplicationState.Submitted, "External submission recorded; LifeOS did not send it."),
            new ApplicationStageEvent(now.AddHours(-12), ApplicationState.Submitted, ApplicationState.Acknowledged, "Acknowledgement imported as read-only context.")
        ]).ToArray();

        var interview = new InterviewRecord(
            "interview-61-a", now.AddDays(2), InterviewFormat.Video,
            ["Alex Example", "Hiring panel"], false,
            ["Prepare evidence examples", "Record post-interview notes"],
            "calendar-fictional-61");

        var followUps = new[]
        {
            new CareerFollowUp("follow-61-a", now.AddHours(-3), FollowUpState.Overdue, "Review acknowledgement and schedule manual follow-up", "email-read-only-61"),
            new CareerFollowUp("follow-61-b", now.AddDays(4), FollowUpState.WaitingOn, "Waiting on employer update", null)
        };

        return
        [
            created with
            {
                State = ApplicationState.Acknowledged,
                Checklist = checklist,
                SubmittedUtc = now.AddDays(-1),
                SubmissionChannel = "External job board",
                ConfirmationReference = "REDACTED-61",
                NextFollowUpUtc = now.AddHours(-3),
                Timeline = timeline,
                Interviews = [interview],
                FollowUps = followUps,
                Materials =
                [
                    new ApplicationMaterialLink("mat-61-cv", ChecklistItemType.Cv, "doc-cv-redacted", "Role-specific CV"),
                    new ApplicationMaterialLink("mat-61-port", ChecklistItemType.Portfolio, "portfolio-proof-01", "Portfolio evidence")
                ],
                EvidenceLinks = ["doc-cv-redacted", "portfolio-proof-01"]
            }
        ];
    }
}
