namespace LifeOS.Core.Assistant;

public enum AssistantSourceType
{
    CommandCentre,
    FollowUps,
    WorkPipeline,
    Timeline,
    Evidence
}

public enum AssistantStatementKind
{
    Fact,
    Inference,
    MissingData,
    Uncertainty
}

public sealed record AssistantSourcePermission(AssistantSourceType Source, bool Enabled);

public sealed record AssistantConfiguration(
    bool Enabled,
    IReadOnlyList<AssistantSourcePermission> Sources,
    int MaximumRecords = 20,
    int MaximumQuestionCharacters = 500)
{
    public static AssistantConfiguration Disabled { get; } = new(false,
        Enum.GetValues<AssistantSourceType>().Select(source => new AssistantSourcePermission(source, false)).ToArray());

    public bool IsSourceEnabled(AssistantSourceType source) =>
        Sources.Any(permission => permission.Source == source && permission.Enabled);
}

public sealed record AssistantEvidenceRecord(
    string RecordId,
    AssistantSourceType Source,
    string Title,
    string Summary,
    DateTimeOffset Timestamp,
    string Provenance);

public sealed record AssistantStatement(AssistantStatementKind Kind, string Text);

public sealed record AssistantSuggestion(string Title, string Rationale, string ReviewRoute)
{
    public bool IsExecutable => false;
}

public sealed record AssistantRequest(string Question, AssistantConfiguration Configuration);

public sealed record AssistantResponse(
    string Answer,
    IReadOnlyList<AssistantEvidenceRecord> SourcesUsed,
    IReadOnlyList<AssistantStatement> Statements,
    string Confidence,
    AssistantSuggestion? Suggestion,
    bool Refused,
    string SafetyBoundary);

public interface IAssistantEvidenceSource
{
    AssistantSourceType SourceType { get; }
    IReadOnlyList<AssistantEvidenceRecord> Retrieve(string question, int maximumRecords);
}

public interface IAssistantAnswerProvider
{
    AssistantResponse Generate(string question, IReadOnlyList<AssistantEvidenceRecord> evidence);
}
