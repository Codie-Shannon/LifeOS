using LifeOS.Website.Content;
using LifeOS.Website.Services;
using Xunit;

namespace LifeOS.Website.Tests;

public sealed class DocumentationContentTests
{
    [Fact]
    public void Public_documents_have_complete_unique_metadata()
    {
        Assert.True(PublicContent.Docs.Count >= 10);
        Assert.Equal(PublicContent.Docs.Count, PublicContent.Docs.Select(x => x.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Equal(PublicContent.Docs.Count, PublicContent.Docs.Select(x => x.Route).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(PublicContent.Docs, item =>
        {
            Assert.False(string.IsNullOrWhiteSpace(item.Id));
            Assert.StartsWith("/docs/", item.Route);
            Assert.Equal("Public", item.PublicStatus);
            Assert.NotEmpty(item.Sections);
            Assert.NotEqual(default, item.Updated);
        });
    }

    [Fact]
    public void Inventory_precedes_copy_reduction_and_has_required_fields()
    {
        Assert.True(PublicContent.DocumentationInventory.Count >= 10);
        Assert.All(PublicContent.DocumentationInventory, item =>
        {
            Assert.False(string.IsNullOrWhiteSpace(item.Source));
            Assert.False(string.IsNullOrWhiteSpace(item.CurrentLocation));
            Assert.False(string.IsNullOrWhiteSpace(item.Classification));
            Assert.StartsWith("/docs/", item.IntendedRoute);
        });
    }

    [Fact]
    public void Related_document_ids_resolve()
    {
        var service = new PublicContentService();
        Assert.All(PublicContent.Docs, item => Assert.Equal(item.RelatedIds.Length, service.Related(item).Count));
    }

    [Theory]
    [InlineData("assistant", "assistant")]
    [InlineData("offline", "troubleshooting-recovery")]
    [InlineData("review inbox", "integrations")]
    [InlineData("full mobile", "product-release-boundaries")]
    public void Search_is_deterministic_across_indexed_fields(string query, string expectedId)
    {
        var service = new PublicContentService();
        Assert.Contains(service.Search(query), x => x.Id == expectedId);
        Assert.Equal(service.Search(query).Select(x => x.Id), service.Search(query).Select(x => x.Id));
    }

    [Fact]
    public void User_documents_are_ordered_first_by_default()
    {
        var results = new PublicContentService().Search(null);
        var firstNonUser = results.ToList().FindIndex(x => x.Audience is not ("User" or "All"));
        var lastUser = results.ToList().FindLastIndex(x => x.Audience is "User" or "All");
        Assert.True(firstNonUser == -1 || lastUser < firstNonUser);
    }

    [Fact]
    public void Filters_cover_category_audience_and_product()
    {
        var service = new PublicContentService();
        Assert.All(service.Search(null, category: "Module"), x => Assert.Equal("Module", x.Category));
        Assert.All(service.Search(null, audience: "User"), x => Assert.Equal("User", x.Audience));
        Assert.All(service.Search(null, product: "Companion"), x => Assert.Equal("Companion", x.Product));
    }
}
