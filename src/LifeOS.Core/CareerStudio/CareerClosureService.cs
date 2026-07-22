namespace LifeOS.Core.CareerStudio;

public sealed class CareerClosureService
{
    public CareerFollowUpStatus CalculateStatus(CareerFollowUpPlan plan, DateTimeOffset now)
    {
        if (plan.Status is CareerFollowUpStatus.Completed or CareerFollowUpStatus.Cancelled or CareerFollowUpStatus.Deferred or CareerFollowUpStatus.Waiting)
            return plan.Status;
        return plan.DueUtc < now ? CareerFollowUpStatus.Overdue : plan.DueUtc.Date == now.Date ? CareerFollowUpStatus.Due : CareerFollowUpStatus.Planned;
    }

    public CareerFollowUpPlan Transition(CareerFollowUpPlan plan, CareerFollowUpStatus status, DateTimeOffset occurredUtc, string summary) =>
        plan with { Status = status, History = plan.History.Append(new FollowUpEvent(occurredUtc, status.ToString(), summary)).ToArray() };

    public bool CanIncludeReference(ReferenceContact reference) => reference.CanUseInApplicationPack;

    public CareerMetricSnapshot BuildReview(CareerClosureProof proof, CareerReviewPeriod period) => proof.Review with { Period = period };
}

public static class CareerClosureProofData
{
    public static CareerClosureProof Build(DateTimeOffset now)
    {
        var followUps = new[]
        {
            new CareerFollowUpPlan("fu-due", "Check application acknowledgement", CareerFollowUpType.ApplicationAcknowledgement, now, "Owner", "app-61-a", "Email draft only", "Confirm receipt without sending automatically.", CareerFollowUpStatus.Due, null, new[]{ new FollowUpEvent(now.AddDays(-1), "Planned", "Follow-up created explicitly.") }),
            new CareerFollowUpPlan("fu-overdue", "Employer promised update", CareerFollowUpType.PromisedUpdate, now.AddDays(-2), "Owner", "opp-61-a", "Manual email", "Ask whether the hiring timeline has changed.", CareerFollowUpStatus.Overdue, null, new[]{ new FollowUpEvent(now.AddDays(-5), "Planned", "Promised update recorded.") }),
            new CareerFollowUpPlan("fu-waiting", "Wait for interview outcome", CareerFollowUpType.InterviewOutcome, now.AddDays(1), "Owner", "interview-61-a", "No action", "Waiting on employer outcome.", CareerFollowUpStatus.Waiting, null, new[]{ new FollowUpEvent(now.AddHours(-8), "Waiting", "Interview completed; no message sent.") }),
            new CareerFollowUpPlan("fu-complete", "Portfolio delivery confirmation", CareerFollowUpType.PortfolioDelivery, now.AddDays(-3), "Owner", "pack-61-a", "External record", "", CareerFollowUpStatus.Completed, "Delivery confirmed externally.", new[]{ new FollowUpEvent(now.AddDays(-3), "Completed", "External delivery evidence recorded.") })
        };

        var references = new[]
        {
            new ReferenceContact("ref-ready", "person-ref-01", "Avery Morgan (fictional)", "Former project lead", "Technical reviewer", "Review-based software delivery", new ReferencePermission(ReferencePermissionState.Granted, now.AddDays(-30), now.AddDays(60), "permission-redacted-01"), "Email", "Available with 48-hour notice", CareerPrivacyState.Redacted, "aâ€¢â€¢â€¢â€¢@example.invalid", new[]{ new ReferenceUsage("pack-61-a", "opp-61-a", now.AddDays(-2), "Reference placeholder approved") }, new[]{ new ReferenceHistory(now.AddDays(-30), "Permission granted", "Consent evidence retained privately.") }, Array.Empty<string>()),
            new ReferenceContact("ref-review", "person-ref-02", "Jordan Lee (fictional)", "Client contact", "Workflow owner", "Client proof context", new ReferencePermission(ReferencePermissionState.Unknown, null, now, "none"), "Not supplied", "Unknown", CareerPrivacyState.Private, null, Array.Empty<ReferenceUsage>(), Array.Empty<ReferenceHistory>(), new[]{ "Permission not granted", "Contact method missing", "Availability not confirmed" })
        };

        var questions = new QuestionsToAskPlan("interview-61-a", "Software Application Developer", now.AddDays(3), "Ask only user-reviewed questions.", new[]
        {
            new InterviewQuestionToAsk("q1", "interview-61-a", 1, "Role", "What would success look like in the first 90 days?", "Listen for concrete delivery expectations.", false),
            new InterviewQuestionToAsk("q2", "interview-61-a", 2, "Team", "How does the team review evidence and technical decisions?", "Relates to review-first delivery.", false),
            new InterviewQuestionToAsk("q3", "interview-61-a", 3, "Process", "What are the next steps after this interview?", "Record the promised update date.", true)
        });

        var coverage = new EvidenceCoverageSummary(3,4,1,1,1,2,1,2, new[]
        {
            new CoverageDrillDown("CV facts", "Partial", "career-profile", "One stale profile fact requires review."),
            new CoverageDrillDown("Portfolio evidence", "Covered", "portfolio-lifeos", "Trusted project and document proof linked."),
            new CoverageDrillDown("References", "Review required", "ref-review", "One reference lacks explicit permission."),
            new CoverageDrillDown("Application packs", "Partial", "pack-61-a", "References placeholder remains missing.")
        });
        var review = new CareerMetricSnapshot(new CareerReviewPeriod(now.AddDays(-30), now), "Complete with explicit partial coverage", new PipelineSummary(7,3,2,1,0,1), new FollowUpPerformance(2,1,1,31.5), coverage,
            new Dictionary<string,int>{{"Direct employer",3},{"Job board",3},{"Referral",1}},
            new Dictionary<string,int>{{"Software",5},{"Automation",2}},
            new Dictionary<string,int>{{"Hybrid",4},{"Remote",2},{"On-site",1}},
            new Dictionary<string,int>{{"Discovered",2},{"Reviewing",2},{"Applied",2},{"Interview",1}});

        return new CareerClosureProof(followUps, new[]{ new WaitingOnRecord("wait-1", "interview-61-a", "Employer outcome", now.AddHours(-8), now.AddDays(2), "No automatic chase is scheduled.") }, references, questions, review);
    }
}
