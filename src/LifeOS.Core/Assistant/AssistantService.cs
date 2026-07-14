using System.Globalization;
using System.Text.RegularExpressions;

namespace LifeOS.Core.Assistant;

public sealed class AssistantService
{
    private static readonly string[] MutationTerms =
    [
        "send email", "reply to", "forward", "delete", "approve", "confirm", "execute", "run script",
        "powershell", "launch process", "pay ", "create event", "update calendar", "change status", "mark complete"
    ];

    private readonly IReadOnlyDictionary<AssistantSourceType, IAssistantEvidenceSource> _sources;
    private readonly IAssistantAnswerProvider _answerProvider;

    public AssistantService(IEnumerable<IAssistantEvidenceSource> sources, IAssistantAnswerProvider answerProvider)
    {
        ArgumentNullException.ThrowIfNull(sources);
        _answerProvider = answerProvider ?? throw new ArgumentNullException(nameof(answerProvider));
        _sources = sources.GroupBy(source => source.SourceType).ToDictionary(group => group.Key, group => group.First());
    }

    public AssistantResponse Ask(AssistantRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        const string boundary = "Read-only assistant: no durable memory, tools, execution, approval, confirmation, external writes or LifeOS state mutation.";

        if (!request.Configuration.Enabled)
            return Unavailable("Assistant is disabled. Enable it explicitly before asking a question.", boundary);
        if (!request.Configuration.Sources.Any(source => source.Enabled))
            return Unavailable("Assistant configuration is incomplete because no approved source is enabled.", boundary);

        var question = (request.Question ?? string.Empty).Trim();
        if (question.Length == 0)
            return Unavailable("Enter a question about approved local LifeOS state.", boundary);
        if (question.Length > request.Configuration.MaximumQuestionCharacters)
            return Unavailable($"Question exceeds the bounded {request.Configuration.MaximumQuestionCharacters}-character request limit.", boundary);

        if (MutationTerms.Any(term => question.Contains(term, StringComparison.OrdinalIgnoreCase)))
        {
            return new AssistantResponse(
                "I cannot perform or confirm that request. This assistant is read-only and cannot reach execution, approval, orchestration, connector or mutation handlers.",
                [],
                [new(AssistantStatementKind.Fact, "No LifeOS state or external system was changed."),
                 new(AssistantStatementKind.Uncertainty, "A separate existing review path may be appropriate, but the assistant cannot start it automatically.")],
                "High", null, true, boundary, ClassifyIntent(question), [], [], [], [],
                "High confidence in the refusal because the request crosses an explicit safety boundary.");
        }

        var now = request.Now ?? DateTimeOffset.Now;
        var intent = ClassifyIntent(question);
        var relevant = RelevantSources(intent);
        var enabled = request.Configuration.Sources.Where(p => p.Enabled).Select(p => p.Source).Distinct().ToArray();
        var searched = enabled.Where(_sources.ContainsKey).ToArray();
        var disabledRelevant = relevant.Where(source => !enabled.Contains(source)).ToArray();
        var perSourceLimit = Math.Max(1, Math.Min(10, request.Configuration.MaximumRecords / Math.Max(1, searched.Length) + 1));

        var candidates = searched
            .SelectMany(source => _sources[source].Retrieve(question, perSourceLimit))
            .GroupBy(record => (record.Source, record.RecordId))
            .Select(group => group.First())
            .Take(request.Configuration.MaximumRecords * 2)
            .ToArray();

        var ranked = Rank(question, intent, candidates, now)
            .Take(request.Configuration.MaximumRecords)
            .ToArray();
        var usedCount = Math.Min(8, ranked.Count(item => item.Score > 0));
        ranked = ranked.Select((item, index) => item with
        {
            Used = index < usedCount && item.Score > 0,
            Reason = index < usedCount && item.Score > 0
                ? SelectionReason(item.Record, item.Score, now)
                : item.Score <= 0 ? "Excluded: no direct match to the classified intent." : "Excluded: bounded answer context retained higher-ranked records."
        }).ToArray();

        var used = ranked.Where(item => item.Used).Select(item => item.Record).ToArray();
        if (used.Length == 0)
        {
            return new AssistantResponse(
                "The approved local sources do not contain enough trustworthy evidence to answer that question.",
                [],
                [new(AssistantStatementKind.MissingData, "No directly matching approved source records were retrieved."),
                 new(AssistantStatementKind.Uncertainty, "The answer may exist outside enabled LifeOS sources, but this assistant will not search elsewhere.")],
                "Insufficient evidence", null, false, boundary, intent, searched, ranked, [], disabledRelevant,
                "No direct supporting records survived bounded relevance ranking.");
        }

        var conflicts = DetectConflicts(used);
        var generated = _answerProvider.Generate(question, intent, ranked, conflicts, searched, disabledRelevant, now);
        return generated with { SafetyBoundary = boundary };
    }

