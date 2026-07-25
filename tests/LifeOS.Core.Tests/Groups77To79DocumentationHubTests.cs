using LifeOS.Core.DocumentationHub;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Groups77To79DocumentationHubTests
{
    private readonly DocumentationHubService _service = new();

    [Fact]
    public void Public_search_never_returns_internal_notes()
    {
        DocumentationEntry[] entries =
        {
            Entry("help", DocumentationAudience.Public),
            Entry("beta", DocumentationAudience.BetaTester),
            Entry("handoff", DocumentationAudience.Internal)
        };

        IReadOnlyList<DocumentationEntry> results = _service.Search(entries, "", DocumentationAudience.Public);

        Assert.Single(results);
        Assert.Equal("help", results[0].Id);
    }

    [Fact]
    public void App_keeps_only_concise_help_entries()
    {
        DocumentationEntry concise = Entry("quick-help", DocumentationAudience.Public, true);
        DocumentationEntry longForm = Entry("architecture", DocumentationAudience.Public, false);

        IReadOnlyList<DocumentationEntry> results = _service.AppHelp(new[] { longForm, concise });

        Assert.Single(results);
        Assert.Equal("quick-help", results[0].Id);
    }

    [Fact]
    public void Public_boundary_rejects_machine_paths_and_private_handoff_copy()
    {
        DocumentationEntry unsafeEntry = Entry("unsafe", DocumentationAudience.Public) with
        {
            Summary = @"Private Codex notes from C:\Projects\LifeOS"
        };

        Assert.Throws<InvalidOperationException>(() => _service.ValidatePublicBoundary(unsafeEntry));
    }

    [Fact]
    public void Desktop_settings_exposes_documentation_hub_through_normal_navigation()
    {
        string repositoryRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        string shellPath = Path.Combine(repositoryRoot, "LifeOS.Desktop", "V8ShellWindow.xaml");
        string shell = File.ReadAllText(shellPath);

        Assert.Contains("Content=\"Documentation Hub\"", shell, StringComparison.Ordinal);
        Assert.Contains("Tag=\"documentation-hub\"", shell, StringComparison.Ordinal);
        Assert.Contains(
            "AutomationProperties.AutomationId=\"Settings.Diagnostics.DocumentationHub\"",
            shell,
            StringComparison.Ordinal);
    }

    private static DocumentationEntry Entry(
        string id,
        DocumentationAudience audience,
        bool concise = false) =>
        new(id, id, $"Documentation for {id}", $"/docs/{id}", audience, "v15", new[] { "help" }, concise);
}
