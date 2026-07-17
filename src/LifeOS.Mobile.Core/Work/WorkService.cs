namespace LifeOS.Mobile.Core.Work;

public sealed class WorkService
{
    private readonly Dictionary<string, WorkReviewResult> _reviews =
        new(StringComparer.Ordinal);

    public WorkDashboardSnapshot BuildSnapshot(DateTimeOffset now)
    {
        var items = new[]
        {
            new MobileWorkItem(
                "client-proof",
                "Fictional Engineering",
                "Review mobile proof package",
                MobileWorkStatus.Active,
                "Confirm evidence list before sending",
                now.AddHours(3),
                3,
                4,
                ["Alex Example", "Morgan Demo"],
                ["proof-summary.pdf", "validation-log.txt"]),
            new MobileWorkItem(
                "waiting-approval",
                "Example Door Systems",
                "OCR proof follow-up",
                MobileWorkStatus.Waiting,
                "Wait for client review",
                now.AddDays(1),
                2,
                3,
                ["Taylor Sample"],
                ["invoice-sample.pdf"]),
            new MobileWorkItem(
                "blocked-access",
                "Demo Cloud Systems",
                "Provider connection review",
                MobileWorkStatus.Blocked,
                "Resolve fictional access dependency",
                now.AddHours(6),
                1,
                3,
                ["Jordan Test"],
                []),
            new MobileWorkItem(
                "due-soon",
                "Sample Automation",
                "Prepare meeting evidence",
                MobileWorkStatus.DueSoon,
                "Review meeting context",
                now.AddMinutes(45),
                2,
                2,
                ["Casey Example"],
                ["meeting-notes.txt"])
        };

        var communications = new[]
        {
            new CommunicationCandidate(
                "gmail-proof",
                "Gmail",
                "Email",
                "Proof review update",
                "A fictional client has replied with review notes.",
                now.AddMinutes(-12),
                "Fresh",
                CommunicationReviewState.PendingReview,
                "Normalized Gmail candidate - provider payload excluded"),
            new CommunicationCandidate(
                "outlook-meeting",
                "Outlook",
                "Email",
                "Meeting time clarification",
                "A sanitized meeting clarification is ready for review.",
                now.AddMinutes(-40),
                "Fresh",
                CommunicationReviewState.PendingReview,
                "Normalized Outlook candidate - provider payload excluded"),
            new CommunicationCandidate(
                "teams-action",
                "Teams",
                "Chat",
                "Action item mentioned",
                "A fictional action item may link to the active work item.",
                now.AddHours(-2),
                "Stale",
                CommunicationReviewState.Conflict,
                "Normalized Teams candidate - provider payload excluded - duplicate/conflict detected")
        };

        var meetings = new[]
        {
            new MeetingContext(
                "meeting-proof",
                "Fictional client proof review",
                now.AddHours(2),
                ["Alex Example", "Morgan Demo"],
                "Review current evidence and capture only explicit next actions.",
                ["Confirm screenshot list", "Prepare review-safe summary"])
        };

        var evidence = new[]
        {
            new WorkEvidenceItem(
                "ev-1",
                "Build validation",
                true,
                "validation-log.txt",
                "Local trusted record",
                18420),
            new WorkEvidenceItem(
                "ev-2",
                "Proof summary",
                true,
                "proof-summary.pdf",
                "Linked file metadata",
                248120),
            new WorkEvidenceItem(
                "ev-3",
                "Client acknowledgement",
                false,
                "pending",
                "Review required",
                0)
        };

        return new WorkDashboardSnapshot(
            items,
            communications,
            meetings,
            evidence);
    }

    public IReadOnlyList<MobileWorkItem> FilterItems(
        WorkDashboardSnapshot snapshot,
        MobileWorkStatus? status,
        string? query)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        IEnumerable<MobileWorkItem> result = snapshot.Items;

        if (status is not null)
        {
            result = result.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            var normalized = query.Trim();

            result = result.Where(
                x => x.Client.Contains(
                        normalized,
                        StringComparison.OrdinalIgnoreCase) ||
                    x.Title.Contains(
                        normalized,
                        StringComparison.OrdinalIgnoreCase) ||
                    x.NextAction.Contains(
                        normalized,
                        StringComparison.OrdinalIgnoreCase));
        }

        return result
            .OrderBy(x => x.DueUtc)
            .ThenBy(x => x.Client, StringComparer.Ordinal)
            .ThenBy(x => x.Id, StringComparer.Ordinal)
            .ToArray();
    }

    public WorkReviewResult ReviewCommunication(
        string candidateId,
        CommunicationReviewState state,
        DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(candidateId);

        if (state is CommunicationReviewState.PendingReview or
            CommunicationReviewState.Conflict)
        {
            throw new ArgumentOutOfRangeException(
                nameof(state),
                "A deliberate review action is required.");
        }

        if (_reviews.TryGetValue(candidateId, out var existing) &&
            existing.State == state)
        {
            return existing;
        }

        var result = new WorkReviewResult(
            candidateId,
            state,
            now);

        _reviews[candidateId] = result;
        return result;
    }

    public MobileWorkItem CaptureFollowUp(
        MobileWorkItem item,
        string nextAction)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentException.ThrowIfNullOrWhiteSpace(nextAction);

        return item with
        {
            Status = MobileWorkStatus.Waiting,
            NextAction = nextAction.Trim()
        };
    }
}
