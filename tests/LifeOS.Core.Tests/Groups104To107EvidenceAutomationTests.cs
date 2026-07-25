using LifeOS.Core.EvidenceAutomation;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Groups104To107EvidenceAutomationTests
{
    private readonly EvidenceAutomationService _service = new();

    [Fact]
    public void Intake_excludes_old_screenshots_and_proposes_stable_names()
    {
        DateTimeOffset cutoff = new(2026, 7, 27, 8, 0, 0, TimeSpan.FromHours(12));
        IReadOnlyList<ScreenshotCandidate> candidates = _service.Classify(new[]
        {
            (@"C:\Pictures\Screenshots\old\legacy.png", cutoff.AddDays(-1)),
            (@"C:\Pictures\Screenshots\work time.png", cutoff.AddMinutes(1))
        }, cutoff, "SG-67");

        ScreenshotCandidate candidate = Assert.Single(candidates);
        Assert.False(candidate.IsOld);
        Assert.Equal(@"C:\Pictures\Screenshots\work time.png", candidate.SourcePath);
        Assert.Equal("01_SG-67_work-time.png", candidate.SuggestedName);
    }

    [Fact]
    public void Completion_gate_lists_every_missing_proof()
    {
        CompletionGateResult result = _service.Evaluate(new CompletionGateInput(
            true, 0, true, true, false, false, true, false));

        Assert.False(result.Passed);
        Assert.Contains("screenshots", result.Missing);
        Assert.Contains("mobile Release build", result.Missing);
        Assert.Contains("release notes", result.Missing);
        Assert.Contains("GitHub status", result.Missing);
    }

    [Fact]
    public void Completion_gate_passes_only_with_full_evidence()
    {
        CompletionGateResult result = _service.Evaluate(new CompletionGateInput(
            true, 8, true, true, true, true, true, true));

        Assert.True(result.Passed);
        Assert.Empty(result.Missing);
    }

    [Fact]
    public void Desktop_settings_exposes_the_evidence_automation_route()
    {
        string repositoryRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        string shell = File.ReadAllText(
            Path.Combine(repositoryRoot, "LifeOS.Desktop", "V8ShellWindow.xaml"));

        Assert.Contains("Content=\"Evidence Automation\"", shell, StringComparison.Ordinal);
        Assert.Contains("Tag=\"evidence-automation\"", shell, StringComparison.Ordinal);
        Assert.Contains(
            "AutomationProperties.AutomationId=\"Settings.Diagnostics.EvidenceAutomation\"",
            shell,
            StringComparison.Ordinal);
    }
}
