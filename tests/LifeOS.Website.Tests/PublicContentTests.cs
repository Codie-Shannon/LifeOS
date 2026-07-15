using Xunit;
using LifeOS.Website.Content;
using LifeOS.Website.Services;
namespace LifeOS.Website.Tests;
public sealed class PublicContentTests
{
    [Fact] public void Required_routes_are_unique_and_present()
    {
        string[] required = ["/", "/product", "/product/desktop", "/product/companion", "/solutions", "/solutions/businesses", "/solutions/individuals", "/solutions/technical-reviewers", "/safety", "/docs", "/docs/guides", "/docs/concepts", "/docs/modules", "/docs/releases", "/evidence", "/roadmap", "/access", "/about", "/privacy"];
        Assert.Equal(PublicContent.Routes.Count, PublicContent.Routes.Select(x => x.Path).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(required, route => Assert.Contains(PublicContent.Routes, x => x.Path == route));
    }
    [Fact] public void Product_boundaries_are_explicit()
    {
        Assert.Contains(PublicContent.Products, x => x.Name == "Desktop" && x.State == "Beta complete");
        Assert.Contains(PublicContent.Products, x => x.Name == "Mobile Companion" && x.State == "Beta complete");
        Assert.Contains(PublicContent.Products, x => x.Name == "Full Mobile" && x.State == "Planned");
    }
    [Fact] public void Docs_have_required_metadata()
    {
        Assert.NotEmpty(PublicContent.Docs);
        Assert.All(PublicContent.Docs, x => { Assert.False(string.IsNullOrWhiteSpace(x.Title)); Assert.False(string.IsNullOrWhiteSpace(x.Summary)); Assert.False(string.IsNullOrWhiteSpace(x.Audience)); Assert.False(string.IsNullOrWhiteSpace(x.Category)); Assert.False(string.IsNullOrWhiteSpace(x.Product)); Assert.False(string.IsNullOrWhiteSpace(x.Version)); Assert.NotEqual(default, x.Updated); });
    }
    [Theory] [InlineData("safety", "Safety and trust model")] [InlineData("desktop state", "Desktop operating model")] [InlineData("release", "Release history")]
    public void Search_is_deterministic(string query, string expectedTitle)
    {
        var service = new PublicContentService(); var first = service.Search(query).Select(x => x.Title).ToArray(); var second = service.Search(query).Select(x => x.Title).ToArray();
        Assert.Equal(first, second); Assert.Contains(expectedTitle, first);
    }
    [Fact] public void Primary_message_is_approved() => Assert.Equal("A local-first personal operating system that turns work, money, projects, evidence and daily pressure into visible, reviewable state.", PublicContent.PrimaryMessage);
}
