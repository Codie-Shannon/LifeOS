namespace LifeOS.Core.CareerStudio;

public sealed class CareerOpportunityService
{
    private static readonly IReadOnlyDictionary<OpportunityStage, OpportunityStage[]> AllowedTransitions =
        new Dictionary<OpportunityStage, OpportunityStage[]>
        {
            [OpportunityStage.Discovered] = [OpportunityStage.Reviewing, OpportunityStage.Paused, OpportunityStage.Archived],
            [OpportunityStage.Reviewing] = [OpportunityStage.Interested, OpportunityStage.Declined, OpportunityStage.Paused, OpportunityStage.Archived],
            [OpportunityStage.Interested] = [OpportunityStage.Preparing, OpportunityStage.Declined, OpportunityStage.Paused],
            [OpportunityStage.Preparing] = [OpportunityStage.Applied, OpportunityStage.Withdrawn, OpportunityStage.Paused],
            [OpportunityStage.Applied] = [OpportunityStage.Interviewing, OpportunityStage.Rejected, OpportunityStage.Withdrawn],
            [OpportunityStage.Interviewing] = [OpportunityStage.Offer, OpportunityStage.Rejected, OpportunityStage.Withdrawn],
            [OpportunityStage.Offer] = [OpportunityStage.Accepted, OpportunityStage.Declined],
            [OpportunityStage.Paused] = [OpportunityStage.Reviewing, OpportunityStage.Interested, OpportunityStage.Preparing, OpportunityStage.Archived]
        };

    public StageTransitionResult Transition(
        CareerOpportunity opportunity,
        OpportunityStage target,
        DateTimeOffset now,
        string reviewedBy)
    {
        if (opportunity.Stage == target)
            return new(false, opportunity, "Opportunity is already at that stage.");

        if (!AllowedTransitions.TryGetValue(opportunity.Stage, out var allowed) || !allowed.Contains(target))
            return new(false, opportunity, $"Transition from {opportunity.Stage} to {target} is not allowed.");

        var history = opportunity.History.Concat(
        [
            new OpportunityHistory(
                now,
                "Stage changed",
                opportunity.Stage,
                target,
                $"Reviewed local stage change by {Redact(reviewedBy)}.")
        ]).ToArray();

        return new(true, opportunity with { Stage = target, History = history }, "Stage updated explicitly.");
    }

    public PriorityLevel CalculatePriority(CareerOpportunity opportunity, DateTimeOffset now)
    {
        if (opportunity.ClosingUtc is null) return opportunity.Priority;
        var remaining = opportunity.ClosingUtc.Value - now;
        if (remaining <= TimeSpan.Zero) return PriorityLevel.Urgent;
        if (remaining <= TimeSpan.FromDays(2)) return PriorityLevel.Urgent;
        if (remaining <= TimeSpan.FromDays(7)) return PriorityLevel.High;
        return opportunity.Priority;
    }

    public bool IsStale(CareerOpportunity opportunity, DateTimeOffset now) =>
        now - opportunity.FreshnessUtc > TimeSpan.FromDays(14);

    public IReadOnlyList<DuplicateOpportunityCandidate> FindDuplicateCandidates(
        IReadOnlyList<CareerOpportunity> opportunities)
    {
        var results = new List<DuplicateOpportunityCandidate>();
        for (var i = 0; i < opportunities.Count; i++)
        {
            for (var j = i + 1; j < opportunities.Count; j++)
            {
                var a = opportunities[i];
                var b = opportunities[j];
                var signals = new List<string>();
                if (Normalize(a.Employer.Name) == Normalize(b.Employer.Name)) signals.Add("normalized employer");
                if (Normalize(a.Title) == Normalize(b.Title)) signals.Add("normalized title");
                if (Normalize(a.Source.Reference ?? "") == Normalize(b.Source.Reference ?? "")) signals.Add("source reference");
                if (Math.Abs((a.CapturedUtc - b.CapturedUtc).TotalDays) <= 7) signals.Add("capture date proximity");
                if (signals.Count < 2) continue;

                results.Add(new DuplicateOpportunityCandidate(
                    $"dup-{a.Id}-{b.Id}",
                    a.Id,
                    b.Id,
                    Math.Min(1m, signals.Count / 4m),
                    signals,
                    CandidateReviewState.AwaitingReview));
            }
        }
        return results;
    }

    public ImportedOpportunityCandidate LinkImportedCandidate(
        string inboxItemId,
        string employer,
        string role,
        string sourceReference,
        DateTimeOffset capturedUtc) =>
        new(
            $"import-{inboxItemId}",
            inboxItemId,
            employer,
            role,
            sourceReference,
            capturedUtc,
            CandidateReviewState.AwaitingReview,
            "Imported context remains untrusted until explicitly reviewed.");

    private static string Normalize(string value) =>
        string.Concat(value.Where(char.IsLetterOrDigit)).ToUpperInvariant();

    private static string Redact(string value) =>
        string.IsNullOrWhiteSpace(value) ? "reviewer" : value.Length <= 2 ? "**" : value[..2] + "***";
}
