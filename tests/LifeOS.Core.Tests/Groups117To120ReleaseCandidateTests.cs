using LifeOS.Core.ReleaseCandidate;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Groups117To120ReleaseCandidateTests
{
    private readonly ReleaseCandidateService _service = new();

    [Fact]
    public void Missing_surface_check_blocks_product_complete_claim()
    {
        ReleaseCandidateDecision decision = _service.Evaluate(
            "27.0.0-rc.1",
            "release/product-complete",
            new[] { Check(ReleaseSurface.Desktop, true) });

        Assert.False(decision.Ready);
        Assert.Contains(decision.Blockers, blocker => blocker.StartsWith("Mobile:", StringComparison.Ordinal));
        Assert.Contains(decision.Blockers, blocker => blocker.StartsWith("Website:", StringComparison.Ordinal));
    }

    [Fact]
    public void Failing_check_is_reported_with_its_surface()
    {
        ReleaseSurfaceCheck[] checks = AllPassing();
        checks[1] = Check(ReleaseSurface.Mobile, false, "Android Release build failed");

        ReleaseCandidateDecision decision = _service.Evaluate("27.0.0-rc.1", "release/product-complete", checks);

        Assert.False(decision.Ready);
        Assert.Contains("Mobile: End-to-end pass", decision.Blockers);
    }

    [Fact]
    public void Fully_verified_candidate_still_requires_human_tag_approval()
    {
        ReleaseCandidateDecision decision = _service.Evaluate(
            "27.0.0-rc.1",
            "release/product-complete",
            AllPassing());

        Assert.True(decision.Ready);
        Assert.True(decision.RequiresHumanApproval);
        Assert.Equal("v27.0.0-rc.1", decision.ProposedTag);
        Assert.Throws<InvalidOperationException>(() => _service.ApproveTag(decision, false));
        _service.ApproveTag(decision, true);
    }

    [Fact]
    public void Candidate_review_is_reachable_across_desktop_mobile_and_website()
    {
        string repositoryRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

        string desktopShell = File.ReadAllText(
            Path.Combine(repositoryRoot, "LifeOS.Desktop", "V8ShellWindow.xaml"));
        string mobileMore = File.ReadAllText(
            Path.Combine(repositoryRoot, "src", "LifeOS.Mobile", "Views", "MorePage.cs"));
        string websitePage = File.ReadAllText(
            Path.Combine(repositoryRoot, "src", "LifeOS.Website", "Pages", "ReleaseCandidate.razor"));

        Assert.Contains("Tag=\"release-candidate\"", desktopShell, StringComparison.Ordinal);
        Assert.Contains("new ProductCompletePage()", mobileMore, StringComparison.Ordinal);
        Assert.Contains("@page \"/release-candidate\"", websitePage, StringComparison.Ordinal);
        Assert.DoesNotContain("Life Control OS", websitePage, StringComparison.Ordinal);
    }

    private static ReleaseSurfaceCheck[] AllPassing() =>
        Enum.GetValues<ReleaseSurface>()
            .Select(surface => Check(surface, true))
            .ToArray();

    private static ReleaseSurfaceCheck Check(
        ReleaseSurface surface,
        bool passed,
        string evidence = "Verified") =>
        new(surface, "End-to-end pass", passed, evidence);
}
