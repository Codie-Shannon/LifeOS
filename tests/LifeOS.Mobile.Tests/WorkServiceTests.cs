using LifeOS.Mobile.Core.Work;
using Xunit;

namespace LifeOS.Mobile.Tests;

public sealed class WorkServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 17, 10, 0, 0, TimeSpan.FromHours(12));

    [Fact]
    public void DashboardContainsRequiredWorkClassifications()
    {
        var snapshot = new WorkService().BuildSnapshot(Now);

        Assert.Contains(
            snapshot.Items,
            x => x.Status == MobileWorkStatus.Active);

        Assert.Contains(
            snapshot.Items,
            x => x.Status == MobileWorkStatus.Waiting);

        Assert.Contains(
            snapshot.Items,
            x => x.Status == MobileWorkStatus.Blocked);

        Assert.Contains(
            snapshot.Items,
            x => x.Status == MobileWorkStatus.DueSoon);
    }

    [Fact]
    public void WorkItemsContainNextActionAndEvidenceSummary()
    {
        var snapshot = new WorkService().BuildSnapshot(Now);

        Assert.All(
            snapshot.Items,
            x => Assert.False(
                string.IsNullOrWhiteSpace(x.NextAction)));

        Assert.All(
            snapshot.Items,
            x => Assert.InRange(
                x.EvidenceComplete,
                0,
                x.EvidenceTotal));
    }

    [Fact]
    public void CommunicationCandidatesExposeSafeProvenance()
    {
        var snapshot = new WorkService().BuildSnapshot(Now);

        Assert.All(
            snapshot.Communications,
            x => Assert.Contains(
                "payload excluded",
                x.Provenance,
                StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ReviewActionsAreExplicitAndIdempotent()
    {
        var service = new WorkService();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => service.ReviewCommunication(
                "candidate",
                CommunicationReviewState.PendingReview,
                Now));

        var first = service.ReviewCommunication(
            "candidate",
            CommunicationReviewState.Accepted,
            Now);

        var second = service.ReviewCommunication(
            "candidate",
            CommunicationReviewState.Accepted,
            Now.AddMinutes(5));

        Assert.Same(first, second);
    }

    [Fact]
    public void ConflictCandidateRemainsVisible()
    {
        var snapshot = new WorkService().BuildSnapshot(Now);

        Assert.Contains(
            snapshot.Communications,
            x => x.ReviewState ==
                CommunicationReviewState.Conflict);
    }

    [Fact]
    public void FilterIsDeterministic()
    {
        var service = new WorkService();
        var snapshot = service.BuildSnapshot(Now);

        var first = service.FilterItems(
            snapshot,
            MobileWorkStatus.Active,
            "proof");

        var second = service.FilterItems(
            snapshot,
            MobileWorkStatus.Active,
            "proof");

        Assert.Equal(
            first.Select(x => x.Id),
            second.Select(x => x.Id));
    }

    [Fact]
    public void FollowUpMovesItemToWaitingWithoutProviderMutation()
    {
        var service = new WorkService();
        var item = service.BuildSnapshot(Now).Items[0];

        var updated = service.CaptureFollowUp(
            item,
            "Wait for explicit review");

        Assert.Equal(
            MobileWorkStatus.Waiting,
            updated.Status);

        Assert.Equal(
            "Wait for explicit review",
            updated.NextAction);
    }

    [Fact]
    public void EvidenceUsesMetadataOnly()
    {
        var snapshot = new WorkService().BuildSnapshot(Now);

        Assert.All(
            snapshot.Evidence,
            x => Assert.False(
                string.IsNullOrWhiteSpace(x.Source)));
    }

    [Fact]
    public void MeetingContextContainsExplicitCapturedActions()
    {
        var snapshot = new WorkService().BuildSnapshot(Now);

        Assert.NotEmpty(snapshot.Meetings);
        Assert.NotEmpty(snapshot.Meetings[0].CapturedActions);
    }

    [Fact]
    public void NoCommunicationCandidateClaimsSendCapability()
    {
        var snapshot = new WorkService().BuildSnapshot(Now);

        Assert.DoesNotContain(
            snapshot.Communications,
            x => x.SafePreview.Contains(
                "send",
                StringComparison.OrdinalIgnoreCase));
    }
}
