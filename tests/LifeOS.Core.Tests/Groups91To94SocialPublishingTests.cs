using LifeOS.Core.SocialPublishing;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Groups91To94SocialPublishingTests
{
    private readonly SocialPublishingService _service = new();

    [Fact]
    public void Official_api_is_preferred_when_available()
    {
        SocialCapability capability = Capability(SocialDestination.FacebookPage, true, true);

        Assert.Equal(SocialDeliveryPath.OfficialApi, _service.ResolvePath(capability));
    }

    [Fact]
    public void Browser_assisted_path_is_explicit_when_api_is_unavailable()
    {
        SocialDraft draft = _service.Draft(
            Capability(SocialDestination.FacebookGroup, false, true),
            "Fictional community group",
            "Fictional update",
            DateTimeOffset.Now.AddDays(1),
            "fictional://project/update");

        Assert.Equal(SocialDeliveryPath.BrowserAssisted, draft.Path);
        Assert.Equal(SocialDraftState.Draft, draft.State);
    }

    [Fact]
    public void Editing_a_social_post_revokes_approval()
    {
        SocialDraft approved = _service.Approve(Draft(), DateTimeOffset.Now);

        SocialDraft edited = _service.Edit(approved, "Revised fictional update");

        Assert.Equal(SocialDraftState.Draft, edited.State);
        Assert.Null(edited.ApprovedAt);
    }

    [Fact]
    public void Provider_confirmation_is_required_before_marking_published()
    {
        SocialDraft approved = _service.Approve(Draft(), DateTimeOffset.Now);

        Assert.Throws<InvalidOperationException>(() => _service.MarkPublished(approved, false));
        Assert.Equal(SocialDraftState.Published, _service.MarkPublished(approved, true).State);
    }

    private SocialDraft Draft() =>
        _service.Draft(
            Capability(SocialDestination.Messenger, true, false),
            "Fictional recipient",
            "Fictional message",
            DateTimeOffset.Now.AddHours(2),
            "fictional://message/source");

    private static SocialCapability Capability(
        SocialDestination destination,
        bool api,
        bool browser) =>
        new(destination, api, browser, api ? "" : "Provider API does not support this target.");
}
