using LifeOS.Website.Content;
using Xunit;

namespace LifeOS.Website.Tests;

public sealed class WebsiteCompletionTests
{
    private static readonly string Root = FindRepositoryRoot();

    [Fact]
    public void Website_release_is_beta_checkpoint()
    {
        var project = File.ReadAllText(Path.Combine(Root, "src", "LifeOS.Website", "LifeOS.Website.csproj"));
        Assert.Contains("<Version>0.3.0-beta.1</Version>", project);
    }

    [Fact]
    public void Footer_and_public_routes_are_complete()
    {
        var footer = File.ReadAllText(Path.Combine(Root, "src", "LifeOS.Website", "Layout", "SiteFooter.razor"));
        foreach (var route in new[] { "/product/desktop", "/product/companion", "/docs", "/evidence", "/roadmap", "/about", "/privacy", "/access" }) Assert.Contains(route, footer);
        Assert.Contains(PublicContent.Routes, x => x.Path == "/downloads");
    }

    [Fact]
    public void Docs_have_empty_state_and_reset()
    {
        var search = File.ReadAllText(Path.Combine(Root, "src", "LifeOS.Website", "Components", "DocSearch.razor"));
        Assert.Contains("No public document matched", search);
        Assert.Contains("Reset search and filters", search);
    }

    [Fact]
    public void Seo_and_spa_fallback_assets_exist()
    {
        var root = Path.Combine(Root, "src", "LifeOS.Website", "wwwroot");
        Assert.True(File.Exists(Path.Combine(root, "robots.txt")));
        Assert.True(File.Exists(Path.Combine(root, "sitemap.xml")));
        Assert.True(File.Exists(Path.Combine(root, "_redirects")));
        Assert.True(File.Exists(Path.Combine(root, "social", "lifeos-social.svg")));
        Assert.Contains("/index.html 200", File.ReadAllText(Path.Combine(root, "_redirects")));
    }

    [Fact]
    public void No_download_control_or_binary_asset_is_published()
    {
        var website = Path.Combine(Root, "src", "LifeOS.Website");
        var textFiles = Directory.EnumerateFiles(website, "*", SearchOption.AllDirectories).Where(x => new[] { ".razor", ".html", ".css", ".js" }.Contains(Path.GetExtension(x))).ToArray();
        var text = string.Join("\n", textFiles.Select(File.ReadAllText));
        Assert.DoesNotContain(">Download<", text, StringComparison.OrdinalIgnoreCase);
        var binaries = Directory.EnumerateFiles(Path.Combine(website, "wwwroot"), "*", SearchOption.AllDirectories).Where(x => new[] { ".exe", ".msi", ".apk", ".zip" }.Contains(Path.GetExtension(x), StringComparer.OrdinalIgnoreCase));
        Assert.Empty(binaries);
    }

    [Fact]
    public void Design_system_is_locked_for_group_43_input()
    {
        var design = File.ReadAllText(Path.Combine(Root, "docs", "website", "design-system-v0.3-beta.md"));
        Assert.Contains("Purple is the approved primary accent", design);
        Assert.Contains("Later Desktop equivalent", design);
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null) { if (File.Exists(Path.Combine(current.FullName, "LifeOS.slnx"))) return current.FullName; current = current.Parent; }
        throw new DirectoryNotFoundException("LifeOS repository root not found.");
    }
}
