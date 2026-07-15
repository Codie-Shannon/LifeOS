using LifeOS.Website.Content;
using Xunit;

namespace LifeOS.Website.Tests;

public sealed class PublicSafetyTests
{
    [Fact]
    public void Public_content_does_not_expose_private_patterns()
    {
        var text = string.Join("\n", PublicContent.Docs.SelectMany(x => new[] { x.Title, x.Summary, x.Route }.Concat(x.Keywords).Concat(x.Sections.SelectMany(s => s.Paragraphs))));
        foreach (var forbidden in new[] { "C:\\Projects", "client_secret", "token-cache", "@gmail.com", "localhost:", "192.168." })
            Assert.DoesNotContain(forbidden, text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Every_document_route_is_registered()
    {
        var routes = PublicContent.Routes.Select(x => x.Path).ToHashSet(StringComparer.OrdinalIgnoreCase);
        Assert.All(PublicContent.Docs, item => Assert.Contains(item.Route, routes));
    }
}