    public static AssistantIntent ClassifyIntent(string question)
    {
        var q = question.ToLowerInvariant();
        if (q.Contains("disagree") || q.Contains("conflict")) return AssistantIntent.ConflictCheck;
        if (q.Contains("missing") || q.Contains("confident")) return AssistantIntent.MissingEvidence;
        if (q.Contains("work did") || q.Contains("timesheet") || q.Contains("this week") || q.Contains("hours")) return AssistantIntent.WorkRecorded;
        if (q.Contains("invoice") || q.Contains("payment") || q.Contains("money") || q.Contains("amount")) return AssistantIntent.MoneyAttention;
        if (q.Contains("waiting") || q.Contains("waiting on")) return AssistantIntent.WaitingOn;
        if (q.Contains("project") || q.Contains("blocked") || q.Contains("status")) return AssistantIntent.ProjectStatus;
        if (q.Contains("changed today") || q.Contains("what changed") || q.Contains("today")) return AssistantIntent.WhatChanged;
        return AssistantIntent.General;
    }

    private static IReadOnlyList<AssistantSourceType> RelevantSources(AssistantIntent intent) => intent switch
    {
        AssistantIntent.WaitingOn => [AssistantSourceType.WaitingOn, AssistantSourceType.FollowUps, AssistantSourceType.WorkPipeline, AssistantSourceType.Relationships],
        AssistantIntent.ProjectStatus or AssistantIntent.ConflictCheck => [AssistantSourceType.Projects, AssistantSourceType.WorkPipeline, AssistantSourceType.Timeline, AssistantSourceType.Evidence],
        AssistantIntent.MoneyAttention => [AssistantSourceType.MoneyPressure, AssistantSourceType.Receipts, AssistantSourceType.Evidence, AssistantSourceType.WorkPipeline],
        AssistantIntent.WorkRecorded => [AssistantSourceType.WorkSessions, AssistantSourceType.Timesheets, AssistantSourceType.Projects],
        AssistantIntent.WhatChanged => [AssistantSourceType.DailyState, AssistantSourceType.Agenda, AssistantSourceType.Timeline, AssistantSourceType.CommandCentre],
        _ => Enum.GetValues<AssistantSourceType>()
    };

    private static IEnumerable<AssistantRankedRecord> Rank(string question, AssistantIntent intent, IEnumerable<AssistantEvidenceRecord> records, DateTimeOffset now)
    {
        var terms = Tokenise(question).ToArray();
        return records.Select(record =>
        {
            var text = $"{record.Title} {record.Summary} {record.EntityKey} {record.Status}";
            var matches = terms.Count(term => text.Contains(term, StringComparison.OrdinalIgnoreCase));
            var intentBoost = RelevantSources(intent).Contains(record.Source) ? 24 : 0;
            var trust = (int)record.Trust * 12;
            var ageDays = Math.Max(0, (now - record.Timestamp).TotalDays);
            var freshness = ageDays <= 2 ? 20 : ageDays <= 14 ? 12 : ageDays <= 45 ? 4 : -12;
            var directness = matches * 18;
            var score = directness + intentBoost + trust + freshness;
            return new AssistantRankedRecord(record, score, false, string.Empty);
        }).OrderByDescending(item => item.Score).ThenByDescending(item => item.Record.Timestamp);
    }

    private static string SelectionReason(AssistantEvidenceRecord record, double score, DateTimeOffset now)
    {
        var age = (now - record.Timestamp).TotalDays;
        var freshness = age <= 2 ? "fresh" : age <= 14 ? "recent" : age <= 45 ? "older" : "stale";
        return $"Selected: direct {record.Source} evidence; trust {record.Trust}; {freshness}; rank {score:0}.";
    }

