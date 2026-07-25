using LifeOS.Core.BetaOnboarding;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Groups80To82BetaOnboardingTests
{
    private readonly BetaOnboardingService _service = new();

    [Fact]
    public void Optional_integrations_can_be_configured_later_or_declined()
    {
        SetupModule integration = new(
            "google",
            "Google Workspace",
            false,
            true,
            SetupChoice.SetupNow,
            "Settings > Integrations");

        SetupModule later = _service.Choose(integration, SetupChoice.Later);
        SetupModule declined = _service.Choose(integration, SetupChoice.Declined);

        Assert.Equal(SetupChoice.Later, later.Choice);
        Assert.Equal(SetupChoice.Declined, declined.Choice);
        Assert.Contains("Settings", declined.ChangePath);
    }

    [Fact]
    public void Closed_beta_gate_reports_each_failed_check()
    {
        BetaReadiness readiness = _service.Evaluate(
            new[] { Module("local", true, SetupChoice.SetupNow) },
            new[]
            {
                new BetaReadinessCheck("desktop", "Desktop Release build", true, "Passed"),
                new BetaReadinessCheck("mobile", "Mobile Release build", false, "Not run")
            });

        Assert.False(readiness.Ready);
        Assert.Contains("Mobile Release build", readiness.Blockers);
    }

    [Fact]
    public void Declining_an_optional_provider_does_not_block_local_beta()
    {
        BetaReadiness readiness = _service.Evaluate(
            new[]
            {
                Module("local", true, SetupChoice.SetupNow),
                Module("ai", false, SetupChoice.Declined)
            },
            new[] { new BetaReadinessCheck("tests", "Tests", true, "Passing") });

        Assert.True(readiness.Ready);
    }

    [Fact]
    public void Desktop_settings_exposes_private_beta_readiness_through_normal_navigation()
    {
        string repositoryRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        string shellPath = Path.Combine(repositoryRoot, "LifeOS.Desktop", "V8ShellWindow.xaml");
        string shell = File.ReadAllText(shellPath);

        Assert.Contains("Content=\"Private Beta Readiness\"", shell, StringComparison.Ordinal);
        Assert.Contains("Tag=\"beta-readiness\"", shell, StringComparison.Ordinal);
        Assert.Contains(
            "AutomationProperties.AutomationId=\"Settings.Diagnostics.BetaReadiness\"",
            shell,
            StringComparison.Ordinal);
    }

    private static SetupModule Module(string id, bool core, SetupChoice choice) =>
        new(id, id, core, false, choice, "Settings");
}
