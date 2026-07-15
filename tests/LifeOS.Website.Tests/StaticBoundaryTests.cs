using Xunit;
namespace LifeOS.Website.Tests;
public sealed class StaticBoundaryTests
{
    private static readonly string Root = FindRepositoryRoot();
    [Fact] public void Public_assets_contain_no_tracking_scripts()
    {
        var wwwroot = Path.Combine(Root, "src", "LifeOS.Website", "wwwroot");
        var text = string.Join(Environment.NewLine, Directory.EnumerateFiles(wwwroot, "*", SearchOption.AllDirectories).Where(x => new[]{".html",".js",".xml",".txt"}.Contains(Path.GetExtension(x))).Select(File.ReadAllText));
        string[] forbidden = ["google-analytics", "googletagmanager", "gtag(", "segment.io", "mixpanel", "hotjar", "facebook pixel", "clarity.ms"];
        Assert.All(forbidden, item => Assert.DoesNotContain(item, text, StringComparison.OrdinalIgnoreCase));
    }
    [Fact] public void Waitlist_page_is_disabled_and_links_privacy()
    {
        var text = File.ReadAllText(Path.Combine(Root, "src", "LifeOS.Website", "Pages", "Access.razor"));
        Assert.Contains("disabled", text); Assert.Contains("/privacy", text); Assert.Contains("no collection", text, StringComparison.OrdinalIgnoreCase);
    }
    [Fact] public void Privacy_page_matches_group_40_boundary()
    {
        var text = File.ReadAllText(Path.Combine(Root, "src", "LifeOS.Website", "Pages", "Privacy.razor"));
        Assert.Contains("no analytics", text, StringComparison.OrdinalIgnoreCase); Assert.Contains("no account", text, StringComparison.OrdinalIgnoreCase); Assert.Contains("not uploaded", text, StringComparison.OrdinalIgnoreCase); Assert.Contains("fictional or sanitized", text, StringComparison.OrdinalIgnoreCase);
    }
    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null) { if (File.Exists(Path.Combine(current.FullName, "LifeOS.slnx"))) return current.FullName; current = current.Parent; }
        throw new DirectoryNotFoundException("LifeOS repository root not found.");
    }
}
