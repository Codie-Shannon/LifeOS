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
        var boundary = "Read-only assistant: answers and suggestions cannot execute, approve, confirm or mutate LifeOS state.";

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
                "High",
                null,
                true,
                boundary);
        }

        var perSourceLimit = Math.Max(1, request.Configuration.MaximumRecords / Math.Max(1, request.Configuration.Sources.Count(s => s.Enabled)));
        var evidence = request.Configuration.Sources
            .Where(permission => permission.Enabled)
            .SelectMany(permission => _sources.TryGetValue(permission.Source, out var source)
                ? source.Retrieve(question, perSourceLimit)
                : [])
            .OrderByDescending(record => record.Timestamp)
            .Take(request.Configuration.MaximumRecords)
            .ToArray();

        if (evidence.Length == 0)
        {
            return new AssistantResponse(
                "The approved local sources do not contain enough trustworthy evidence to answer that question.",
                [],
                [new(AssistantStatementKind.MissingData, "No matching approved source records were retrieved."),
                 new(AssistantStatementKind.Uncertainty, "The answer may exist outside the currently enabled LifeOS sources, but this assistant will not search elsewhere.")],
                "Insufficient evidence",
                null,
                false,
                boundary);
        }

        var generated = _answerProvider.Generate(question, evidence);
        return generated with { SafetyBoundary = boundary };
    }

    private static AssistantResponse Unavailable(string message, string boundary) => new(
        message, [], [new(AssistantStatementKind.MissingData, message)], "Unavailable", null, false, boundary);
}

public sealed class LocalRuleAssistantAnswerProvider : IAssistantAnswerProvider
{
    public AssistantResponse Generate(string question, IReadOnlyList<AssistantEvidenceRecord> evidence)
    {
        var answer = evidence.Count == 1
            ? $"The strongest matching local record is '{evidence[0].Title}'. {evidence[0].Summary}"
            : $"I found {evidence.Count} matching records across {evidence.Select(item => item.Source).Distinct().Count()} approved source(s). " +
              string.Join(" ", evidence.Take(4).Select(item => $"{item.Title}: {item.Summary}"));

        var statements = new List<AssistantStatement>
        {
            new(AssistantStatementKind.Fact, $"{evidence.Count} approved local record(s) support this answer."),
            new(AssistantStatementKind.Inference, "Ordering by most recent evidence makes the first records the best current explanation, but this remains an interpretation."),
            new(AssistantStatementKind.Uncertainty, "Records outside enabled sources were not consulted.")
        };

        var suggestion = new AssistantSuggestion(
            "Review the highest-priority matching record",
            "The evidence indicates that the newest matching item is the clearest place for the user to inspect next.",
            "Open the relevant LifeOS module manually; no action has been created or executed.");

        return new AssistantResponse(answer, evidence, statements, evidence.Count >= 2 ? "Moderate" : "Limited", suggestion, false, string.Empty);
    }
}
