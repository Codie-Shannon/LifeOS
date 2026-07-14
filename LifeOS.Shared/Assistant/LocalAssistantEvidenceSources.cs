using LifeOS.Core.Assistant;
using LifeOS.Shared.FollowUps;
using LifeOS.Shared.WorkPipeline;

namespace LifeOS.Shared.Assistant;

public static class LocalAssistantEvidenceSources
{
    private const string DemoName = "Northstar Systems";

    public static IReadOnlyList<IAssistantEvidenceSource> Create() =>
    [
        new FollowUpEvidenceSource(),
        new WorkPipelineEvidenceSource(),
        new StaticSummaryEvidenceSource(
            AssistantSourceType.CommandCentre,
            "command-centre-summary",
            "Command Centre summary",
            "Command Centre is a read-only summary source; use linked Follow-Up and Work Pipeline records for detail."),
        new StaticSummaryEvidenceSource(
            AssistantSourceType.Timeline,
            "timeline-summary",
            "Timeline summary",
            "Timeline evidence is available only when a question mentions dates, overdue work, waiting or recent activity."),
        new StaticSummaryEvidenceSource(
            AssistantSourceType.Evidence,
            "evidence-summary",
            "Evidence summary",
            "Evidence references are local summaries only; private file contents and connector caches are not exposed.")
    ];

    private sealed class FollowUpEvidenceSource : IAssistantEvidenceSource
    {
        public AssistantSourceType SourceType => AssistantSourceType.FollowUps;

        public IReadOnlyList<AssistantEvidenceRecord> Retrieve(
            string question,
            int maximumRecords)
        {
            if (IsUnsupportedDemoQuestion(question))
            {
                return [];
            }

            if (IsNorthstarDemoQuestion(question))
            {
                return NorthstarFollowUpRecords()
                    .Take(maximumRecords)
                    .ToArray();
            }

            return FollowUpStorage.Load()
                .Where(item => Matches(
                    question,
                    item.PersonOrOrganisation,
                    item.Context,
                    item.NextAction,
                    item.Status.ToString(),
                    item.Notes))
                .OrderByDescending(item => item.UpdatedAt)
                .Take(maximumRecords)
                .Select(item => new AssistantEvidenceRecord(
                    item.Id.ToString(),
                    SourceType,
                    item.PersonOrOrganisation,
                    $"Status {item.Status}; next action: {item.NextAction}; follow-up: {item.FollowUpDate?.ToString("yyyy-MM-dd") ?? "not set"}.",
                    new DateTimeOffset(item.UpdatedAt),
                    "LifeOS local Follow-Ups storage"))
                .ToArray();
        }

        private IReadOnlyList<AssistantEvidenceRecord> NorthstarFollowUpRecords() =>
        [
            new AssistantEvidenceRecord(
                "demo-followup-northstar-review",
                SourceType,
                DemoName,
                "Status Waiting; next action: wait for the fictional reviewer to confirm the sample invoice fields; follow-up: 2026-07-18.",
                new DateTimeOffset(2026, 7, 14, 16, 30, 0, TimeSpan.FromHours(12)),
                "LifeOS fictional Group 35 Follow-Ups evidence"),
            new AssistantEvidenceRecord(
                "demo-followup-northstar-scope",
                SourceType,
                "Northstar scope review",
                "Status Waiting; next action: confirm whether the fictional proof covers sales invoices only or also supplier bills; follow-up: not set.",
                new DateTimeOffset(2026, 7, 13, 11, 15, 0, TimeSpan.FromHours(12)),
                "LifeOS fictional Group 35 Follow-Ups evidence")
        ];
    }

    private sealed class WorkPipelineEvidenceSource : IAssistantEvidenceSource
    {
        public AssistantSourceType SourceType => AssistantSourceType.WorkPipeline;

        public IReadOnlyList<AssistantEvidenceRecord> Retrieve(
            string question,
            int maximumRecords)
        {
            if (IsUnsupportedDemoQuestion(question))
            {
                return [];
            }

            if (IsNorthstarDemoQuestion(question))
            {
                return NorthstarWorkPipelineRecords()
                    .Take(maximumRecords)
                    .ToArray();
            }

            return WorkPipelineStorage.Load()
                .Where(item => Matches(
                    question,
                    item.Title,
                    item.ContactName,
                    item.ClientOrCompany,
                    item.Status.ToString(),
                    item.WaitingOn,
                    item.NextAction,
                    item.RiskNote,
                    item.Notes))
                .OrderByDescending(item => item.UpdatedAt)
                .Take(maximumRecords)
                .Select(item => new AssistantEvidenceRecord(
                    item.Id.ToString(),
                    SourceType,
                    item.Title,
                    $"Status {item.Status}; waiting on: {item.WaitingOn}; next action: {item.NextAction}; risk: {item.RiskNote}.",
                    new DateTimeOffset(item.UpdatedAt),
                    "LifeOS local Work Pipeline storage"))
                .ToArray();
        }

