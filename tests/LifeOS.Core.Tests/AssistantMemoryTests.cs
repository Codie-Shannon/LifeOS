using LifeOS.Core.AssistantMemory;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class AssistantMemoryTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 14, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Memory_identity_type_scope_sensitivity_and_status_are_explicit()
    {
        var saved = Save(Service(), "Use evidence-first formatting.", AssistantMemoryType.Preference, Scope(), AssistantMemorySensitivity.Standard);
        Assert.NotEqual(Guid.Empty, saved.MemoryId);
        Assert.Equal(AssistantMemoryType.Preference, saved.Type);
        Assert.Equal(AssistantMemoryScopeType.Project, saved.Scope.Type);
        Assert.Equal(AssistantMemoryStatus.Active, saved.Status);
    }

    [Fact]
    public void No_automatic_creation_preview_and_confirmation_required()
    {
        var service = Service();
        var proposal = Propose(service, "Use short summaries.");
        Assert.Empty(service.List());
        service.Cancel(proposal.ProposalId);
        Assert.Empty(service.List());
    }

    [Fact]
    public void Cancellation_creates_no_record()
    {
        var service = Service(); var proposal = Propose(service, "Use short summaries.");
        service.Cancel(proposal.ProposalId);
        Assert.Empty(service.List());
    }

    [Fact]
    public void Duplicate_candidates_require_explicit_resolution()
    {
        var service = Service(); Save(service, "Use short evidence-first summaries.", AssistantMemoryType.Preference, Scope(), AssistantMemorySensitivity.Standard);
        var proposal = Propose(service, "Use short evidence-first summaries.");
        Assert.Single(proposal.DuplicateCandidates);
        Assert.Throws<InvalidOperationException>(() => service.Confirm(proposal.ProposalId, "Codie", true, Now));
    }

    [Fact]
    public void Conflicting_candidates_are_visible_and_not_silently_merged()
    {
        var service = Service(); Save(service, "Project Zephyr summaries must be detailed and long.", AssistantMemoryType.Preference, Scope(), AssistantMemorySensitivity.Standard);
        var proposal = Propose(service, "Project Zephyr summaries must be short and detailed.");
        Assert.NotEmpty(proposal.ConflictingCandidates);
    }

    [Fact]
    public void Scope_filtering_excludes_unrelated_project_memory()
    {
        var service = Service(); Save(service, "Zephyr uses evidence-first summaries.", AssistantMemoryType.ProjectContext, Scope(), AssistantMemorySensitivity.Standard);
        Assert.Empty(service.Retrieve(new AssistantMemoryQuery("Zephyr summary", Project: "Northstar"), Now));
        Assert.Single(service.Retrieve(new AssistantMemoryQuery("Zephyr summary", Project: "Zephyr Quill"), Now));
    }

    [Fact]
    public void Expired_memory_is_excluded_immediately()
    {
        var service = Service();
        var proposal = service.Propose("Expired", "Zephyr uses evidence-first summaries.", AssistantMemoryType.Preference, Scope(), AssistantMemorySensitivity.Standard, Origin(), Now.AddMinutes(1), now: Now);
        service.Confirm(proposal.ProposalId, "Codie", true, Now);
        Assert.Empty(service.Retrieve(new AssistantMemoryQuery("Zephyr summaries", Project: "Zephyr Quill"), Now.AddMinutes(2)));
        Assert.Equal(AssistantMemoryStatus.Expired, Assert.Single(service.List()).Status);
    }

    [Fact]
    public void Revoked_memory_is_excluded_immediately()
    {
        var service = Service(); var saved = Save(service, "Zephyr uses evidence-first summaries.", AssistantMemoryType.Preference, Scope(), AssistantMemorySensitivity.Standard);
        service.Revoke(saved.MemoryId, "Codie", Now.AddMinutes(1));
        Assert.Empty(service.Retrieve(new AssistantMemoryQuery("Zephyr summaries", Project: "Zephyr Quill"), Now.AddMinutes(2)));
    }

    [Fact]
    public void Memory_use_is_disclosed_and_usage_can_be_audited()
    {
        var service = Service(); Save(service, "Zephyr uses evidence-first summaries.", AssistantMemoryType.Preference, Scope(), AssistantMemorySensitivity.Standard);
        var use = Assert.Single(service.Retrieve(new AssistantMemoryQuery("Zephyr summaries", Project: "Zephyr Quill", RecordUsage: true), Now.AddMinutes(1)));
        Assert.Contains("Memory used", use.Disclosure);
        Assert.Equal(1, Assert.Single(service.List()).UsageCount);
    }

    [Fact]
    public void Edit_creates_auditable_revision()
    {
        var service = Service(); var saved = Save(service, "Zephyr uses short summaries.", AssistantMemoryType.Preference, Scope(), AssistantMemorySensitivity.Standard);
        var edited = service.Edit(saved.MemoryId, "Formatting", "Zephyr uses short evidence-first summaries.", "Codie", "Clarified preference", Now.AddMinutes(1));
        var revision = Assert.Single(edited.Revisions);
        Assert.Equal("Zephyr uses short summaries.", revision.PreviousStatement);
    }

    [Fact]
    public void Delete_preserves_origin_and_creates_audit_tombstone()
    {
        var service = Service(); var saved = Save(service, "Zephyr uses short summaries.", AssistantMemoryType.Preference, Scope(), AssistantMemorySensitivity.Standard);
        var deleted = service.Delete(saved.MemoryId, "Codie", Now.AddMinutes(1));
        Assert.Equal(AssistantMemoryStatus.Deleted, deleted.Status);
        Assert.Equal("source-1", deleted.Origin.SourceId);
        Assert.Empty(service.Retrieve(new AssistantMemoryQuery("Zephyr summaries", Project: "Zephyr Quill"), Now.AddMinutes(2)));
    }

    [Fact]
    public void Sensitive_memory_requires_acknowledgement()
    {
        var service = Service();
        var proposal = service.Propose("Client", "Fictional client prefers Friday reviews.", AssistantMemoryType.RelationshipContext, new(AssistantMemoryScopeType.Relationship, "Fictional Client"), AssistantMemorySensitivity.PrivateClient, Origin(), now: Now);
        Assert.Throws<InvalidOperationException>(() => service.Confirm(proposal.ProposalId, "Codie", false, Now));
    }

    [Theory]
    [InlineData("password: super-secret")]
    [InlineData("client_secret=abc123")]
    [InlineData("access_token: abc123")]
    public void Secret_bearing_memory_is_rejected(string statement)
    {
        Assert.Throws<InvalidOperationException>(() => Propose(Service(), statement));
    }

    [Fact]
    public void Current_trusted_source_outranks_conflicting_memory()
    {
        var service = Service(); var saved = Save(service, "Zephyr status is Blocked.", AssistantMemoryType.Fact, Scope(), AssistantMemorySensitivity.Standard);
        var result = service.ResolveAgainstCurrentSource(saved, "Zephyr status is Active.");
        Assert.Equal("Zephyr status is Active.", result.Result);
        Assert.Contains("outranks", result.Reason);
    }

    [Fact]
    public void Session_limited_memory_is_never_durably_retrieved()
    {
        var service = Service(); Save(service, "Temporary formatting context.", AssistantMemoryType.Preference, new(AssistantMemoryScopeType.SessionLimited, "session-1"), AssistantMemorySensitivity.Standard);
        Assert.Empty(service.Retrieve(new AssistantMemoryQuery("Temporary formatting context"), Now));
    }

    private static AssistantMemoryService Service() => new(new InMemoryAssistantMemoryStore());
    private static AssistantMemoryScope Scope() => new(AssistantMemoryScopeType.Project, "Zephyr Quill");
    private static AssistantMemoryOrigin Origin() => new("AssistantAnswer", "source-1", "Fictional Zephyr answer", ["Projects/zephyr"], "Fictional approved source");
    private static ProposedAssistantMemory Propose(AssistantMemoryService service, string statement) => service.Propose("Formatting", statement, AssistantMemoryType.Preference, Scope(), AssistantMemorySensitivity.Standard, Origin(), now: Now);
    private static AssistantMemoryRecord Save(AssistantMemoryService service, string statement, AssistantMemoryType type, AssistantMemoryScope scope, AssistantMemorySensitivity sensitivity)
    {
        var proposal = service.Propose("Formatting", statement, type, scope, sensitivity, Origin(), Now.AddDays(30), now: Now);
        return service.Confirm(proposal.ProposalId, "Codie", true, Now);
    }
}
