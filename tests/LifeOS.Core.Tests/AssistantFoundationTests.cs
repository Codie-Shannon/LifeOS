using Xunit;
using LifeOS.Core.Assistant;

namespace LifeOS.Core.Tests;

public sealed class AssistantFoundationTests
{
    private static AssistantConfiguration Enabled(params AssistantSourceType[] sources) => new(true,
        Enum.GetValues<AssistantSourceType>().Select(s => new AssistantSourcePermission(s, sources.Contains(s))).ToArray(), 10, 200);

    [Fact] public void Disabled_assistant_is_unavailable() => Assert.Equal("Unavailable", Service().Ask(new("status", AssistantConfiguration.Disabled)).Confidence);
    [Fact] public void No_enabled_source_is_unavailable() => Assert.Contains("incomplete", Service().Ask(new("status", Enabled())).Answer, StringComparison.OrdinalIgnoreCase);
    [Fact] public void Mutation_request_is_refused() => Assert.True(Service().Ask(new("send email to client", Enabled(AssistantSourceType.FollowUps))).Refused);
    [Fact] public void Unsupported_question_returns_insufficient_evidence() => Assert.Equal("Insufficient evidence", Service().Ask(new("quantum weather", Enabled(AssistantSourceType.FollowUps))).Confidence);
    [Fact] public void Disabled_source_is_not_queried() { var src=new FakeSource(AssistantSourceType.FollowUps); Service(src).Ask(new("client", Enabled(AssistantSourceType.WorkPipeline))); Assert.Equal(0,src.Calls); }
    [Fact] public void Enabled_source_is_queried() { var src=new FakeSource(AssistantSourceType.FollowUps); Service(src).Ask(new("client", Enabled(AssistantSourceType.FollowUps))); Assert.Equal(1,src.Calls); }
    [Fact] public void Context_is_bounded() { var response=Service().Ask(new(new string('x',201), Enabled(AssistantSourceType.FollowUps))); Assert.Equal("Unavailable",response.Confidence); }
    [Fact] public void Source_reference_is_preserved() { var response=Service(new FakeSource(AssistantSourceType.FollowUps,true)).Ask(new("client",Enabled(AssistantSourceType.FollowUps))); Assert.Equal("record-1",response.SourcesUsed.Single().RecordId); }
    [Fact] public void Response_distinguishes_fact_inference_and_uncertainty() { var response=Service(new FakeSource(AssistantSourceType.FollowUps,true)).Ask(new("client",Enabled(AssistantSourceType.FollowUps))); Assert.Contains(response.Statements,s=>s.Kind==AssistantStatementKind.Fact); Assert.Contains(response.Statements,s=>s.Kind==AssistantStatementKind.Inference); Assert.Contains(response.Statements,s=>s.Kind==AssistantStatementKind.Uncertainty); }
    [Fact] public void Suggestion_is_never_executable() { var response=Service(new FakeSource(AssistantSourceType.FollowUps,true)).Ask(new("client",Enabled(AssistantSourceType.FollowUps))); Assert.False(response.Suggestion!.IsExecutable); }
    [Fact] public void Maximum_record_count_is_enforced() { var response=Service(new ManySource()).Ask(new("client",new AssistantConfiguration(true,[new(AssistantSourceType.FollowUps,true)],2,200))); Assert.Equal(2,response.SourcesUsed.Count); }
    [Fact] public void Safety_boundary_is_always_visible() => Assert.Contains("Read-only",Service().Ask(new("send email",Enabled(AssistantSourceType.FollowUps))).SafetyBoundary,StringComparison.OrdinalIgnoreCase);

    private static AssistantService Service(params IAssistantEvidenceSource[] sources) => new(sources.Length==0?[new FakeSource(AssistantSourceType.FollowUps)]:sources,new LocalRuleAssistantAnswerProvider());

    private sealed class FakeSource(AssistantSourceType type, bool returnEvidence=false) : IAssistantEvidenceSource
    {
        public int Calls { get; private set; }
        public AssistantSourceType SourceType => type;
        public IReadOnlyList<AssistantEvidenceRecord> Retrieve(string question,int maximumRecords) { Calls++; return returnEvidence?[new("record-1",type,"Client record","Waiting for review.",DateTimeOffset.Parse("2026-07-14T00:00:00+12:00"),"test source")]:[]; }
    }
    private sealed class ManySource : IAssistantEvidenceSource
    {
        public AssistantSourceType SourceType=>AssistantSourceType.FollowUps;
        public IReadOnlyList<AssistantEvidenceRecord> Retrieve(string question,int maximumRecords)=>Enumerable.Range(1,10).Select(i=>new AssistantEvidenceRecord($"r{i}",SourceType,$"Record {i}","Summary",DateTimeOffset.UtcNow.AddMinutes(-i),"test")).ToArray();
    }
}

