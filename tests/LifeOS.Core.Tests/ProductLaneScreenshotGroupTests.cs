using LifeOS.Core.ProductCompletion;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class ProductLaneScreenshotGroupTests
{
    [Theory]
    [InlineData("work-time", "SG-68")]
    [InlineData("operating-day", "SG-72")]
    [InlineData("guarded-providers", "SG-76")]
    [InlineData("documentation-hub", "SG-79")]
    [InlineData("beta-readiness", "SG-82")]
    [InlineData("intelligence", "SG-86")]
    [InlineData("communications", "SG-90")]
    [InlineData("social-publishing", "SG-94")]
    [InlineData("pay-later-insights", "SG-98")]
    [InlineData("grocery-lookup", "SG-103")]
    [InlineData("evidence-automation", "SG-107")]
    [InlineData("control-plane", "SG-111")]
    [InlineData("public-packaging", "SG-116")]
    [InlineData("release-candidate", "SG-120")]
    public void Compressed_product_lanes_use_the_ending_group_as_the_screenshot_group(
        string route,
        string expectedScreenshotGroup)
    {
        Assert.Equal(expectedScreenshotGroup, ProductLaneCatalog.Get(route).ScreenshotGroup);
    }
}
