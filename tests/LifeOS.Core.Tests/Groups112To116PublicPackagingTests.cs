using LifeOS.Core.PublicPackaging;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Groups112To116PublicPackagingTests
{
    private readonly PublicPackagingService _service = new();

    [Fact]
    public void Public_package_excludes_private_and_internal_assets()
    {
        PackagingResult result = _service.Validate(new[]
        {
            Asset("product", PublicAssetAudience.Public),
            Asset("beta-notes", PublicAssetAudience.PrivateBeta),
            Asset("handoff", PublicAssetAudience.Internal)
        });

        Assert.True(result.Ready);
        Assert.Single(result.PublicAssets);
        Assert.Equal("product", result.PublicAssets[0].Id);
    }

    [Fact]
    public void Unsupported_public_claims_and_private_paths_fail_packaging()
    {
        PublicAsset unsafeAsset = Asset("unsafe", PublicAssetAudience.Public) with
        {
            Status = "private beta",
            Copy = @"Production-ready from C:\Projects\LifeOS private Codex handoff."
        };

        PackagingResult result = _service.Validate(new[] { unsafeAsset });

        Assert.False(result.Ready);
        Assert.Single(result.Errors);
    }

    [Fact]
    public void Honest_beta_case_study_can_be_packaged()
    {
        PublicAsset caseStudy = Asset("case-study", PublicAssetAudience.Public) with
        {
            Copy = "A private-beta case study covering the local-first architecture, review boundaries and verified test results."
        };

        Assert.True(_service.Validate(new[] { caseStudy }).Ready);
    }

    [Fact]
    public void Screenshot_paths_cannot_expose_private_local_locations()
    {
        PublicAsset unsafeAsset = Asset("screenshots", PublicAssetAudience.Public) with
        {
            ScreenshotPaths = new[] { @"C:\Users\Codie\Pictures\private.png" }
        };

        Assert.False(_service.Validate(new[] { unsafeAsset }).Ready);
    }

    [Fact]
    public void Duplicate_public_routes_fail_packaging()
    {
        PublicAsset first = Asset("first", PublicAssetAudience.Public) with { Route = "/case-study" };
        PublicAsset second = Asset("second", PublicAssetAudience.Public) with { Route = "/case-study" };

        Assert.False(_service.Validate(new[] { first, second }).Ready);
    }

    [Fact]
    public void Desktop_settings_exposes_the_public_packaging_route()
    {
        string repositoryRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        string shell = File.ReadAllText(
            Path.Combine(repositoryRoot, "LifeOS.Desktop", "V8ShellWindow.xaml"));

        Assert.Contains("Tag=\"public-packaging\"", shell, StringComparison.Ordinal);
        Assert.Contains(
            "AutomationProperties.AutomationId=\"Settings.Diagnostics.PublicPackaging\"",
            shell,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Public_pages_use_the_preferred_LifeOS_name()
    {
        string repositoryRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        string pages = Path.Combine(repositoryRoot, "src", "LifeOS.Website", "Pages");

        Assert.DoesNotContain(
            "Life Control OS",
            File.ReadAllText(Path.Combine(pages, "Onboarding.razor")),
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "Life Control OS",
            File.ReadAllText(Path.Combine(pages, "CaseStudy.razor")),
            StringComparison.Ordinal);
    }

    private static PublicAsset Asset(string id, PublicAssetAudience audience) =>
        new(id, id, $"/{id}", audience, "private beta", "Neutral factual copy.", Array.Empty<string>());
}