        private IReadOnlyList<AssistantEvidenceRecord> NorthstarWorkPipelineRecords() =>
        [
            new AssistantEvidenceRecord(
                "demo-pipeline-northstar-ocr",
                SourceType,
                "Northstar invoice OCR proof",
                "Status Waiting; waiting on: fictional sample-field confirmation; next action: review extracted invoice number, date and total fields; risk: building beyond the agreed fictional proof scope.",
                new DateTimeOffset(2026, 7, 14, 16, 29, 0, TimeSpan.FromHours(12)),
                "LifeOS fictional Group 35 Work Pipeline evidence"),
            new AssistantEvidenceRecord(
                "demo-pipeline-northstar-evidence",
                SourceType,
                "Northstar proof evidence pack",
                "Status Active; waiting on: screenshot review; next action: capture one fictional source-backed answer and one insufficient-evidence response; risk: none recorded.",
                new DateTimeOffset(2026, 7, 14, 15, 55, 0, TimeSpan.FromHours(12)),
                "LifeOS fictional Group 35 Work Pipeline evidence"),
            new AssistantEvidenceRecord(
                "demo-pipeline-northstar-boundary",
                SourceType,
                "Northstar automation boundary",
                "Status Active; waiting on: safety verification; next action: confirm the assistant cannot send, approve, execute or mutate state; risk: any executable suggestion would fail the Group 35 boundary.",
                new DateTimeOffset(2026, 7, 14, 15, 40, 0, TimeSpan.FromHours(12)),
                "LifeOS fictional Group 35 Work Pipeline evidence")
        ];
    }

    private sealed class StaticSummaryEvidenceSource(
        AssistantSourceType sourceType,
        string id,
        string title,
        string summary) : IAssistantEvidenceSource
    {
        public AssistantSourceType SourceType => sourceType;

        public IReadOnlyList<AssistantEvidenceRecord> Retrieve(
            string question,
            int maximumRecords)
        {
            if (IsUnsupportedDemoQuestion(question))
            {
                return [];
            }

            if (IsNorthstarDemoQuestion(question))
            {
                return sourceType switch
                {
                    AssistantSourceType.Timeline =>
                    [
                        new AssistantEvidenceRecord(
                            "demo-timeline-northstar",
                            sourceType,
                            "Northstar timeline summary",
                            "The fictional proof entered Waiting after the sample-field review was requested. No approval date or payment amount is recorded.",
                            new DateTimeOffset(2026, 7, 14, 16, 28, 0, TimeSpan.FromHours(12)),
                            "LifeOS fictional Group 35 Timeline evidence")
                    ],
                    AssistantSourceType.Evidence =>
                    [
                        new AssistantEvidenceRecord(
                            "demo-evidence-northstar",
                            sourceType,
                            "Northstar evidence summary",
                            "The fictional evidence confirms a pending review and bounded proof scope. It does not contain an approval date, payment promise or confirmed amount.",
                            new DateTimeOffset(2026, 7, 14, 16, 27, 0, TimeSpan.FromHours(12)),
                            "LifeOS fictional Group 35 Evidence summary")
                    ],
                    AssistantSourceType.CommandCentre =>
                    [
                        new AssistantEvidenceRecord(
                            "demo-command-northstar",
                            sourceType,
                            "Northstar command summary",
                            "The fictional proof is waiting rather than blocked. The recorded next step is manual sample-field review.",
                            new DateTimeOffset(2026, 7, 14, 16, 26, 0, TimeSpan.FromHours(12)),
                            "LifeOS fictional Group 35 Command Centre summary")
                    ],
                    _ => []
                };
            }

            var keywords = sourceType switch
            {
                AssistantSourceType.CommandCentre =>
                    new[] { "command", "pressure", "priority", "today" },
                AssistantSourceType.Timeline =>
                    new[] { "timeline", "date", "overdue", "waiting", "recent" },
                AssistantSourceType.Evidence =>
                    new[] { "evidence", "proof", "source", "record" },
                _ => []
            };

            return keywords.Any(keyword =>
                    question.Contains(
                        keyword,
                        StringComparison.OrdinalIgnoreCase))
                ?
                [
                    new AssistantEvidenceRecord(
                        id,
                        sourceType,
                        title,
                        summary,
                        DateTimeOffset.Now,
                        $"LifeOS local {sourceType} summary")
                ]
                : [];
        }
    }

    private static bool IsUnsupportedDemoQuestion(string question) =>
        question.Contains(
            "Zephyr Quill",
            StringComparison.OrdinalIgnoreCase);

    private static bool IsNorthstarDemoQuestion(string question) =>
        question.Contains(
            "Northstar",
            StringComparison.OrdinalIgnoreCase);

    private static bool Matches(
        string question,
        params string?[] values)
    {
        var terms = question
            .Split(
                [' ', ',', '.', '?', '!', ':', ';'],
                StringSplitOptions.RemoveEmptyEntries)
            .Where(term => term.Length >= 3)
            .Where(term => !new[]
            {
                "what",
                "which",
                "show",
                "tell",
                "about",
                "from",
                "with",
                "this",
                "that",
                "does",
                "have"
            }.Contains(
                term,
                StringComparer.OrdinalIgnoreCase))
            .ToArray();

        if (terms.Length == 0)
        {
            return false;
        }

        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Any(value => terms.Any(term =>
                value!.Contains(
                    term,
                    StringComparison.OrdinalIgnoreCase)));
    }
}
