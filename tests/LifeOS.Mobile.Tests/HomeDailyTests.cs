using LifeOS.Mobile.Core.Home;
using LifeOS.Mobile.Core.Services;
using LifeOS.Mobile.Core.Storage;
using System.Security.Cryptography;
using Xunit;

namespace LifeOS.Mobile.Tests;

public sealed class HomeDailyTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 17, 10, 0, 0, TimeSpan.FromHours(12));

    [Fact]
    public void TodayAggregationCoversMultipleDomains()
    {
        var overview = new HomeDailyService().BuildOverview(Now);

        Assert.Contains(overview.Priorities, x => x.Workspace == "Work");
        Assert.Contains(overview.Priorities, x => x.Workspace == "Money");
        Assert.Contains(overview.Priorities, x => x.Workspace == "Projects");
        Assert.Contains(overview.Priorities, x => x.Workspace == "Life");
    }

    [Fact]
    public void PriorityOrderingIsDeterministicAndExplainable()
    {
        var service = new HomeDailyService();

        var first = service.BuildOverview(Now).Priorities;
        var second = service.BuildOverview(Now).Priorities;

        Assert.Equal(
            first.Select(x => x.Id),
            second.Select(x => x.Id));

        Assert.Equal(
            [1, 2, 3, 4],
            first.Select(x => x.Rank));

        Assert.All(
            first,
            x => Assert.False(
                string.IsNullOrWhiteSpace(x.Explanation)));
    }

    [Fact]
    public void QuickCaptureNormalizesDraftWithoutSavingImplicitly()
    {
        var draft = new HomeDailyService().CreateDraft(
            MobileCaptureKind.ProjectIdea,
            "  mobile   proof\nidea  ",
            Now);

        Assert.Equal("mobile proof idea", draft.Text);
        Assert.Equal(MobileCaptureKind.ProjectIdea, draft.Kind);
    }

    [Fact]
    public void EmptyQuickCaptureIsRejected()
    {
        Assert.Throws<ArgumentException>(
            () => new HomeDailyService().CreateDraft(
                MobileCaptureKind.Note,
                "   ",
                Now));
    }

    [Fact]
    public async Task OfflineQuickCaptureQueuesIdempotently()
    {
        var directory = Path.Combine(
            Path.GetTempPath(),
            "lifeos-mobile-group53",
            Guid.NewGuid().ToString("N"));

        var path = Path.Combine(directory, "store.bin");

        try
        {
            var store = new JsonMobileLocalStore(
                path,
                RandomNumberGenerator.GetBytes(32));

            await store.InitializeAsync();

            var foundation = new MobileFoundationService(store);
            var draft = new HomeDailyService().CreateDraft(
                MobileCaptureKind.Task,
                "Review fictional proof",
                Now);

            await foundation.QueueQuickCaptureAsync(draft);
            await foundation.QueueQuickCaptureAsync(draft);

            var outbox = await store.LoadOutboxAsync();
            var sync = await store.LoadSyncAsync();

            Assert.Single(outbox);
            Assert.Equal(1, sync.PendingCount);
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }
    }

    [Fact]
    public void CompleteActionIsIdempotent()
    {
        var service = new HomeDailyService();

        var first = service.Complete("item-1", Now);
        var second = service.Complete("item-1", Now.AddMinutes(5));

        Assert.Same(first, second);
    }

    [Fact]
    public void DeferRequiresFutureTimeAndIsIdempotent()
    {
        var service = new HomeDailyService();
        var until = Now.AddDays(1);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => service.Defer("item-1", Now, Now));

        var first = service.Defer("item-1", until, Now);
        var second = service.Defer(
            "item-1",
            until,
            Now.AddMinutes(2));

        Assert.Same(first, second);
    }

    [Fact]
    public void WaitingAndReviewCountsRemainExplicit()
    {
        var overview = new HomeDailyService().BuildOverview(Now);

        Assert.Equal(2, overview.Waiting.WaitingOn);
        Assert.Equal(1, overview.Waiting.Blocked);
        Assert.Equal(1, overview.Waiting.Overdue);
        Assert.Equal(3, overview.Waiting.NeedsReview);
        Assert.Equal(3, overview.Review.IntegrationInboxCount);
    }

    [Fact]
    public void UpcomingTimesConvertToRequestedTimezone()
    {
        var item = new HomeUpcomingItem(
            "test",
            "Test item",
            "Calendar",
            new DateTimeOffset(
                2026,
                7,
                17,
                0,
                0,
                0,
                TimeSpan.Zero),
            "UTC");

        var formatted = HomeDailyService.FormatLocalTime(
            item,
            TimeZoneInfo.Utc);

        Assert.Contains("12:00", formatted);
    }

    [Fact]
    public void OverviewContainsNoAutonomousDecisionClaim()
    {
        var overview = new HomeDailyService().BuildOverview(Now);

        Assert.DoesNotContain(
            overview.Priorities,
            x => x.Explanation.Contains(
                "automatically",
                StringComparison.OrdinalIgnoreCase));
    }
}

