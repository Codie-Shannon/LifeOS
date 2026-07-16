using Xunit;
using LifeOS.Core.IntegrationInbox;
using LifeOS.Core.MicrosoftProvider;

namespace LifeOS.Core.Tests;

public sealed class MicrosoftTeamsGroup50Tests
{
    [Fact]
    public void Selection_NormalizesAndBoundsValues()
    {
        MicrosoftTeamsSelection selection = new()
        {
            SelectedTeamIds = [" team-1 ", "TEAM-1", ""],
            SelectedChannelIds = [" channel-1 ", "channel-1"],
            LookbackDays = 0,
            MaximumMessagesPerChannel = 9999
        };

        selection.Normalize();

        Assert.Single(selection.SelectedTeamIds);
        Assert.Single(selection.SelectedChannelIds);
        Assert.Equal(1, selection.LookbackDays);
        Assert.Equal(500, selection.MaximumMessagesPerChannel);
    }

    [Fact]
    public void RequestedScopes_AreIncrementalAndReadOnly()
    {
        IReadOnlyList<string> scopes =
            MicrosoftProviderConfiguration.Group50RequestedScopes;

        Assert.Contains("Files.Read", scopes);
        Assert.Contains("Sites.Read.All", scopes);
        Assert.Contains("Team.ReadBasic.All", scopes);
        Assert.Contains("Channel.ReadBasic.All", scopes);
        Assert.Contains("ChannelMessage.Read.All", scopes);
        Assert.Contains("OnlineMeetings.Read", scopes);
        Assert.DoesNotContain(scopes, scope =>
            scope.Contains("ReadWrite", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(scopes, scope =>
            scope.Contains("Send", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void CandidateFactory_PreservesThreadProvenance()
    {
        MicrosoftProviderAccount account = new()
        {
            Id = "work",
            DisplayName = "Work"
        };
        MicrosoftTeamsMessageDescriptor message = new(
            "message-1",
            "team-1",
            "channel-1",
            "thread-1",
            "parent-1",
            "Author",
            "Subject",
            "Please send the document",
            DateTimeOffset.Parse("2026-07-17T00:00:00Z"),
            DateTimeOffset.Parse("2026-07-17T00:05:00Z"),
            "https://example.invalid/redacted",
            MicrosoftTeamsSourceState.Edited);

        MessageCandidateDraft draft =
            MicrosoftTeamsCandidateFactory.CreateMessageDraft(account, message);

        Assert.Equal("teams-messages", draft.CapabilityId);
        Assert.Equal("thread-1", draft.ConversationId);
        Assert.Equal(message.ModifiedUtc, draft.SourceTimestampUtc);
    }

    [Fact]
    public void ProofMigration_IsIdempotentAndReviewFirst()
    {
        DateTimeOffset now =
            DateTimeOffset.Parse("2026-07-17T08:00:00Z");
        IntegrationInboxV9State state =
            IntegrationInboxV9Seed.CreateFictional(now);

        Group50IntegrationInboxMigration.Apply(state, now);
        int firstCount = state.Candidates.Count;
        Group50IntegrationInboxMigration.Apply(state, now);

        Assert.Equal(firstCount, state.Candidates.Count);
        Assert.Contains(state.Candidates, candidate =>
            candidate.Id == "group50-teams-channel-message" &&
            candidate.Status == IntegrationCandidateStatus.NeedsReview);
        Assert.Contains(state.Candidates, candidate =>
            candidate.Id == "group50-teams-action-suggestion" &&
            candidate.Status == IntegrationCandidateStatus.NeedsReview);
        Assert.Contains(state.Candidates, candidate =>
            candidate.Id == "group50-teams-access-lost" &&
            candidate.Status == IntegrationCandidateStatus.SourceRemoved);
    }
}
