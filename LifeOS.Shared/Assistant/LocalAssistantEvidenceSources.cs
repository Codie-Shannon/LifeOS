using LifeOS.Core.Assistant;
using LifeOS.Shared.FollowUps;
using LifeOS.Shared.WorkPipeline;

namespace LifeOS.Shared.Assistant;

public static class LocalAssistantEvidenceSources
{
    public static IReadOnlyList<IAssistantEvidenceSource> Create() =>
    [
        new FollowUpEvidenceSource(),
        new WorkPipelineEvidenceSource(),
        new StaticSummaryEvidenceSource(AssistantSourceType.CommandCentre, "command-centre-summary", "Command Centre summary", "Command Centre is a read-only summary source; use linked Follow-Up and Work Pipeline records for detail."),
        new StaticSummaryEvidenceSource(AssistantSourceType.Timeline, "timeline-summary", "Timeline summary", "Timeline evidence is available only when a question mentions dates, overdue work, waiting or recent activity."),
        new StaticSummaryEvidenceSource(AssistantSourceType.Evidence, "evidence-summary", "Evidence summary", "Evidence references are local summaries only; private file contents and connector caches are not exposed.")
    ];

    private sealed class FollowUpEvidenceSource : IAssistantEvidenceSource
    {
        public AssistantSourceType SourceType => AssistantSourceType.FollowUps;
        public IReadOnlyList<AssistantEvidenceRecord> Retrieve(string question, int maximumRecords) => FollowUpStorage.Load()
            .Where(item => Matches(question, item.PersonOrOrganisation, item.Context, item.NextAction, item.Status.ToString(), item.Notes))
            .OrderByDescending(item => item.UpdatedAt)
            .Take(maximumRecords)
            .Select(item => new AssistantEvidenceRecord(item.Id.ToString(), SourceType, item.PersonOrOrganisation,
                $"Status {item.Status}; next action: {item.NextAction}; follow-up: {item.FollowUpDate?.ToString("yyyy-MM-dd") ?? "not set"}.",
                new DateTimeOffset(item.UpdatedAt), "LifeOS local Follow-Ups storage"))
            .ToArray();
    }

    private sealed class WorkPipelineEvidenceSource : IAssistantEvidenceSource
    {
        public AssistantSourceType SourceType => AssistantSourceType.WorkPipeline;
        public IReadOnlyList<AssistantEvidenceRecord> Retrieve(string question, int maximumRecords) => WorkPipelineStorage.Load()
            .Where(item => Matches(question, item.Title, item.ContactName, item.ClientOrCompany, item.Status.ToString(), item.WaitingOn, item.NextAction, item.RiskNote, item.Notes))
            .OrderByDescending(item => item.UpdatedAt)
            .Take(maximumRecords)
            .Select(item => new AssistantEvidenceRecord(item.Id.ToString(), SourceType, item.Title,
                $"Status {item.Status}; waiting on: {item.WaitingOn}; next action: {item.NextAction}; risk: {item.RiskNote}.",
                new DateTimeOffset(item.UpdatedAt), "LifeOS local Work Pipeline storage"))
            .ToArray();
    }

    private sealed class StaticSummaryEvidenceSource(AssistantSourceType sourceType, string id, string title, string summary) : IAssistantEvidenceSource
    {
        public AssistantSourceType SourceType => sourceType;
        public IReadOnlyList<AssistantEvidenceRecord> Retrieve(string question, int maximumRecords)
        {
            var keywords = sourceType switch
            {
                AssistantSourceType.CommandCentre => new[] { "command", "pressure", "priority", "today" },
                AssistantSourceType.Timeline => new[] { "timeline", "date", "overdue", "waiting", "recent" },
                AssistantSourceType.Evidence => new[] { "evidence", "proof", "source", "record" },
                _ => []
            };
            return keywords.Any(k => question.Contains(k, StringComparison.OrdinalIgnoreCase))
                ? [new AssistantEvidenceRecord(id, sourceType, title, summary, DateTimeOffset.Now, $"LifeOS local {sourceType} summary")]
                : [];
        }
    }

    private static bool Matches(string question, params string?[] values)
    {
        var terms = question.Split([' ', ',', '.', '?', '!', ':', ';'], StringSplitOptions.RemoveEmptyEntries)
            .Where(term => term.Length >= 3)
            .Where(term => !new[] { "what", "which", "show", "tell", "about", "from", "with", "this", "that", "does", "have" }.Contains(term, StringComparer.OrdinalIgnoreCase))
            .ToArray();
        if (terms.Length == 0) return false;
        return values.Where(v => !string.IsNullOrWhiteSpace(v)).Any(value => terms.Any(term => value!.Contains(term, StringComparison.OrdinalIgnoreCase)));
    }
}
