using LifeOS.Core.Assistant;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class AssistantAnswerQualityTests
{
    private static readonly DateTimeOffset Now = new(2026,7,14,17,0,0,TimeSpan.FromHours(12));

    [Fact] public void Intent_classifies_waiting_on() => Assert.Equal(AssistantIntent.WaitingOn, AssistantService.ClassifyIntent("What am I waiting on from Northstar?"));
    [Fact] public void Intent_classifies_work_recorded() => Assert.Equal(AssistantIntent.WorkRecorded, AssistantService.ClassifyIntent("What work did I record this week?"));
    [Fact] public void Intent_classifies_conflict() => Assert.Equal(AssistantIntent.ConflictCheck, AssistantService.ClassifyIntent("Do records disagree about project status?"));

    [Fact]
    public void Disabled_source_is_not_searched_and_is_reported()
    {
        var configuration = Configuration([AssistantSourceType.Projects]);
        var response = Service().Ask(new("Why is Project Zephyr blocked?", configuration, Now));
        Assert.DoesNotContain(AssistantSourceType.WorkPipeline, response.SourcesSearched!);
        Assert.Contains(AssistantSourceType.WorkPipeline, response.DisabledRelevantSources!);
    }

    [Fact]
    public void Direct_evidence_outranks_summary()
    {
        var response = Service().Ask(new("What am I waiting on from Northstar?", Configuration(Enum.GetValues<AssistantSourceType>()), Now));
        var used = response.RecordsConsidered!.Where(r => r.Used).ToArray();
        Assert.NotEmpty(used);
        Assert.Equal(AssistantTrustLevel.Direct, used[0].Record.Trust);
    }

    [Fact]
    public void Conflict_is_visible_and_lowers_confidence()
    {
        var response = Service().Ask(new("Do any records disagree about Project Zephyr Quill status?", Configuration(Enum.GetValues<AssistantSourceType>()), Now));
        Assert.NotEmpty(response.Conflicts!);
        Assert.Equal("Reduced", response.Confidence);
        Assert.Contains(response.Statements, s => s.Kind == AssistantStatementKind.Conflict);
    }

    [Fact]
    public void Stale_record_is_warned()
    {
        var response = Service().Ask(new("Why is Project Zephyr Quill blocked?", Configuration(Enum.GetValues<AssistantSourceType>()), Now));
        Assert.Contains(response.Statements, s => s.Kind == AssistantStatementKind.StaleData);
    }

    [Fact]
    public void Unsupported_question_does_not_fall_through()
    {
        var response = Service().Ask(new("What colour is the Project Zephyr Quill logo?", Configuration(Enum.GetValues<AssistantSourceType>()), Now));
        Assert.Empty(response.SourcesUsed);
        Assert.Equal("Insufficient evidence", response.Confidence);
    }

    [Fact]
    public void Retrieval_is_bounded()
    {
        var configuration = Configuration(Enum.GetValues<AssistantSourceType>()) with { MaximumRecords = 3 };
        var response = Service().Ask(new("What am I waiting on from Northstar?", configuration, Now));
        Assert.True(response.RecordsConsidered!.Count <= 3);
    }

    [Fact]
    public void Mutation_request_remains_refused()
    {
        var response = Service().Ask(new("Approve the Northstar invoice", Configuration(Enum.GetValues<AssistantSourceType>()), Now));
        Assert.True(response.Refused);
        Assert.Empty(response.SourcesUsed);
    }

    [Fact]
    public void Expanded_registry_has_fourteen_sources() => Assert.Equal(14, Enum.GetValues<AssistantSourceType>().Length);

    private static AssistantService Service() => new(TestSources(), new LocalRuleAssistantAnswerProvider());

    private static AssistantConfiguration Configuration(IEnumerable<AssistantSourceType> enabled) => new(true,
        Enum.GetValues<AssistantSourceType>().Select(s => new AssistantSourcePermission(s, enabled.Contains(s))).ToArray());

    private static IEnumerable<IAssistantEvidenceSource> TestSources() => Enum.GetValues<AssistantSourceType>().Select(s => new TestSource(s));

    private sealed class TestSource(AssistantSourceType source) : IAssistantEvidenceSource
    {
        public AssistantSourceType SourceType => source;
        public IReadOnlyList<AssistantEvidenceRecord> Retrieve(string question, int maximumRecords)
        {
            if (question.Contains("logo", StringComparison.OrdinalIgnoreCase)) return [];
            var timestamp = source == AssistantSourceType.WorkPipeline ? Now.AddDays(-70) : Now.AddHours(-1);
            var status = source switch { AssistantSourceType.Projects => "Active", AssistantSourceType.WorkPipeline => "Blocked", _ => "Waiting" };
            var trust = source == AssistantSourceType.CommandCentre ? AssistantTrustLevel.Summary : AssistantTrustLevel.Direct;
            return [new($"id-{source}", source, $"Northstar Zephyr {source}", $"Matching {source} record.", timestamp, "fictional test", trust, "zephyr-quill", status, IsFictional:true)];
        }
    }
}
