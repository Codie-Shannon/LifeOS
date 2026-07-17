using LifeOS.Mobile.Core.Beta;

namespace LifeOS.Mobile.Views;

public sealed class BetaPerformancePage : ContentPage
{
    public BetaPerformancePage(
        BetaClosureService service,
        PerformanceSnapshot performance)
    {
        Title = "Performance";
        BackgroundColor = BetaVisuals.Background;

        var passed = service.IsPerformanceWithinBetaBudget(performance);

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 20,
                Spacing = 14,
                Children =
                {
                    new Label
                    {
                        Text = "Performance budget",
                        FontSize = 30,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White
                    },
                    BetaVisuals.CardView(
                        "Startup",
                        $"{performance.StartupMilliseconds} ms",
                        performance.StartupMilliseconds <= 1500
                            ? "Passed"
                            : "Review"),
                    BetaVisuals.CardView(
                        "Home render",
                        $"{performance.HomeRenderMilliseconds} ms"),
                    BetaVisuals.CardView(
                        "Work render",
                        $"{performance.WorkRenderMilliseconds} ms"),
                    BetaVisuals.CardView(
                        "Money render",
                        $"{performance.MoneyRenderMilliseconds} ms"),
                    BetaVisuals.CardView(
                        "Life render",
                        $"{performance.LifeRenderMilliseconds} ms"),
                    BetaVisuals.CardView(
                        "Projects render",
                        $"{performance.ProjectsRenderMilliseconds} ms"),
                    BetaVisuals.CardView(
                        "Managed memory",
                        $"{performance.ManagedMemoryBytes / 1_000_000d:N1} MB"),
                    BetaVisuals.CardView(
                        "Beta budget",
                        passed
                            ? "Within conservative local demo budget"
                            : "Requires review",
                        passed ? "Passed" : "Review")
                }
            }
        };
    }
}