    private static IReadOnlyList<AssistantConflict> DetectConflicts(IReadOnlyList<AssistantEvidenceRecord> used)
    {
        var conflicts = new List<AssistantConflict>();
        foreach (var group in used.Where(r => !string.IsNullOrWhiteSpace(r.EntityKey)).GroupBy(r => r.EntityKey!, StringComparer.OrdinalIgnoreCase))
        {
            var statuses = group.Where(r => !string.IsNullOrWhiteSpace(r.Status)).GroupBy(r => r.Status!, StringComparer.OrdinalIgnoreCase).ToArray();
            if (statuses.Length > 1)
            {
                var first = statuses[0].First(); var second = statuses[1].First();
                conflicts.Add(new("Status", first, second, $"'{first.Title}' reports {first.Status}, while '{second.Title}' reports {second.Status}."));
            }
            var amounts = group.Where(r => r.Amount.HasValue).GroupBy(r => r.Amount!.Value).ToArray();
            if (amounts.Length > 1)
            {
                var first = amounts[0].First(); var second = amounts[1].First();
                conflicts.Add(new("Amount", first, second, $"Records disagree: {first.Amount:C} versus {second.Amount:C}."));
            }
        }
        return conflicts;
    }

    private static IEnumerable<string> Tokenise(string question) => Regex.Split(question.ToLowerInvariant(), "[^a-z0-9]+")
        .Where(term => term.Length >= 3 && !StopWords.Contains(term));

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    { "what", "which", "show", "tell", "about", "from", "with", "this", "that", "does", "have", "why", "the", "and", "for", "are", "did", "any" };

    private static AssistantResponse Unavailable(string message, string boundary) => new(
        message, [], [new(AssistantStatementKind.MissingData, message)], "Unavailable", null, false, boundary,
        ConfidenceReason: "Assistant configuration is unavailable or incomplete.");
}

public sealed class LocalRuleAssistantAnswerProvider : IAssistantAnswerProvider
{
    public AssistantResponse Generate(string question, AssistantIntent intent, IReadOnlyList<AssistantRankedRecord> ranked,
        IReadOnlyList<AssistantConflict> conflicts, IReadOnlyList<AssistantSourceType> searched,
        IReadOnlyList<AssistantSourceType> disabledRelevant, DateTimeOffset now)
    {
        var used = ranked.Where(item => item.Used).Select(item => item.Record).ToArray();
        var direct = used.Where(r => r.Trust == AssistantTrustLevel.Direct).ToArray();
        var stale = used.Where(r => (now - r.Timestamp).TotalDays > 45).ToArray();
        var answer = $"Intent: {intent}. I used {used.Length} of {ranked.Count} bounded candidate records across {used.Select(r => r.Source).Distinct().Count()} source(s). " +
                     string.Join(" ", used.Take(4).Select(r => $"{r.Title}: {r.Summary}"));

        var statements = new List<AssistantStatement>
        {
            new(AssistantStatementKind.Fact, $"{direct.Length} direct record(s) support the answer."),
            new(AssistantStatementKind.Inference, "Cross-source ordering reflects relevance, trust and freshness; it is not a new stored fact."),
            new(AssistantStatementKind.Uncertainty, "Only enabled approved sources were searched.")
        };
        if (conflicts.Count > 0) statements.Add(new(AssistantStatementKind.Conflict, $"{conflicts.Count} conflicting field(s) require manual review."));
        if (stale.Length > 0) statements.Add(new(AssistantStatementKind.StaleData, $"{stale.Length} used record(s) are older than 45 days."));
        if (disabledRelevant.Count > 0) statements.Add(new(AssistantStatementKind.MissingData, $"Relevant disabled sources: {string.Join(", ", disabledRelevant)}."));

        var confidence = conflicts.Count > 0 || stale.Length > 0 || disabledRelevant.Count > 0 ? "Reduced" : direct.Length >= 2 ? "High" : "Moderate";
        var reason = conflicts.Count > 0 ? "Reduced because records conflict." : stale.Length > 0 ? "Reduced because stale evidence was required." : disabledRelevant.Count > 0 ? "Reduced because relevant sources are disabled." : "Supported by fresh direct evidence from multiple approved sources.";
        var suggestion = new AssistantSuggestion("Review the highest-ranked direct record", ranked.First(i => i.Used).Reason,
            "Open the named LifeOS module manually; no action is created or executed.");

        return new AssistantResponse(answer, used, statements, confidence, suggestion, false, string.Empty, intent,
            searched, ranked, conflicts, disabledRelevant, reason);
    }
}
